using System.Buffers;
using System.Numerics;
using System.Linq;
using FrinkyEngine.Core.Animation.IK;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Rendering;
using Raylib_cs;

namespace FrinkyEngine.Core.Components;

/// <summary>
/// Plays skeletal animation clips for a sibling <see cref="MeshRendererComponent"/> using GPU skinning.
/// Supports frame interpolation and can preview in editor viewport rendering.
/// </summary>
[ComponentCategory("Rendering")]
[ComponentDisplayName("Skinned Mesh Animator")]
public sealed unsafe class SkinnedMeshAnimatorComponent : Component
{
    private const float DefaultAnimationFps = 60f;

    private MeshRendererComponent? _meshRenderer;
    private int _lastModelVersion;
    private unsafe ModelAnimation* _animations;
    private int _animationCount;

    private int _clipIndex;
    private float _playheadFrames;
    private double _lastSampleTime = -1d;
    private bool _playbackInitialized;
    private ulong _lastPreparedRenderToken;

    private Matrix4x4[][]? _bindPose;
    private Matrix4x4[][]? _poseFrameA;
    private Matrix4x4[][]? _poseFrameB;
    private Matrix4x4[][]? _poseLerped;
    private bool _hasSkinnedMeshes;

    // Current model-space bone transforms, updated each PrepareForRender for bone preview.
    private (Vector3 t, Quaternion r, Vector3 s)[]? _currentModelPose;

    // IK pose buffers (allocated on demand when IK component is present)
    private (Vector3 t, Quaternion r, Vector3 s)[]? _ikModelPoseA;
    private (Vector3 t, Quaternion r, Vector3 s)[]? _ikModelPoseB;
    private (Vector3 t, Quaternion r, Vector3 s)[]? _ikModelPose;
    private (Vector3 t, Quaternion r, Vector3 s)[]? _ikLocalPoseA;
    private (Vector3 t, Quaternion r, Vector3 s)[]? _ikLocalPoseB;
    private (Vector3 t, Quaternion r, Vector3 s)[]? _ikLocalPose;
    private int[]? _ikParentIndices;
    private Matrix4x4[]? _ikWorldMatrices;

    /// <summary>
    /// Whether playback starts automatically once a valid clip is available.
    /// </summary>
    public bool PlayAutomatically { get; set; } = true;

    /// <summary>
    /// Whether playback loops when reaching the end of the selected clip.
    /// </summary>
    public bool Loop { get; set; } = true;

    /// <summary>
    /// Whether playback advances over time.
    /// </summary>
    public bool Playing { get; set; } = true;

    /// <summary>
    /// Playback speed multiplier where 1.0 is normal speed.
    /// </summary>
    [InspectorRange(0f, 4f, 0.01f)]
    public float PlaybackSpeed { get; set; } = 1f;

    /// <summary>
    /// Animation sample rate in frames per second.
    /// </summary>
    [InspectorRange(1f, 120f, 1f)]
    public float AnimationFps { get; set; } = DefaultAnimationFps;

    /// <summary>
    /// Selected animation clip index.
    /// </summary>
    [InspectorDropdown(nameof(GetActionNames))]
    [InspectorOnChanged(nameof(OnClipIndexChanged))]
    public int ClipIndex
    {
        get => _clipIndex;
        set => _clipIndex = Math.Max(0, value);
    }

    /// <summary>
    /// Name of the currently selected animation action.
    /// </summary>
    [InspectorReadOnly]
    public string ActionName => GetAnimationName(ResolveClipIndex());

    /// <summary>
    /// Number of animation actions loaded for the current model.
    /// </summary>
    [InspectorReadOnly]
    public int ActionCount => _animationCount;

    /// <summary>
    /// Frame count of the currently selected animation clip.
    /// </summary>
    [InspectorReadOnly]
    public int FrameCount
    {
        get
        {
            int clip = ResolveClipIndex();
            return clip < 0 ? 0 : _animations[clip].FrameCount;
        }
    }

    /// <summary>
    /// Resets playback time to clip start.
    /// </summary>
    [InspectorButton("Restart")]
    public void Restart()
    {
        ResetPlayhead();
        Playing = true;
    }

    /// <summary>
    /// Stops playback and applies bind pose.
    /// </summary>
    [InspectorButton("Stop")]
    public void StopAndResetPose()
    {
        Playing = false;
        ResetPlayhead();
        ApplyBindPose();
    }

