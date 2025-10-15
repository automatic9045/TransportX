using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Scenery;

namespace Bus.Common
{
    public class RigidBody : LocatableObject, IDisposable, IDrawable
    {
        protected readonly Simulation Simulation;

        public bool IsModelAttached { get; private set; } = false;

        private LocatedModel ModelKey = new LocatedModel(Rendering.Model.Empty, Matrix4x4.Identity);
        public LocatedModel Model => IsModelAttached ? ModelKey : throw new InvalidOperationException("まだモデルがアタッチされていません。");

        private BodyHandle HandleKey = default;
        public BodyHandle Handle => IsModelAttached ? HandleKey : throw new InvalidOperationException("まだモデルがアタッチされていません。");

        public RigidBody(Simulation simulation, int plateX, int plateZ, Matrix4x4 locator) : base(plateX, plateZ, locator)
        {
            Simulation = simulation;
        }

        public RigidBody(Simulation simulation, int plateX, int plateZ, SixDoF position) : this(simulation, plateX, plateZ, position.CreateTransform())
        {
        }

        public RigidBody(Simulation simulation) : this(simulation, 0, 0, Matrix4x4.Identity)
        {
        }

        public virtual void Dispose()
        {
        }

        public void AttachModel(ICollidableModel model, float mass)
        {
            if (IsModelAttached) throw new InvalidOperationException("既にモデルがアタッチされています。");

            ModelKey = new LocatedModel(model, Locator);

            BodyInertia inertia = model.Collider.ComputeInertia(mass);
            BodyDescription desc = BodyDescription.CreateDynamic(Locator.ToRigidPose(), inertia, model.Collider.ShapeIndex, 0.001f);
            HandleKey = Simulation.Bodies.Add(desc);

            IsModelAttached = true;
        }

        public virtual void ComputeTick(TimeSpan elapsed)
        {
            if (!IsModelAttached) return;

            BodyReference bodyRef = Simulation.Bodies[Handle];
            ICollider collider = ((ICollidableModel)Model.Model).Collider;

            Matrix4x4 locator = collider.TransformInverse * bodyRef.Pose.ToMatrix4x4();
            PlateOffset plateOffset = Locate(PlateX, PlateZ, locator);

            if (!plateOffset.IsZero)
            {
                bodyRef.Pose = (collider.Transform * Locator).ToRigidPose();
            }
            Model.Locator = Locator;
        }

        public virtual void Tick(TimeSpan elapsed)
        {
        }

        public virtual void Draw(DrawContext context)
        {
            Model.Draw(context);
        }
    }
}
