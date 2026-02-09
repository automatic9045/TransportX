using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

namespace TransportX.Physics
{
    public readonly struct ColliderMaterial
    {
        public static readonly ColliderMaterial Default = new ColliderMaterial(1, float.MaxValue, new SpringSettings(30, 1));


        public bool IsInitialized { get; } = false;

        public float FrictionCoefficient { get; }
        public float MaximumRecoveryVelocity { get; }
        public SpringSettings SpringSettings { get; }

        public ColliderMaterial(float frictionCoefficient, float maximumRecoveryVelocity, SpringSettings springSettings)
        {
            IsInitialized = true;

            FrictionCoefficient = frictionCoefficient;
            MaximumRecoveryVelocity = maximumRecoveryVelocity;
            SpringSettings = springSettings;
        }
    }
}