    /// <inheritdoc />
    public override void Start()
    {
        ResetPlayhead();
        _playbackInitialized = false;
    }

    /// <inheritdoc />
    public override void Awake()
    {
        EnsureMeshRendererReference();
    }

    /// <inheritdoc />
    public override void OnDestroy()
    {
        if (_meshRenderer != null)
            _meshRenderer.SetRequireUniqueModelInstance(false);
    }

    internal void PrepareForRender(ulong renderToken)
    {
        if (!Enabled)
            return;

        EnsureMeshRendererReference();
        if (_meshRenderer == null)
            return;

        _meshRenderer.SetRequireUniqueModelInstance(true);
        _meshRenderer.EnsureModelReady();
        if (!_meshRenderer.RenderModel.HasValue)
            return;

        if (!EnsureAnimationState())
            return;

        if (_lastPreparedRenderToken == renderToken)
            return;

        _lastPreparedRenderToken = renderToken;

        var model = _meshRenderer.RenderModel.Value;

        if (!RenderRuntimeCvars.AnimationEnabled)
        {
            // Prevent a large dt jump when animation is re-enabled.
            _lastSampleTime = -1d;
            ApplyBindPose();
            CaptureCurrentModelPose(model);
            return;
        }

        var ikComponent = Entity.GetComponent<InverseKinematicsComponent>();
        bool hasActiveIk = ikComponent != null && model.BoneCount > 0 && ikComponent.HasRunnableSolvers(model);
        var activeIk = hasActiveIk ? ikComponent : null;

        int clip = ResolveClipIndex();

        // "(none)" selected — use bind pose, optionally with IK
        if (clip < 0)
        {
            if (activeIk != null)
            {
                EnsureIkPoseBuffers(model);
                CopyBindPoseToModel(model, _ikModelPose!);
                ConvertModelPoseToLocal(_ikModelPose!, _ikLocalPose!, _ikParentIndices!);
                activeIk.ApplyIK(_ikLocalPose!, model, Entity.Transform.WorldMatrix, _ikWorldMatrices!);
                ConvertLocalPoseToModel(_ikLocalPose!, _ikModelPose!, _ikParentIndices!);
                ComputeSkinningMatrices(model, _ikModelPose!);
                CaptureCurrentModelPose(_ikModelPose!);
            }
            else
            {
                ApplyBindPose();
                CaptureCurrentModelPose(model);
            }
            return;
        }

        if (!Playing)
        {
            // Pose is already applied; keep _currentModelPose from last update.
            return;
        }

        var now = Raylib.GetTime();
        if (_lastSampleTime < 0d)
        {
            _lastSampleTime = now;
        }

        float dt = (float)Math.Max(0d, now - _lastSampleTime);
        _lastSampleTime = now;

        unsafe
        {
            var animation = _animations[clip];
            if (animation.FrameCount <= 0 || !Raylib.IsModelAnimationValid(model, animation))
                return;

            var speed = float.IsFinite(PlaybackSpeed) ? Math.Max(0f, PlaybackSpeed) : 1f;
            _playheadFrames += dt * speed * AnimationFps;

            int frameCount = Math.Max(1, animation.FrameCount);
            if (Loop)
            {
                _playheadFrames %= frameCount;
                if (_playheadFrames < 0f)
                    _playheadFrames += frameCount;
            }
            else
            {
                float maxFrame = Math.Max(0f, frameCount - 1);
                if (_playheadFrames >= maxFrame)
                {
                    _playheadFrames = maxFrame;
                    Playing = false;
                }
            }

            int frameA = (int)MathF.Floor(_playheadFrames);
            int frameB = Loop
                ? (frameA + 1) % frameCount
                : Math.Min(frameA + 1, frameCount - 1);
            float alpha = Math.Clamp(_playheadFrames - frameA, 0f, 1f);

            // IK path: sample as local transforms, apply IK, then compute skinning matrices
            if (activeIk != null)
            {
                EnsureIkPoseBuffers(model);
                SampleModelPose(animation, frameA, _ikModelPoseA!);
                ConvertModelPoseToLocal(_ikModelPoseA!, _ikLocalPoseA!, _ikParentIndices!);

                if (alpha > 0f && frameA != frameB)
                {
                    SampleModelPose(animation, frameB, _ikModelPoseB!);
                    ConvertModelPoseToLocal(_ikModelPoseB!, _ikLocalPoseB!, _ikParentIndices!);
                    LerpTransformPose(_ikLocalPoseA!, _ikLocalPoseB!, _ikLocalPose!, alpha);
                }
                else
                {
                    Array.Copy(_ikLocalPoseA!, _ikLocalPose!, _ikLocalPoseA!.Length);
                }

                activeIk.ApplyIK(_ikLocalPose!, model, Entity.Transform.WorldMatrix, _ikWorldMatrices!);
                ConvertLocalPoseToModel(_ikLocalPose!, _ikModelPose!, _ikParentIndices!);
                ComputeSkinningMatrices(model, _ikModelPose!);
                CaptureCurrentModelPose(_ikModelPose!);
                return;
            }

            // Fast path: no IK — use existing matrix-based sampling
            SampleFramePose(model, animation, frameA, _poseFrameA!);
            if (alpha <= 0f || frameA == frameB)
            {
                ApplyPose(_poseFrameA!);
                CaptureInterpolatedModelPose(model, animation, frameA, frameA, 0f);
                return;
            }

            SampleFramePose(model, animation, frameB, _poseFrameB!);
            LerpPose(_poseFrameA!, _poseFrameB!, _poseLerped!, alpha);
            ApplyPose(_poseLerped!);
            CaptureInterpolatedModelPose(model, animation, frameA, frameB, alpha);
        }
    }

