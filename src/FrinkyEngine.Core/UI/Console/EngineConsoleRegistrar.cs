using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.Audio;
using AudioApi = FrinkyEngine.Core.Audio.Audio;
using FrinkyEngine.Core.Physics;
using FrinkyEngine.Core.Rendering;
using FrinkyEngine.Core.Rendering.Profiling;
using FrinkyEngine.Core.Scene;
using Raylib_cs;

namespace FrinkyEngine.Core.UI.ConsoleSystem;

/// <summary>
/// Registers engine-provided developer-console commands and cvars.
/// </summary>
public static class EngineConsoleRegistrar
{
    /// <summary>
    /// Context callbacks required for registrations that touch overlay-local state.
    /// </summary>
    public readonly struct RegistrationContext(
        Action clearConsoleHistory,
        Func<string> getStatsModeValue,
        Func<string, bool> trySetStatsModeValue)
    {
        /// <summary>
        /// Clears console output history.
        /// </summary>
        public Action ClearConsoleHistory { get; } = clearConsoleHistory ?? throw new ArgumentNullException(nameof(clearConsoleHistory));

        /// <summary>
        /// Gets stats overlay mode value for the <c>r_showstats</c> cvar.
        /// </summary>
        public Func<string> GetStatsModeValue { get; } = getStatsModeValue ?? throw new ArgumentNullException(nameof(getStatsModeValue));

        /// <summary>
        /// Attempts to set stats overlay mode value for the <c>r_showstats</c> cvar.
        /// </summary>
        public Func<string, bool> TrySetStatsModeValue { get; } = trySetStatsModeValue ?? throw new ArgumentNullException(nameof(trySetStatsModeValue));
    }

