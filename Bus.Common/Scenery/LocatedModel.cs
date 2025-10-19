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

namespace Bus.Common.Scenery
{
    public class LocatedModel : IDrawable
    {
        public IModel Model { get; }
        public Matrix4x4 InitialLocator { get; }
        public Matrix4x4 InitialLocatorInverse { get; }
        public virtual Matrix4x4 Locator { get; set; } = Matrix4x4.Identity;

        protected LocatedModel(IModel model, Matrix4x4 locator, bool setLocator)
        {
            Model = model;
            InitialLocator = locator;
            Matrix4x4.Invert(locator, out Matrix4x4 initialLocatorInverse);
            InitialLocatorInverse = initialLocatorInverse;
            if (setLocator) Locator = locator;
        }

        public LocatedModel(IModel model, Matrix4x4 locator) : this(model, locator, true)
        {
        }

        public void Draw(DrawContext context)
        {
            ConstantBuffer cb = new ConstantBuffer()
            {
                World = Matrix4x4.Transpose(Locator * context.PlateOffset.Transform),
                View = Matrix4x4.Transpose(context.View),
                Projection = Matrix4x4.Transpose(context.Projection),
            };
            context.DeviceContext.UpdateSubresource(cb, context.ConstantBuffer);

            Model.Draw(context.DeviceContext);
        }
    }


    public abstract class CollidableLocatedModel : LocatedModel
    {
        protected readonly Simulation Simulation;

        public new ICollidableModel Model { get; }
        public PlateOffset FromCamera { get; private set; } = PlateOffset.Identity;

        public override Matrix4x4 Locator
        {
            get => base.Locator;
            set
            {
                base.Locator = value;
                ColliderLocator = Locator;
            }
        }

        protected abstract Matrix4x4 ColliderLocator { get; set; }

        internal protected CollidableLocatedModel(Simulation simulation, ICollidableModel model, Matrix4x4 locator) : base(model, locator, false)
        {
            Simulation = simulation;
            Model = model;
        }

        public void ComputeTick(PlateOffset fromCamera)
        {
            PlateOffset fromCameraDelta = fromCamera - FromCamera;
            if (!fromCameraDelta.IsZero)
            {
                Matrix4x4 colliderLocator = ColliderLocator;
                FromCamera = fromCamera;
                ColliderLocator = colliderLocator;
            }

            base.Locator = ColliderLocator;
        }
    }


    public class StaticLocatedModel : CollidableLocatedModel
    {
        public StaticHandle Handle { get; }
        public StaticReference Static => Simulation.Statics[Handle];

        protected override Matrix4x4 ColliderLocator
        {
            get => Model.Collider.TransformInverse * Static.Pose.ToMatrix4x4() * FromCamera.TransformInverse;
            set
            {
                Static.GetDescription(out StaticDescription desc);
                desc.Pose = (Model.Collider.Transform * value * FromCamera.Transform).ToRigidPose();
                Static.ApplyDescription(desc);
            }
        }

        internal protected StaticLocatedModel(Simulation simulation, ICollidableModel model, StaticHandle handle, Matrix4x4 locator)
            : base(simulation, model, locator)
        {
            Handle = handle;
        }

        public static StaticLocatedModel Create(Simulation simulation, ICollidableModel model, Matrix4x4 locator)
        {
            StaticDescription desc = new StaticDescription((locator * model.Collider.Transform).ToRigidPose(), model.Collider.ShapeIndex);
            StaticHandle handle = simulation.Statics.Add(desc);
            return new StaticLocatedModel(simulation, model, handle, locator);
        }

        public static LocatedModel CreateStaticOrNonCollision(Simulation simulation, IModel model, Matrix4x4 locator)
        {
            return model is ICollidableModel collidableModel
                ? Create(simulation, collidableModel, locator) : new LocatedModel(model, locator);
        }
    }


    public class DynamicLocatedModel : CollidableLocatedModel
    {
        public BodyHandle Handle { get; }
        public BodyReference Body => Simulation.Bodies[Handle];
        public bool IsKinematic => Body.Kinematic;

        protected override Matrix4x4 ColliderLocator
        {
            get => Model.Collider.TransformInverse * Body.Pose.ToMatrix4x4() * FromCamera.TransformInverse;
            set
            {
                Simulation.Awakener.AwakenBody(Handle);
                Body.Pose = (Model.Collider.Transform * value * FromCamera.Transform).ToRigidPose();
            }
        }

        internal protected DynamicLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, Matrix4x4 locator)
            : base(simulation, model, locator)
        {
            Handle = handle;
        }

        public static DynamicLocatedModel Create(Simulation simulation,
            ICollidableModel model, Func<ICollidableModel, RigidPose, BodyDescription> descFactory, Matrix4x4 locator)
        {
            BodyDescription desc = descFactory(model, (locator * model.Collider.Transform).ToRigidPose());
            BodyHandle handle = simulation.Bodies.Add(desc);
            return new DynamicLocatedModel(simulation, model, handle, locator);
        }

        public static DynamicLocatedModel Create(Simulation simulation,
            ICollidableModel model, float mass, CollidableDescription collidableDescription, Matrix4x4 locator)
        {
            BodyInertia inertia = model.Collider.ComputeInertia(mass);
            BodyDescription CreateDesc(ICollidableModel model, RigidPose pose)
                => BodyDescription.CreateDynamic(pose, inertia, collidableDescription, 0.01f);

            return Create(simulation, model, CreateDesc, locator);
        }

        public static DynamicLocatedModel Create(Simulation simulation, ICollidableModel model, float mass, Matrix4x4 locator)
        {
            return Create(simulation, model, mass, model.Collider.ShapeIndex, locator);
        }

        public static DynamicLocatedModel CreateKinematic(Simulation simulation, ICollidableModel model, Matrix4x4 locator)
        {
            BodyDescription CreateDesc(ICollidableModel model, RigidPose pose)
                => BodyDescription.CreateKinematic(pose, model.Collider.ShapeIndex, 0.01f);

            return Create(simulation, model, CreateDesc, locator);
        }

        public static LocatedModel CreateKinematicOrNonCollision(Simulation simulation, IModel model, Matrix4x4 locator)
        {
            return model is ICollidableModel collidableModel
                ? CreateKinematic(simulation, collidableModel, locator) : new LocatedModel(model, locator);
        }

        public void Shift(PlateOffset offset)
        {
            Locator *= offset.Transform;
        }
    }
}