    internal bool UsesSkinning => _hasSkinnedMeshes;

    /// <summary>
    /// Returns the current model-space bone transforms after animation has been applied.
    /// Each element contains the translation, rotation and scale for that bone index.
    /// Returns an empty span when no animation state is available.
    /// </summary>
    public ReadOnlySpan<(Vector3 t, Quaternion r, Vector3 s)> CurrentModelPose =>
        _currentModelPose is not null ? _currentModelPose.AsSpan() : ReadOnlySpan<(Vector3, Quaternion, Vector3)>.Empty;

    private bool EnsureAnimationState()
    {
        if (_meshRenderer == null || !_meshRenderer.RenderModel.HasValue)
            return false;

        if (_meshRenderer.ModelVersion != _lastModelVersion)
        {
            ResetAnimationState();
            _lastModelVersion = _meshRenderer.ModelVersion;
            _animations = AssetManager.Instance.LoadModelAnimations(_meshRenderer.ModelPath.Path, out _animationCount);
            _playbackInitialized = false;
        }

        var model = _meshRenderer.RenderModel.Value;
        if (!PoseShapeMatchesModel(model))
            ResetPoseBuffersOnly();
        CaptureBindPoseIfNeeded(model);
        if (_bindPose == null || _bindPose.Length == 0)
            return false;

        if (!_playbackInitialized)
        {
            Playing = PlayAutomatically;
            _playbackInitialized = true;
        }

        int clip = ResolveClipIndex();
        if (clip >= 0)
        {
            unsafe
            {
                var animation = _animations[clip];
                if (Raylib.IsModelAnimationValid(model, animation))
                    return true;
            }
        }

        // clip < 0 means "(none)" selected — still valid if IK component is present
        var ikComponent = Entity.GetComponent<InverseKinematicsComponent>();
        if (clip < 0 && ikComponent != null && ikComponent.HasRunnableSolvers(model))
            return true;

        ApplyBindPose();
        return false;
    }

    private int ResolveClipIndex()
    {
        // _clipIndex 0 = "(none)" dropdown entry → resolved clip -1
        int resolved = _clipIndex - 1;

        if (_animations == null || _animationCount <= 0)
            return -1;

        if (resolved < 0)
            return -1;

        return Math.Clamp(resolved, 0, _animationCount - 1);
    }

    private void CaptureBindPoseIfNeeded(Model model)
    {
        if (_bindPose != null)
            return;

        _bindPose = CaptureCurrentPose(model);
        _poseFrameA = ClonePoseShape(_bindPose);
        _poseFrameB = ClonePoseShape(_bindPose);
        _poseLerped = ClonePoseShape(_bindPose);
        _hasSkinnedMeshes = _bindPose.Any(static x => x.Length > 0);
    }

