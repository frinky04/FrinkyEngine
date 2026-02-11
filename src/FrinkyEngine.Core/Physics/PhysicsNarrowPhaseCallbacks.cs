using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using FrinkyEngine.Core.Physics.Characters;

namespace FrinkyEngine.Core.Physics;

internal struct PhysicsNarrowPhaseCallbacks : INarrowPhaseCallbacks
{
    public SpringSettings ContactSpringiness;
    public float MaximumRecoveryVelocity;
    public float DefaultFriction;
    public float DefaultRestitution;
    public PhysicsMaterialTable MaterialTable;
    public CharacterControllers? Characters;
    public HashSet<int>? TriggerBodyHandles;
    public HashSet<int>? TriggerStaticHandles;
    public ConcurrentBag<(CollidableReference A, CollidableReference B)>? TriggerPairSink;

    public PhysicsNarrowPhaseCallbacks(
        SpringSettings contactSpringiness,
        float maximumRecoveryVelocity,
        float defaultFriction,
        float defaultRestitution,
        PhysicsMaterialTable materialTable,
        CharacterControllers? characters,
        HashSet<int>? triggerBodyHandles,
        HashSet<int>? triggerStaticHandles,
        ConcurrentBag<(CollidableReference A, CollidableReference B)>? triggerPairSink)
    {
        ContactSpringiness = contactSpringiness;
        MaximumRecoveryVelocity = maximumRecoveryVelocity;
        DefaultFriction = defaultFriction;
        DefaultRestitution = defaultRestitution;
        MaterialTable = materialTable;
        Characters = characters;
        TriggerBodyHandles = triggerBodyHandles;
        TriggerStaticHandles = triggerStaticHandles;
        TriggerPairSink = triggerPairSink;
    }

    public void Initialize(Simulation simulation)
    {
        if (ContactSpringiness.AngularFrequency == 0f && ContactSpringiness.TwiceDampingRatio == 0f)
            ContactSpringiness = new SpringSettings(30f, 1f);

        Characters?.Initialize(simulation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsTrigger(CollidableReference collidable)
    {
        return collidable.Mobility switch
        {
            CollidableMobility.Static => TriggerStaticHandles != null && TriggerStaticHandles.Contains(collidable.StaticHandle.Value),
            _ => TriggerBodyHandles != null && TriggerBodyHandles.Contains(collidable.BodyHandle.Value),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        // Allow contact generation for triggers so we can detect overlaps,
        // and for the normal dynamic requirement.
        if (IsTrigger(a) || IsTrigger(b))
            return true;

        return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
    {
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConfigureContactManifold<TManifold>(
        int workerIndex,
        CollidablePair pair,
        ref TManifold manifold,
        out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
    {
        // If either collidable is a trigger, record the pair and suppress physics response.
        if (IsTrigger(pair.A) || IsTrigger(pair.B))
        {
            TriggerPairSink?.Add((pair.A, pair.B));
            pairMaterial = default;
            return false;
        }

        var materialA = MaterialTable.Get(pair.A, DefaultFriction, DefaultRestitution);
        var materialB = MaterialTable.Get(pair.B, DefaultFriction, DefaultRestitution);
        var friction = MathF.Sqrt(MathF.Max(0f, materialA.Friction * materialB.Friction));

        // BEPU exposes maximum recovery velocity rather than direct restitution; scale it from the pair's restitution.
        var restitution = MathF.Max(materialA.Restitution, materialB.Restitution);
        var maxRecovery = MaximumRecoveryVelocity * (1f + restitution * 2f);

        pairMaterial = new PairMaterialProperties(friction, maxRecovery, ContactSpringiness);
        Characters?.TryReportContacts(pair, ref manifold, workerIndex, ref pairMaterial);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConfigureContactManifold(
        int workerIndex,
        CollidablePair pair,
        int childIndexA,
        int childIndexB,
        ref ConvexContactManifold manifold)
    {
        return true;
    }

    public void Dispose()
    {
    }
}
