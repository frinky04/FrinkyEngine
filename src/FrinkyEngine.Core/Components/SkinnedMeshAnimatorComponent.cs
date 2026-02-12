using System.Numerics;
using System.Linq;
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

        if (!RenderRuntimeCvars.AnimationEnabled)
        {
            // Prevent a large dt jump when animation is re-enabled.
            _lastSampleTime = -1d;
            ApplyBindPose();
            return;
        }

        if (!Playing)
            return;

        int clip = ResolveClipIndex();
        if (clip < 0)
            return;

        var now = Raylib.GetTime();
        if (_lastSampleTime < 0d)
        {
            _lastSampleTime = now;
        }

        float dt = (float)Math.Max(0d, now - _lastSampleTime);
        _lastSampleTime = now;

        var model = _meshRenderer.RenderModel.Value;
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

            SampleFramePose(model, animation, frameA, _poseFrameA!);
            if (alpha <= 0f || frameA == frameB)
            {
                ApplyPose(_poseFrameA!);
                return;
            }

            SampleFramePose(model, animation, frameB, _poseFrameB!);
            LerpPose(_poseFrameA!, _poseFrameB!, _poseLerped!, alpha);
            ApplyPose(_poseLerped!);
        }
    }

    internal bool UsesSkinning => _hasSkinnedMeshes;

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

        ApplyBindPose();
        return false;
    }

    private int ResolveClipIndex()
    {
        if (_animationCount <= 0 || _animations == null)
            return -1;

        return Math.Clamp(_clipIndex, 0, _animationCount - 1);
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
        _hasSkinnedMeshes = false;
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
            return ["(no animations)"];

        var names = new string[_animationCount];
        for (int i = 0; i < _animationCount; i++)
            names[i] = GetAnimationName(i);
        return names;
    }
}