    private static Matrix4x4[][] CaptureCurrentPose(Model model)
    {
        unsafe
        {
            int meshCount = Math.Max(0, model.MeshCount);
            var pose = new Matrix4x4[meshCount][];

            for (int i = 0; i < meshCount; i++)
            {
                var mesh = model.Meshes[i];
                int boneCount = mesh.BoneCount;
                if (boneCount <= 0 || mesh.BoneMatrices == null)
                {
                    pose[i] = Array.Empty<Matrix4x4>();
                    continue;
                }

                var data = new Matrix4x4[boneCount];
                for (int b = 0; b < boneCount; b++)
                    data[b] = mesh.BoneMatrices[b];
                pose[i] = data;
            }

            return pose;
        }
    }

    private bool PoseShapeMatchesModel(Model model)
    {
        if (_bindPose == null)
            return false;
        if (_bindPose.Length != model.MeshCount)
            return false;

        unsafe
        {
            for (int i = 0; i < model.MeshCount; i++)
            {
                int expected = model.Meshes[i].BoneCount;
                if (_bindPose[i].Length != Math.Max(0, expected))
                    return false;
            }
        }

        return true;
    }

    private static Matrix4x4[][] ClonePoseShape(Matrix4x4[][] source)
    {
        var clone = new Matrix4x4[source.Length][];
        for (int i = 0; i < source.Length; i++)
            clone[i] = new Matrix4x4[source[i].Length];
        return clone;
    }

    private unsafe void SampleFramePose(Model model, ModelAnimation animation, int frame, Matrix4x4[][] target)
    {
        Raylib.UpdateModelAnimationBones(model, animation, frame);
        CopyCurrentPoseInto(model, target);
    }

    private static void CopyCurrentPoseInto(Model model, Matrix4x4[][] target)
    {
        unsafe
        {
            for (int i = 0; i < model.MeshCount && i < target.Length; i++)
            {
                var mesh = model.Meshes[i];
                var outArr = target[i];
                if (mesh.BoneMatrices == null || outArr.Length == 0)
                    continue;

                int count = Math.Min(mesh.BoneCount, outArr.Length);
                for (int b = 0; b < count; b++)
                    outArr[b] = mesh.BoneMatrices[b];
            }
        }
    }

    private static void LerpPose(Matrix4x4[][] a, Matrix4x4[][] b, Matrix4x4[][] output, float alpha)
    {
        float beta = 1f - alpha;

        for (int i = 0; i < output.Length; i++)
        {
            int count = output[i].Length;
            for (int m = 0; m < count; m++)
            {
                var ma = a[i][m];
                var mb = b[i][m];
                output[i][m] = new Matrix4x4(
                    ma.M11 * beta + mb.M11 * alpha, ma.M12 * beta + mb.M12 * alpha, ma.M13 * beta + mb.M13 * alpha, ma.M14 * beta + mb.M14 * alpha,
                    ma.M21 * beta + mb.M21 * alpha, ma.M22 * beta + mb.M22 * alpha, ma.M23 * beta + mb.M23 * alpha, ma.M24 * beta + mb.M24 * alpha,
                    ma.M31 * beta + mb.M31 * alpha, ma.M32 * beta + mb.M32 * alpha, ma.M33 * beta + mb.M33 * alpha, ma.M34 * beta + mb.M34 * alpha,
                    ma.M41 * beta + mb.M41 * alpha, ma.M42 * beta + mb.M42 * alpha, ma.M43 * beta + mb.M43 * alpha, ma.M44 * beta + mb.M44 * alpha);
            }
        }
    }

    private void ApplyBindPose()
    {
        if (_bindPose == null)
            return;

        ApplyPose(_bindPose);
    }

