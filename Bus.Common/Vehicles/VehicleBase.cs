using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using Bus.Common.Input;
using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Common.Vehicles
{
    public abstract class VehicleBase : LocatableObject, IDisposable
    {
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }

        public IDXHost DXHost { get; }
        public IPhysicsHost PhysicsHost { get; }
        public ITimeManager TimeManager { get; }
        public InputManager InputManager { get; }
        public Camera Camera { get; }

        public VehicleBase(VehicleBuilder builder) : base()
        {
            DXHost = builder.DXHost;
            PhysicsHost = builder.PhysicsHost;
            TimeManager = builder.TimeManager;
            InputManager = builder.InputManager;
            Camera = builder.Camera;
        }

        public abstract void Dispose();
        public abstract void ComputeTick(TimeSpan elapsed);
        public abstract void Tick(TimeSpan elapsed);
        public abstract void Draw(ID3D11DeviceContext context, ID3D11Buffer constantBuffer, Matrix4x4 view, Matrix4x4 projection);
    }
}
