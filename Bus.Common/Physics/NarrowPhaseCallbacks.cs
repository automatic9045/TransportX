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

namespace Bus.Common.Physics
{
    public struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        private readonly CollidableProperty<Material> Materials;

        public NarrowPhaseCallbacks(CollidableProperty<Material> materials)
        {
            Materials = materials;
        }

        public NarrowPhaseCallbacks() : this(new CollidableProperty<Material>())
        {
        }

        public void Initialize(Simulation simulation)
        {
            Materials.Initialize(simulation);
        }

        public void Dispose()
        {

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
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial)
            where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial = new PairMaterialProperties()
            {
                FrictionCoefficient = 1,
                MaximumRecoveryVelocity = float.MaxValue,
                SpringSettings = new SpringSettings(10, 0),
            };
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }
    }
}
