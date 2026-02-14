# Editor UX - Quick-Add Physics

## Status: In Progress

## Plan
1. Add "Add Physics" submenu to entity context menus in HierarchyPanel (right-click on entity)
2. Add "Add Physics" submenu to InspectorPanel (near Add Component button)
3. Logic: auto-detect collider shape from primitive type, skip duplicates, auto-size
4. Update docs/editor-guide.md with new feature info

## Key Files
- `src/FrinkyEngine.Editor/Panels/HierarchyPanel.cs` - entity context menu
- `src/FrinkyEngine.Editor/Panels/InspectorPanel.cs` - inspector Add Component area
- `src/FrinkyEngine.Core/Components/ColliderComponent.cs` - base collider
- `src/FrinkyEngine.Core/Components/BoxColliderComponent.cs`
- `src/FrinkyEngine.Core/Components/SphereColliderComponent.cs`
- `src/FrinkyEngine.Core/Components/CapsuleColliderComponent.cs`
- `src/FrinkyEngine.Core/Components/RigidbodyComponent.cs` - BodyMotionType enum
- `src/FrinkyEngine.Core/Components/PrimitiveComponent.cs` - base primitive
- `src/FrinkyEngine.Core/Components/CubePrimitive.cs`
- `src/FrinkyEngine.Core/Components/SpherePrimitive.cs`
- `src/FrinkyEngine.Core/Components/CylinderPrimitive.cs`
- `src/FrinkyEngine.Core/Components/PlanePrimitive.cs`

## Design
- Create a `PhysicsShortcuts` helper class in Editor to encapsulate the logic
- Methods: AddStaticBody, AddDynamicBody, AddKinematicBody
- Auto-detect collider shape: CubePrimitive -> BoxCollider, SpherePrimitive -> SphereCollider, CylinderPrimitive -> CapsuleCollider, PlanePrimitive -> BoxCollider (flat)
- Auto-size: match collider dims to primitive dims
- Skip if entity already has the component
- Integrate into both HierarchyPanel context menu and InspectorPanel
