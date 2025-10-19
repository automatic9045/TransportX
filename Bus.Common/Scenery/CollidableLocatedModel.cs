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
}
