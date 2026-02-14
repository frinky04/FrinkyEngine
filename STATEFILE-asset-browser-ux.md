# Asset Browser UX Pass

## Status: Implementation Complete

## Changes Made

### 1. Pinned search bar at the top
- `Draw()` now wraps the asset list in `ImGui.BeginChild("##AssetContent")`, creating a scrollable child region below the toolbar/tag filter bar
- The toolbar and tag filter bar remain in the parent window, staying pinned at top

### 2. Settings cog button
- Replaced the flat toolbar buttons (Refresh, Grid/List radio, Icon Size slider, Tag Manager) with a single gear icon button
- Clicking the cog opens a popup containing all secondary settings:
  - Refresh
  - Grid View / List View toggles
  - Icon Size slider
  - Hide Unrecognised Assets toggle
  - Tag Manager link
- The top bar now focuses on: settings cog, type filter combo, and search input

### 3. Hide Unrecognised Assets
- Added `HideUnrecognisedAssets` property to `EditorProjectSettings` (defaults to `true`)
- Updated `Clone()` and `GetDefault()` to include the new property
- In `BuildFilteredItems()`, assets with `AssetType.Unknown` are filtered out when the setting is enabled and "All" filter is selected
- Setting is persisted via `EditorProjectSettings.Save()`

## Files Modified
- `src/FrinkyEngine.Editor/EditorProjectSettings.cs` — Added `HideUnrecognisedAssets` property
- `src/FrinkyEngine.Editor/Panels/AssetBrowserPanel.cs` — Sticky header, settings cog, unknown asset filtering