    /// <summary>
    /// Registers all engine commands and cvars. Safe when called once per process.
    /// </summary>
    /// <param name="context">Registration context for overlay-bound actions.</param>
    public static void RegisterAll(RegistrationContext context)
    {
        ConsoleBackend.EnsureBuiltinsRegistered();

        // --- Rendering cvars ---
        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_postprocess",
            "r_postprocess [0|1]",
            "Enable or disable post-processing in runtime and Play/Simulate (1=on, 0=off).",
            RenderRuntimeCvars.GetPostProcessingValue,
            RenderRuntimeCvars.TrySetPostProcessing));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_autoinstancing",
            "r_autoinstancing [0|1]",
            "Enable or disable automatic model/primitive instancing (1=on, 0=off).",
            RenderRuntimeCvars.GetAutoInstancingValue,
            RenderRuntimeCvars.TrySetAutoInstancing));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_animation",
            "r_animation [0|1]",
            "Enable or disable skeletal animation playback for skinned meshes (1=on, 0=off).",
            RenderRuntimeCvars.GetAnimationValue,
            RenderRuntimeCvars.TrySetAnimation));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_showstats",
            "r_showstats [0-3]",
            "Stats overlay mode (0=off, 1=fps, 2=advanced, 3=verbose).",
            context.GetStatsModeValue,
            context.TrySetStatsModeValue));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_profiler",
            "r_profiler [0|1]",
            "Enable or disable the frame profiler (1=on, 0=off).",
            () => FrameProfiler.Enabled ? "1" : "0",
            value =>
            {
                if (!TryParseBool01(value, out var enabled))
                    return false;
                FrameProfiler.Enabled = enabled;
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_ambient",
            "r_ambient <r> <g> <b>",
            "Override default ambient light (0-1 per channel). Use 'r_ambient default' to reset.",
            RenderRuntimeCvars.GetAmbientValue,
            RenderRuntimeCvars.TrySetAmbient));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_maxfps",
            "r_maxfps [0-500]",
            "Set target FPS (0 = uncapped).",
            RenderRuntimeCvars.GetTargetFpsValue,
            value =>
            {
                if (!RenderRuntimeCvars.TrySetTargetFps(value))
                    return false;
                Raylib.SetTargetFPS(RenderRuntimeCvars.TargetFps);
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_screenpercentage",
            "r_screenpercentage [10-200]",
            "Screen percentage for resolution scaling (100=native, 50=half res pixelated, 200=supersampled).",
            RenderRuntimeCvars.GetScreenPercentageValue,
            RenderRuntimeCvars.TrySetScreenPercentage));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "r_vsync",
            "r_vsync [0|1]",
            "Toggle VSync (1=on, 0=off).",
            () => Raylib.IsWindowState(ConfigFlags.VSyncHint) ? "1" : "0",
            value =>
            {
                if (!TryParseBool01(value, out var enabled))
                    return false;
                if (enabled)
                    Raylib.SetWindowState(ConfigFlags.VSyncHint);
                else
                    Raylib.ClearWindowState(ConfigFlags.VSyncHint);
                return true;
            }));

        // --- Audio cvars ---
        RegisterVolumeCVar("snd_master", "Master audio bus volume.",
            () => AudioProjectSettings.Current.MasterVolume,
            v => { AudioProjectSettings.Current.MasterVolume = v; AudioApi.SetBusVolume(AudioBusId.Master, v); });
        RegisterVolumeCVar("snd_music", "Music bus volume.",
            () => AudioProjectSettings.Current.MusicVolume,
            v => { AudioProjectSettings.Current.MusicVolume = v; AudioApi.SetBusVolume(AudioBusId.Music, v); });
        RegisterVolumeCVar("snd_sfx", "SFX bus volume.",
            () => AudioProjectSettings.Current.SfxVolume,
            v => { AudioProjectSettings.Current.SfxVolume = v; AudioApi.SetBusVolume(AudioBusId.Sfx, v); });
        RegisterVolumeCVar("snd_ui", "UI bus volume.",
            () => AudioProjectSettings.Current.UiVolume,
            v => { AudioProjectSettings.Current.UiVolume = v; AudioApi.SetBusVolume(AudioBusId.Ui, v); });
        RegisterVolumeCVar("snd_voice", "Voice bus volume.",
            () => AudioProjectSettings.Current.VoiceVolume,
            v => { AudioProjectSettings.Current.VoiceVolume = v; AudioApi.SetBusVolume(AudioBusId.Voice, v); });
        RegisterVolumeCVar("snd_ambient", "Ambient bus volume.",
            () => AudioProjectSettings.Current.AmbientVolume,
            v => { AudioProjectSettings.Current.AmbientVolume = v; AudioApi.SetBusVolume(AudioBusId.Ambient, v); });

        // --- Time cvars ---
        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "time_scale",
            "time_scale [0.0-10.0]",
            "Game time scale (1.0 = normal, 0.5 = half speed, 2.0 = double speed, 0 = paused).",
            () => Scene.Scene.TimeScale.ToString("F2"),
            value =>
            {
                if (!TryParseFloatClamped(value, 0f, 10f, out var v))
                    return false;
                Scene.Scene.TimeScale = v;
                return true;
            }));

        // --- Physics cvars ---
        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "physics_gravity",
            "physics_gravity <x> <y> <z>",
            "Scene gravity vector.",
            () =>
            {
                var scene = SceneManager.Instance.ActiveScene;
                if (scene == null)
                    return "no scene";
                var g = scene.PhysicsSettings.Gravity;
                return $"{g.X:F2} {g.Y:F2} {g.Z:F2}";
            },
            value =>
            {
                var scene = SceneManager.Instance.ActiveScene;
                if (scene == null)
                    return false;
                var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                    return false;
                if (!float.TryParse(parts[0], out var x) ||
                    !float.TryParse(parts[1], out var y) ||
                    !float.TryParse(parts[2], out var z))
                    return false;
                if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(z))
                    return false;
                scene.PhysicsSettings.Gravity = new Vector3(x, y, z);
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "physics_timestep",
            "physics_timestep [value]",
            "Fixed physics simulation timestep in seconds.",
            () => PhysicsProjectSettings.Current.FixedTimestep.ToString("F4"),
            value =>
            {
                if (!TryParseFloatClamped(value, 1f / 240f, 1f / 15f, out var v))
                    return false;
                PhysicsProjectSettings.Current.FixedTimestep = v;
                return true;
            }));

        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            "physics_interpolation",
            "physics_interpolation [0|1]",
            "Enable or disable physics interpolation (1=on, 0=off).",
            () => PhysicsProjectSettings.Current.InterpolationEnabled ? "1" : "0",
            value =>
            {
                if (!TryParseBool01(value, out var enabled))
                    return false;
                PhysicsProjectSettings.Current.InterpolationEnabled = enabled;
                return true;
            }));

        // --- Commands ---
        ConsoleBackend.RegisterCommand("clear", "clear", "Clear console history.",
            _ =>
            {
                context.ClearConsoleHistory();
                return new ConsoleExecutionResult(true, new[] { "Console cleared." });
            });

        ConsoleBackend.RegisterCommand("scene", "scene", "Display active scene info.",
            _ =>
            {
                var scene = SceneManager.Instance.ActiveScene;
                if (scene == null)
                    return new ConsoleExecutionResult(true, new[] { "No active scene." });

                var lines = new List<string>
                {
                    $"Scene: {scene.Name}",
                    $"Entities: {scene.Entities.Count}",
                    $"Cameras: {scene.Cameras.Count}",
                    $"Lights: {scene.Lights.Count}"
                };
                return new ConsoleExecutionResult(true, lines);
            });

        ConsoleBackend.RegisterCommand("echo", "echo <text>", "Print text to console.",
            args =>
            {
                var text = args.Count > 0 ? string.Join(' ', args) : string.Empty;
                return new ConsoleExecutionResult(true, new[] { text });
            });

        ConsoleBackend.RegisterCommand("clear_log", "clear_log", "Clear all log entries.",
            _ =>
            {
                FrinkyLog.Clear();
                return new ConsoleExecutionResult(true, new[] { "Log cleared." });
            });

        ConsoleBackend.RegisterCommand("debug_print", "debug_print <text>", "Display a debug message on screen.",
            args =>
            {
                if (args.Count == 0)
                    return new ConsoleExecutionResult(false, new[] { "Usage: debug_print <text>" });
                var text = string.Join(' ', args);
                DebugDraw.PrintString(text);
                return new ConsoleExecutionResult(true, new[] { $"Debug: {text}" });
            });

        ConsoleBackend.RegisterCommand("quit", "quit", "Exit the application.",
            _ =>
            {
                Raylib.CloseWindow();
                return new ConsoleExecutionResult(true, new[] { "Quitting..." });
            });

        ConsoleBackend.RegisterCommand("open_scene", "open_scene <name|path>", "Load a .fscene file by name or path.",
            args =>
            {
                if (args.Count == 0)
                    return new ConsoleExecutionResult(false, new[] { "Usage: open_scene <name|path>" });

                var input = string.Join(' ', args);
                var scene = SceneManager.Instance.LoadSceneByName(input);
                return scene != null
                    ? new ConsoleExecutionResult(true, new[] { $"Loaded scene: {scene.FilePath}" })
                    : new ConsoleExecutionResult(false, new[] { $"Failed to load scene: {input}" });
            });

        // Register per-scene shortcut commands (open_scene.<SceneName>)
        foreach (var sceneAsset in AssetDatabase.Instance.GetAssets(AssetType.Scene))
        {
            var sceneName = Path.GetFileNameWithoutExtension(sceneAsset.FileName);
            var sceneRelPath = sceneAsset.RelativePath;
            ConsoleBackend.RegisterCommand(
                $"open_scene.{sceneName}",
                $"open_scene.{sceneName}",
                $"Load scene: {sceneRelPath}",
                _ =>
                {
                    var scene = SceneManager.Instance.LoadSceneByName(sceneRelPath);
                    return scene != null
                        ? new ConsoleExecutionResult(true, new[] { $"Loaded scene: {scene.FilePath}" })
                        : new ConsoleExecutionResult(false, new[] { $"Failed to load scene: {sceneRelPath}" });
                });
        }

        ConsoleBackend.RegisterCommand("restart_scene", "restart_scene", "Reload the current scene from disk.",
            _ =>
            {
                var active = SceneManager.Instance.ActiveScene;
                if (active == null)
                    return new ConsoleExecutionResult(false, new[] { "No active scene." });

                var filePath = active.FilePath;
                if (string.IsNullOrEmpty(filePath))
                    return new ConsoleExecutionResult(false, new[] { "Active scene has no file path (unsaved scene)." });

                if (!File.Exists(filePath))
                    return new ConsoleExecutionResult(false, new[] { $"Scene file not found: {filePath}" });

                var scene = SceneManager.Instance.LoadScene(filePath);
                return scene != null
                    ? new ConsoleExecutionResult(true, new[] { $"Reloaded scene: {filePath}" })
                    : new ConsoleExecutionResult(false, new[] { $"Failed to reload scene: {filePath}" });
            });

        ConsoleBackend.RegisterCommand("entities", "entities", "List all entities in the active scene.",
            _ =>
            {
                var scene = SceneManager.Instance.ActiveScene;
                if (scene == null)
                    return new ConsoleExecutionResult(true, new[] { "No active scene." });

                var entities = scene.Entities;
                if (entities.Count == 0)
                    return new ConsoleExecutionResult(true, new[] { "Scene has no entities." });

                const int maxDisplay = 50;
                var lines = new List<string>();
                int count = Math.Min(entities.Count, maxDisplay);
                for (int i = 0; i < count; i++)
                {
                    var e = entities[i];
                    var status = e.Active ? "active" : "inactive";
                    lines.Add($"  {e.Name} [{status}]");
                }

                if (entities.Count > maxDisplay)
                    lines.Add($"  ... and {entities.Count - maxDisplay} more");

                lines.Insert(0, $"Entities ({entities.Count}):");
                return new ConsoleExecutionResult(true, lines);
            });
    }

    private static void RegisterVolumeCVar(string name, string description, Func<float> getter, Action<float> setter)
    {
        ConsoleBackend.RegisterCVar(new ConsoleCVar(
            name,
            $"{name} [0.0-2.0]",
            description,
            () => getter().ToString("F2"),
            value =>
            {
                if (!TryParseFloatClamped(value, 0f, 2f, out var v))
                    return false;
                setter(v);
                return true;
            }));
    }

    private static bool TryParseBool01(string value, out bool enabled)
    {
        enabled = false;
        if (value == null)
            return false;
        switch (value.Trim())
        {
            case "1":
                enabled = true;
                return true;
            case "0":
                enabled = false;
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseFloatClamped(string value, float min, float max, out float result)
    {
        result = 0f;
        if (!float.TryParse(value, out var parsed) || !float.IsFinite(parsed))
            return false;
        if (parsed < min || parsed > max)
            return false;
        result = parsed;
        return true;
    }
}
