using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using Vortice.XAudio2;

using Bus.Common.Scenery;
using Bus.Common.Vehicles;

namespace Bus.Common.Rendering
{
    public class Camera : LocatableObject
    {
        public static readonly LocatableObject DefaultViewpoint = new LocatableObject();


        private Matrix4x4 RelativeRotation = Matrix4x4.Identity;
        private Matrix4x4 View = default;

        public int DrawPlateCount { get; set; } = 2;

        public float Perspective { get; set; } = 1;
        public Listener Listener { get; set; } = new Listener();

        private LocatableObject _Viewpoint = DefaultViewpoint;
        public LocatableObject Viewpoint
        {
            get => _Viewpoint;
            set
            {
                RelativeRotation = Matrix4x4.Identity;
                _Viewpoint = value;
            }
        }

        public Camera() : base()
        {
            Moved += (sender, e) =>
            {
                Listener.OrientFront = Direction;
                Listener.OrientTop = Up;
                Listener.Position = Position;
                //Listener.Velocity = Velocity;

                Matrix4x4.Invert(Locator, out View);
            };
        }

        public void SetDirection(Vector3 direction)
        {
            Matrix4x4 rotation = Matrix4x4.CreateLookToLeftHanded(Vector3.Zero, direction, Viewpoint.Up);
            Matrix4x4.Invert(rotation, out rotation);
            RelativeRotation = rotation;
        }

        public void DrawBackground(ID3D11DeviceContext context, ID3D11Buffer constantBuffer, IEnumerable<LocatedModel> models, System.Drawing.Size clientSize)
        {
            UpdateLocation();

            foreach (LocatedModel model in models)
            {
                model.Locator = Matrix4x4.CreateTranslation(Locator.Translation);
                model.Draw(context, constantBuffer, View, CreateProjection(clientSize));
            }
        }

        public void DrawPlates(ID3D11DeviceContext context, ID3D11Buffer constantBuffer, PlateCollection plates, System.Drawing.Size clientSize)
        {
            UpdateLocation();

            List<(int i, int x, int z)> a = new List<(int i, int x, int z)>();
            for (int i = DrawPlateCount - 1; 0 <= i; i--)
            {
                for (int x = PlateX - i; x <= PlateX + i; x++)
                {
                    int dz = int.Abs(x - PlateX) == i ? 1 : i * 2;
                    for (int z = PlateZ - i; z <= PlateZ + i; z += dz)
                    {
                        a.Add((i, x, z));
                        if (plates.TryGetValue(x, z, out LocatedPlate? plate))
                        {
                            Vector3 platePosition = Plate.Size * new Vector3(PlateX - x, 0, PlateZ - z);
                            Matrix4x4 view = Matrix4x4.CreateTranslation(-platePosition) * View;
                            plate!.Plate.Draw(context, constantBuffer, view, CreateProjection(clientSize));
                        }
                    }
                }
            }
        }

        public void DrawVehicles(ID3D11DeviceContext context, ID3D11Buffer constantBuffer, VehicleBase vehicle, System.Drawing.Size clientSize)
        {
            UpdateLocation();

            vehicle.Draw(context, constantBuffer, View, CreateProjection(clientSize));
        }

        protected void UpdateLocation()
        {
            Locate(Viewpoint, RelativeRotation * Viewpoint.Locator);
        }

        protected Matrix4x4 CreateProjection(System.Drawing.Size clientSize)
        {
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                Perspective * MathHelper.ToRadians(45), (float)clientSize.Width / clientSize.Height, 0.1f, 1000);
            return projection;
        }
    }
}