    private void ApplyPose(Matrix4x4[][] pose)
    {
        if (_meshRenderer == null || !_meshRenderer.RenderModel.HasValue)
            return;

        var model = _meshRenderer.RenderModel.Value;
        unsafe
        {
            for (int i = 0; i < model.MeshCount && i < pose.Length; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh.BoneMatrices == null)
                    continue;

                var data = pose[i];
                int count = Math.Min(mesh.BoneCount, data.Length);
                for (int b = 0; b < count; b++)
                    mesh.BoneMatrices[b] = data[b];
            }
        }
    }

    private unsafe void EnsureIkPoseBuffers(Model model)
    {
        int boneCount = model.BoneCount;
        if (_ikLocalPose != null && _ikLocalPose.Length == boneCount)
            return;

        _ikModelPoseA = new (Vector3, Quaternion, Vector3)[boneCount];
        _ikModelPoseB = new (Vector3, Quaternion, Vector3)[boneCount];
        _ikModelPose = new (Vector3, Quaternion, Vector3)[boneCount];
        _ikLocalPoseA = new (Vector3, Quaternion, Vector3)[boneCount];
        _ikLocalPoseB = new (Vector3, Quaternion, Vector3)[boneCount];
        _ikLocalPose = new (Vector3, Quaternion, Vector3)[boneCount];
        _ikWorldMatrices = new Matrix4x4[boneCount];

        // Cache parent indices (stable for a given model)
        _ikParentIndices = new int[boneCount];
        for (int i = 0; i < boneCount; i++)
            _ikParentIndices[i] = model.Bones[i].Parent;
    }

    private static unsafe void SampleModelPose(
        ModelAnimation animation, int frame,
        (Vector3 t, Quaternion r, Vector3 s)[] target)
    {
        int boneCount = Math.Min(animation.BoneCount, target.Length);
        frame = Math.Clamp(frame, 0, Math.Max(0, animation.FrameCount - 1));
        var framePoses = animation.FramePoses[frame];
        for (int i = 0; i < boneCount; i++)
        {
            var t = framePoses[i];
            target[i] = (t.Translation, t.Rotation, t.Scale);
        }
    }

    private static void LerpTransformPose(
        (Vector3 t, Quaternion r, Vector3 s)[] a,
        (Vector3 t, Quaternion r, Vector3 s)[] b,
        (Vector3 t, Quaternion r, Vector3 s)[] output,
        float alpha)
    {
        int count = Math.Min(a.Length, Math.Min(b.Length, output.Length));
        for (int i = 0; i < count; i++)
        {
            output[i] = (
                Vector3.Lerp(a[i].t, b[i].t, alpha),
                Quaternion.Slerp(a[i].r, b[i].r, alpha),
                Vector3.Lerp(a[i].s, b[i].s, alpha));
        }
    }

    private static unsafe void CopyBindPoseToModel(
        Model model,
        (Vector3 t, Quaternion r, Vector3 s)[] target)
    {
        int count = Math.Min(model.BoneCount, target.Length);
        for (int i = 0; i < count; i++)
        {
            var t = model.BindPose[i];
            target[i] = (t.Translation, t.Rotation, t.Scale);
        }
    }

    /// <summary>
    /// Converts per-bone model-space transforms to local-space transforms using parent indices.
    /// Input and output arrays must NOT alias (the forward loop reads parent entries
    /// that may already be overwritten when parent index &gt; child index).
    /// </summary>
    private static void ConvertModelPoseToLocal(
        (Vector3 t, Quaternion r, Vector3 s)[] modelPose,
        (Vector3 t, Quaternion r, Vector3 s)[] localPose,
        int[] parentIndices)
    {
        int count = Math.Min(modelPose.Length, Math.Min(localPose.Length, parentIndices.Length));
        for (int i = 0; i < count; i++)
        {
            int parent = parentIndices[i];
            var model = modelPose[i];
            if (parent < 0 || parent >= count)
            {
                localPose[i] = model;
                continue;
            }

            var parentModel = modelPose[parent];
            var invParentRotation = Raymath.QuaternionInvert(parentModel.r);
            var localRotation = Quaternion.Normalize(Raymath.QuaternionMultiply(invParentRotation, model.r));
            var localScale = ComponentDivide(model.s, parentModel.s);
            var delta = Raymath.Vector3Subtract(model.t, parentModel.t);
            var unrotated = Raymath.Vector3RotateByQuaternion(delta, invParentRotation);
            var localTranslation = ComponentDivide(unrotated, parentModel.s);
            localPose[i] = (localTranslation, localRotation, localScale);
        }
    }

    /// <summary>
    /// Converts per-bone local-space transforms to model-space transforms using parent indices.
    /// Input and output arrays may alias.
    /// </summary>
    private static void ConvertLocalPoseToModel(
        (Vector3 t, Quaternion r, Vector3 s)[] localPose,
        (Vector3 t, Quaternion r, Vector3 s)[] modelPose,
        int[] parentIndices)
    {
        int count = Math.Min(localPose.Length, Math.Min(modelPose.Length, parentIndices.Length));
        if (count <= 0)
            return;

        // Handle arbitrary bone ordering; do not assume parent index < child index.
        var visitState = ArrayPool<byte>.Shared.Rent(count); // 0=unvisited, 1=visiting, 2=done
        Array.Clear(visitState, 0, count);
        for (int i = 0; i < count; i++)
            ComputeModel(i);
        ArrayPool<byte>.Shared.Return(visitState);

        void ComputeModel(int boneIndex)
        {
            if (visitState[boneIndex] == 2)
                return;
            if (visitState[boneIndex] == 1)
            {
                // Cycle guard: fall back to local transform.
                modelPose[boneIndex] = localPose[boneIndex];
                visitState[boneIndex] = 2;
                return;
            }

            visitState[boneIndex] = 1;

            int parent = parentIndices[boneIndex];
            var local = localPose[boneIndex];
            if (parent >= 0 && parent < count)
            {
                ComputeModel(parent);
                var parentModel = modelPose[parent];
                var modelRotation = Quaternion.Normalize(Raymath.QuaternionMultiply(parentModel.r, local.r));
                var modelScale = Raymath.Vector3Multiply(parentModel.s, local.s);
                var scaledTranslation = Raymath.Vector3Multiply(local.t, parentModel.s);
                var modelTranslation = Raymath.Vector3Add(
                    Raymath.Vector3RotateByQuaternion(scaledTranslation, parentModel.r),
                    parentModel.t);
                modelPose[boneIndex] = (modelTranslation, modelRotation, modelScale);
            }
            else
            {
                modelPose[boneIndex] = local;
            }

            visitState[boneIndex] = 2;
        }
    }

    /// <summary>
    /// Replicates Raylib's UpdateModelAnimationBones algorithm:
    /// boneMatrix = inverse(bindMatrix) * targetMatrix (both in model-space).
    /// </summary>
    private unsafe void ComputeSkinningMatrices(
        Model model,
        (Vector3 t, Quaternion r, Vector3 s)[] modelPose)
    {
        if (_meshRenderer == null || !_meshRenderer.RenderModel.HasValue)
            return;

        int boneCount = model.BoneCount;
        int firstMeshWithBones = -1;
        for (int i = 0; i < model.MeshCount; i++)
        {
            var mesh = model.Meshes[i];
            if (mesh.BoneMatrices != null && mesh.BoneCount > 0)
            {
                firstMeshWithBones = i;
                break;
            }
        }
        if (firstMeshWithBones < 0)
            return;

        var firstMesh = model.Meshes[firstMeshWithBones];
        int firstCount = Math.Min(firstMesh.BoneCount, Math.Min(boneCount, modelPose.Length));
        for (int b = 0; b < firstCount; b++)
        {
            var bind = model.BindPose[b];
            var target = modelPose[b];

            // Match Raylib's UpdateModelAnimationBones implementation exactly:
            // bind = MatrixMultiply(MatrixMultiply(MatrixScale, QuaternionToMatrix), MatrixTranslate)
            var bindMatrix = Raymath.MatrixMultiply(
                Raymath.MatrixMultiply(
                    Raymath.MatrixScale(bind.Scale.X, bind.Scale.Y, bind.Scale.Z),
                    Raymath.QuaternionToMatrix(bind.Rotation)),
                Raymath.MatrixTranslate(bind.Translation.X, bind.Translation.Y, bind.Translation.Z));

            var targetMatrix = Raymath.MatrixMultiply(
                Raymath.MatrixMultiply(
                    Raymath.MatrixScale(target.s.X, target.s.Y, target.s.Z),
                    Raymath.QuaternionToMatrix(target.r)),
                Raymath.MatrixTranslate(target.t.X, target.t.Y, target.t.Z));

            firstMesh.BoneMatrices[b] = Raymath.MatrixMultiply(
                Raymath.MatrixInvert(bindMatrix),
                targetMatrix);
        }

        for (int m = firstMeshWithBones + 1; m < model.MeshCount; m++)
        {
            var mesh = model.Meshes[m];
            if (mesh.BoneMatrices == null || mesh.BoneCount <= 0)
                continue;

            int count = Math.Min(mesh.BoneCount, firstCount);
            for (int b = 0; b < count; b++)
                mesh.BoneMatrices[b] = firstMesh.BoneMatrices[b];
        }
    }

    private unsafe void CaptureCurrentModelPose(Model model)
    {
        int boneCount = model.BoneCount;
        if (boneCount <= 0)
        {
            _currentModelPose = null;
            return;
        }

        if (_currentModelPose == null || _currentModelPose.Length != boneCount)
            _currentModelPose = new (Vector3, Quaternion, Vector3)[boneCount];

        for (int i = 0; i < boneCount; i++)
        {
            var t = model.BindPose[i];
            _currentModelPose[i] = (t.Translation, t.Rotation, t.Scale);
        }
    }

    private void CaptureCurrentModelPose((Vector3 t, Quaternion r, Vector3 s)[] modelPose)
    {
        int count = modelPose.Length;
        if (count <= 0)
        {
            _currentModelPose = null;
            return;
        }

        if (_currentModelPose == null || _currentModelPose.Length != count)
            _currentModelPose = new (Vector3, Quaternion, Vector3)[count];

        Array.Copy(modelPose, _currentModelPose, count);
    }

    private unsafe void CaptureInterpolatedModelPose(Model model, ModelAnimation animation, int frameA, int frameB, float alpha)
    {
        int boneCount = Math.Min(model.BoneCount, animation.BoneCount);
        if (boneCount <= 0)
        {
            _currentModelPose = null;
            return;
        }

        if (_currentModelPose == null || _currentModelPose.Length != boneCount)
            _currentModelPose = new (Vector3, Quaternion, Vector3)[boneCount];

        int clampedA = Math.Clamp(frameA, 0, Math.Max(0, animation.FrameCount - 1));
        int clampedB = Math.Clamp(frameB, 0, Math.Max(0, animation.FrameCount - 1));
        var posesA = animation.FramePoses[clampedA];
        var posesB = animation.FramePoses[clampedB];

        for (int i = 0; i < boneCount; i++)
        {
            var a = posesA[i];
            var b = posesB[i];
            _currentModelPose[i] = (
                Vector3.Lerp(a.Translation, b.Translation, alpha),
                Quaternion.Slerp(a.Rotation, b.Rotation, alpha),
                Vector3.Lerp(a.Scale, b.Scale, alpha));
        }
    }

    private static Vector3 ComponentDivide(Vector3 value, Vector3 divisor)
    {
        return new Vector3(
            divisor.X != 0f ? value.X / divisor.X : 0f,
            divisor.Y != 0f ? value.Y / divisor.Y : 0f,
            divisor.Z != 0f ? value.Z / divisor.Z : 0f);
    }

    private void EnsureMeshRendererReference()
    {
        _meshRenderer ??= Entity.GetComponent<MeshRendererComponent>();
    }

    private void ResetAnimationState()
    {
        _animations = null;
        _animationCount = 0;
        ResetPlayhead();
        ResetPoseBuffersOnly();
    }

    private void ResetPoseBuffersOnly()
    {
        _bindPose = null;
        _poseFrameA = null;
        _poseFrameB = null;
        _poseLerped = null;
        _ikModelPoseA = null;
        _ikModelPoseB = null;
        _ikModelPose = null;
        _ikLocalPoseA = null;
        _ikLocalPoseB = null;
        _ikLocalPose = null;
        _ikParentIndices = null;
        _ikWorldMatrices = null;
        _hasSkinnedMeshes = false;
        _currentModelPose = null;
    }

    private void ResetPlayhead()
    {
        _playheadFrames = 0f;
        _lastSampleTime = -1d;
        _lastPreparedRenderToken = 0;
    }

    private void OnClipIndexChanged() => ResetPlayhead();

    private string GetAnimationName(int index)
    {
        if (index < 0 || _animations == null || index >= _animationCount)
            return "(none)";

        var name = new string(_animations[index].Name, 0, 32).TrimEnd('\0');
        return string.IsNullOrWhiteSpace(name) ? $"Action {index}" : name;
    }

    private string[] GetActionNames()
    {
        if (_animationCount <= 0 || _animations == null)
            return ["(none)"];

        var names = new string[_animationCount + 1];
        names[0] = "(none)";
        for (int i = 0; i < _animationCount; i++)
            names[i + 1] = GetAnimationName(i);
        return names;
    }
}
