using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Input;
using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Vehicles
{
    public abstract class VehicleBase : RigidBody
    {
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        public IDXHost DXHost { get; }
        public IPhysicsHost PhysicsHost { get; }
        public ITimeManager TimeManager { get; }
        public InputManager InputManager { get; }
        public Camera Camera { get; }

        public abstract Viewpoint DriverViewpoint { get; }
        public abstract Viewpoint BirdViewpoint { get; }

        public VehicleBase(VehicleBuilder builder) : base(builder.PhysicsHost.Simulation)
        {
            DXHost = builder.DXHost;
            PhysicsHost = builder.PhysicsHost;
            TimeManager = builder.TimeManager;
            InputManager = builder.InputManager;
            Camera = builder.Camera;
        }
    }
}
