using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuUtilities;

namespace TransportX.Physics
{
    public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        private static readonly Vector3 Gravity = new Vector3(0, -9.8f, 0);

        private Vector3Wide GravityWideDt;

        public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public readonly bool AllowSubstepsForUnconstrainedBodies => false;
        public readonly bool IntegrateVelocityForKinematics => false;

        public void Initialize(Simulation simulation)
        {

        }

        public void PrepareForIntegration(float dt)
        {
            GravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
            BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
        {
            velocity.Linear += GravityWideDt;
        }
    }
}
