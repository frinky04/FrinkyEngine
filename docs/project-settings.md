# Project Settings

## `.fproject`

The project file is the entry point for both editor and runtime. It defines where assets, scenes, and game code are located.

```json
{
  "projectName": "MyGame",
  "defaultScene": "Scenes/MainScene.fscene",
  "assetsPath": "Assets",
  "gameAssembly": "bin/Debug/net8.0/MyGame.dll",
  "gameProject": "MyGame.csproj"
}
```

| Field | Description |
|-------|-------------|
| `projectName` | Display name for the project |
| `defaultScene` | Startup scene path, relative to `assetsPath` |
| `assetsPath` | Root folder for project assets |
| `gameAssembly` | Path to the compiled game DLL |
| `gameProject` | Path to the `.csproj` for script builds |

`defaultScene` is relative to `assetsPath`. The editor script build uses `Debug`, so `gameAssembly` points to `bin/Debug`.

## `project_settings.json`

Runtime and build settings. Created automatically in the project root.

```json
{
  "project": {
    "version": "0.1.0",
    "author": "",
    "company": "",
    "description": ""
  },
  "runtime": {
    "targetFps": 120,
    "vSync": true,
    "windowTitle": "MyGame",
    "windowWidth": 1280,
    "windowHeight": 720,
    "resizable": true,
    "fullscreen": false,
    "startMaximized": false,
    "startupSceneOverride": "",
    "forwardPlusTileSize": 16,
    "forwardPlusMaxLights": 256,
    "forwardPlusMaxLightsPerTile": 64,
    "audioMasterVolume": 1.0,
    "audioMusicVolume": 1.0,
    "audioSfxVolume": 1.0,
    "audioUiVolume": 1.0,
    "audioVoiceVolume": 1.0,
    "audioAmbientVolume": 1.0,
    "audioMaxVoices": 128,
    "audioDopplerScale": 1.0,
    "audioEnableVoiceStealing": true
  },
  "build": {
    "outputName": "MyGame",
    "buildVersion": "0.1.0"
  }
}
```

### Project Section

| Field | Description |
|-------|-------------|
| `version` | Project version string |
| `author` | Author name |
| `company` | Company name |
| `description` | Project description |

### Runtime Section

| Field | Default | Description |
|-------|---------|-------------|
| `targetFps` | 120 | Target frame rate (`0` for uncapped; valid range: 30–500) |
| `vSync` | true | Enable vertical sync |
| `windowTitle` | "MyGame" | Window title |
| `windowWidth` | 1280 | Initial window width |
| `windowHeight` | 720 | Initial window height |
| `resizable` | true | Allow window resizing |
| `fullscreen` | false | Start in fullscreen |
| `startMaximized` | false | Start maximized |
| `startupSceneOverride` | "" | Override the default scene at startup |
| `forwardPlusTileSize` | 16 | Lighting tile size in pixels |
| `forwardPlusMaxLights` | 256 | Maximum total lights |
| `forwardPlusMaxLightsPerTile` | 64 | Maximum lights per tile |
| `audioMasterVolume` | 1.0 | Master bus volume |
| `audioMusicVolume` | 1.0 | Music bus volume |
| `audioSfxVolume` | 1.0 | SFX bus volume |
| `audioUiVolume` | 1.0 | UI bus volume |
| `audioVoiceVolume` | 1.0 | Voice bus volume |
| `audioAmbientVolume` | 1.0 | Ambient bus volume |
| `audioMaxVoices` | 128 | Maximum concurrent audio voices |
| `audioDopplerScale` | 1.0 | Doppler effect scale |
| `audioEnableVoiceStealing` | true | Allow voice stealing when at max voices |

### Build Section

| Field | Default | Description |
|-------|---------|-------------|
| `outputName` | "MyGame" | Name for the exported executable |
| `buildVersion` | "0.1.0" | Version embedded in the exported build |

## Editor Settings

### `.frinky/editor_settings.json`

Per-project editor preferences. Auto-created when a project opens.

### `.frinky/keybinds.json`

Per-project keybind overrides. Auto-created with defaults when a project opens. See [Editor Guide](editor-guide.md) for the default keyboard shortcuts.
