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

    public PhysicsNarrowPhaseCallbacks(
        SpringSettings contactSpringiness,
        float maximumRecoveryVelocity,
        float defaultFriction,
        float defaultRestitution,
        PhysicsMaterialTable materialTable,
        CharacterControllers? characters)
    {
        ContactSpringiness = contactSpringiness;
        MaximumRecoveryVelocity = maximumRecoveryVelocity;
        DefaultFriction = defaultFriction;
        DefaultRestitution = defaultRestitution;
        MaterialTable = materialTable;
        Characters = characters;
    }

    public void Initialize(Simulation simulation)
    {
        if (ContactSpringiness.AngularFrequency == 0f && ContactSpringiness.TwiceDampingRatio == 0f)
            ContactSpringiness = new SpringSettings(30f, 1f);

        Characters?.Initialize(simulation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
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

