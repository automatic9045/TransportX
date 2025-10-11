using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

namespace Bus.Common.Physics
{
    public struct Material
    {
        public SpringSettings SpringSettings;
        public float FrictionCoefficient;
        public float MaximumRecoveryVelocity;
    }
}
