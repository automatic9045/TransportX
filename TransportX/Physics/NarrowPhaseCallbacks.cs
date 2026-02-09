using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;

namespace TransportX.Physics
{
    public struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        private readonly CollidableProperty<ColliderGroupHandle> Groups;
        private readonly CollidableProperty<ColliderMaterial> Materials;

        public NarrowPhaseCallbacks(CollidableProperty<ColliderGroupHandle> groups, CollidableProperty<ColliderMaterial> materials)
        {
            Groups = groups;
            Materials = materials;
        }

        public NarrowPhaseCallbacks()
        {
            throw new NotSupportedException();
        }

        public void Initialize(Simulation simulation)
        {
            Groups.Initialize(simulation);
            Materials.Initialize(simulation);
        }

        public void Dispose()
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            if (a.Mobility != CollidableMobility.Static)
            {
                ColliderGroupHandle aGroup = Groups.Allocate(a);
                ColliderGroupHandle bGroup = Groups.Allocate(b);

                if (aGroup == ColliderGroupHandle.Skip) return false;
                if (bGroup == ColliderGroupHandle.Skip) return false;
                if (aGroup != ColliderGroupHandle.None && aGroup == bGroup) return false;
            }

            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial)
            where TManifold : unmanaged, IContactManifold<TManifold>
        {
            ColliderMaterial a = Materials.Allocate(pair.A);
            ColliderMaterial b = Materials.Allocate(pair.B);

            if (!a.IsInitialized) a = ColliderMaterial.Default;
            if (!b.IsInitialized) b = ColliderMaterial.Default;

            pairMaterial = new PairMaterialProperties()
            {
                FrictionCoefficient = a.FrictionCoefficient * b.FrictionCoefficient,
                MaximumRecoveryVelocity = MathF.Max(a.MaximumRecoveryVelocity, b.MaximumRecoveryVelocity),
            };
            pairMaterial.SpringSettings = pairMaterial.MaximumRecoveryVelocity == a.MaximumRecoveryVelocity ? a.SpringSettings : b.SpringSettings;

            if (pairMaterial.SpringSettings.Frequency < 1)
            {
                pairMaterial.SpringSettings.Frequency = 1;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }
    }
}
