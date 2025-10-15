using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using Vortice.Direct3D11;

using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public class LocatedModel : IDrawable
    {
        public IModel Model { get; }
        public Matrix4x4 InitialLocator { get; }
        public Matrix4x4 Locator { get; set; }

        public LocatedModel(IModel model, Matrix4x4 locator)
        {
            Model = model;
            InitialLocator = locator;
            Locator = locator;
        }

        public static LocatedModel CreateStaticOrNonCollision(Simulation simulation, IModel model, Matrix4x4 locator)
        {
            return model is ICollidableModel collidableModel
                ? CreateStatic(simulation, collidableModel, locator) : new LocatedModel(model, locator);
        }

        public static StaticLocatedModel CreateStatic(Simulation simulation, ICollidableModel model, Matrix4x4 locator)
        {
            StaticDescription desc = new StaticDescription((locator * model.Collider.Transform).ToRigidPose(), model.Collider.ShapeIndex);
            StaticHandle handle = simulation.Statics.Add(desc);
            return new StaticLocatedModel(simulation, model, handle, locator);
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


    public class StaticLocatedModel : LocatedModel
    {
        private readonly Simulation Simulation;

        public new ICollidableModel Model { get; }
        public StaticHandle Handle { get; }

        internal protected StaticLocatedModel(Simulation simulation, ICollidableModel model, StaticHandle handle, Matrix4x4 locator) : base(model, locator)
        {
            Simulation = simulation;
            Model = model;
            Handle = handle;
        }

        public void UpdateColliderPose(PlateOffset plateOffset)
        {
            Matrix4x4 locator = Locator * plateOffset.Transform;
            Simulation.Statics[Handle].Pose = (locator * Model.Collider.Transform).ToRigidPose();
        }
    }
}
