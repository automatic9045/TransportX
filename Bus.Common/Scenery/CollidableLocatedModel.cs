using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Rendering;

namespace Bus.Common.Scenery
{
    public abstract class CollidableLocatedModel : LocatedModel
    {
        protected readonly Simulation Simulation;

        public new ICollidableModel Model { get; }
        public PlateOffset FromCamera { get; private set; } = PlateOffset.Identity;

        public override Matrix4x4 Transform
        {
            get => base.Transform;
            set
            {
                base.Transform = value;
                ColliderTransform = Transform;
            }
        }

        protected abstract Matrix4x4 ColliderTransform { get; set; }

        internal protected CollidableLocatedModel(Simulation simulation, ICollidableModel model, Matrix4x4 transform) : base(model, transform, false)
        {
            Simulation = simulation;
            Model = model;
        }

        public void Update(PlateOffset fromCamera)
        {
            PlateOffset fromCameraDelta = fromCamera - FromCamera;
            if (!fromCameraDelta.IsZero)
            {
                Matrix4x4 colliderTransform = ColliderTransform;
                FromCamera = fromCamera;
                ColliderTransform = colliderTransform;
            }

            base.Transform = ColliderTransform;
        }
    }
}
