---
name: update-docs
description: Update hand-written documentation in docs/. Use when adding features, changing APIs, or modifying user-facing behavior that needs documentation updates. Covers physics, scripting, components, editor, and all other guide pages.
allowed-tools: Read Grep Glob Edit
---

# Update Documentation

Update the hand-written guides in `docs/` to reflect code changes.

## Critical Rule

**NEVER touch `docs/api/`** — that directory is auto-generated from XML doc comments by a CI workflow on each release. Do not edit, regenerate, or reference its contents as a source of truth for writing guides.

## Step 1: Identify What Changed

Read the code changes (new files, modified files, git diff) to understand:
- What user-facing APIs or behaviors were added/changed/removed?
- Are there new public types, methods, properties, or parameters?
- Were any existing signatures changed (renamed, removed, new required params)?

## Step 2: Find Affected Docs

The hand-written docs live in `docs/`. Use this mapping to find the right file:

| Topic | File |
|-------|------|
| Rigidbodies, colliders, raycasting, triggers, character controller | `docs/physics.md` |
| Custom components, lifecycle, game assemblies | `docs/scripting.md` |
| Built-in component reference (properties tables) | `docs/components.md` |
| Editor panels, shortcuts, workflows | `docs/editor-guide.md` |
| Audio playback, buses, attenuation | `docs/audio.md` |
| Lighting, materials, post-processing | `docs/rendering.md` |
| UI system | `docs/ui.md` |
| Prefabs, entity references | `docs/prefabs.md` |
| Export pipeline, packaging | `docs/exporting.md` |
| Project settings files | `docs/project-settings.md` |
| First-time setup | `docs/getting-started.md` |
| Common issues | `docs/troubleshooting.md` |
| Docs index / table of contents | `docs/README.md` |
| Future plans | `docs/roadmaps/*.md` |

If a change spans multiple topics, update all relevant files.

## Step 3: Read the Existing Section

Read the target doc file before editing. Understand:
- Where the relevant section is (or where a new section should go)
- The style and depth of surrounding content
- Whether tables, code blocks, or prose are used for similar content

## Step 4: Write the Update

Follow the existing style of the document. General conventions:

- **Code examples**: Use fenced C# blocks with `using` statements when showing API usage for the first time in a section
- **Property tables**: Use `| Property | Default | Description |` format
- **Type/field tables**: Use `| Field | Type | Description |` format
- **Section hierarchy**: `##` for major topics, `###` for subtopics within them
- **Tone**: Direct, concise, second-person ("Add a component", "Cast a ray")
- **Removed APIs**: Delete the old documentation entirely — do not leave stubs or deprecation notes unless backward compatibility shims exist in code
- **New sections**: Place them in logical order relative to existing content (e.g., a new raycast filtering section goes inside the existing Raycasting section, not at the end of the file)

## Step 5: Update docs/README.md If Needed

If a new guide file was created, or if an existing guide's scope changed significantly, update the tables in `docs/README.md` to reflect it.

## Checklist

- [ ] Only hand-written docs in `docs/` were edited (not `docs/api/`)
- [ ] Code examples compile and use current API signatures
- [ ] No references to removed/old API signatures remain
- [ ] Property/field tables match the actual code
- [ ] New sections are placed logically within existing structure
- [ ] `docs/README.md` index is up to date if guides were added/renamed
