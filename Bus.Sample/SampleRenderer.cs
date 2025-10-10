using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common;
using Bus.Common.Rendering;
using Bus.Common.Vehicles;
using Bus.Common.Worlds;
using Bus.Sample.Vehicles;

namespace Bus.Sample
{/*
    public class SampleRenderer : Renderer
    {
        private readonly AttachableObject DriverViewpoint;
        private readonly AttachableObject BirdViewpoint;

        public SampleRenderer(IDXHost dxHost, IWorldInfo worldInfo) : base(dxHost, worldInfo)
        {
            VehicleBuilder vehicleBuilder = new VehicleBuilder()
            {
                DXHost = DXHost,
                TimeManager = TimeManager,
                Camera = Camera,
            };
            Vehicle = new SampleBus(vehicleBuilder);


            Matrix4x4 matrix = Matrix4x4.CreateLookAtLeftHanded(new Vector3(0, 15, -25), Vector3.Zero, Vector3.UnitY);
            Matrix4x4.Invert(matrix, out matrix);

            DriverViewpoint = new AttachableObject(Vehicle, Matrix4x4.CreateTranslation(0.67f, 2, -1.3f)/* * Matrix4x4.CreateRotationX(0.1f)*\/);
            BirdViewpoint = new AttachableObject(Vehicle, matrix);
            Camera.Viewpoint = DriverViewpoint;
        }
    }*/
}
