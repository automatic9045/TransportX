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
        private static readonly Vector3 Gravity = new(0, -9.8f, 0);

        private static readonly Vector<float> MaxLinearVelocityWide = new(500);
        private static readonly Vector<float> MaxAngularVelocityWide = new(400);
        private static readonly Vector<float> EpsilonWide = new(1e-10f);

        private Vector3Wide GravityWideDt;

        public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public readonly bool AllowSubstepsForUnconstrainedBodies => false;
        public readonly bool IntegrateVelocityForKinematics => false;

        public readonly void Initialize(Simulation simulation)
        {
        }

        public void PrepareForIntegration(float dt)
        {
            GravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        public readonly void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
            BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
        {
            velocity.Linear += GravityWideDt;

            Vector3Wide.Length(velocity.Linear, out Vector<float> linearLength);
            Vector<float> safeLinearLength = Vector.Max(linearLength, EpsilonWide);
            Vector<float> linearScale = Vector.Min(Vector<float>.One, MaxLinearVelocityWide / safeLinearLength);
            Vector3Wide.Scale(velocity.Linear, linearScale, out velocity.Linear);

            Vector3Wide.Length(velocity.Angular, out Vector<float> angularLength);
            Vector<float> safeAngularLength = Vector.Max(angularLength, EpsilonWide);
            Vector<float> angularScale = Vector.Min(Vector<float>.One, MaxAngularVelocityWide / safeAngularLength);
            Vector3Wide.Scale(velocity.Angular, angularScale, out velocity.Angular);
        }
    }
}
