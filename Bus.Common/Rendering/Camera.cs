using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.Mathematics;
using Vortice.XAudio2;

using GdiSize = System.Drawing.Size;

using Bus.Common.Scenery;

namespace Bus.Common.Rendering
{
    public class Camera : LocatableObject
    {
        private Matrix4x4 View = default;

        public int DrawPlateCount { get; set; } = 2;
        public Listener Listener { get; } = new Listener();
        public ViewpointSet Viewpoints { get; } = new ViewpointSet();

        public Camera() : base()
        {
            Moved += (sender, e) =>
            {
                Listener.OrientFront = Direction;
                Listener.OrientTop = Up;
                Listener.Position = Position;
                //Listener.Velocity = Velocity;

                Matrix4x4.Invert(Transform, out View);
            };
        }

        public void DrawBackground(ID3D11DeviceContext deviceContext, ID3D11Buffer constantBuffer, IEnumerable<LocatedModel> models, GdiSize clientSize)
        {
            UpdateLocation();

            foreach (LocatedModel model in models)
            {
                model.Transform = Matrix4x4.CreateTranslation(Transform.Translation);

                DrawContext drawContext = new(deviceContext, constantBuffer, PlateOffset.Identity, View, CreateProjection(clientSize));
                model.Draw(drawContext);
            }
        }

        public void DrawPlates(ID3D11DeviceContext deviceContext, ID3D11Buffer constantBuffer, PlateCollection plates, GdiSize clientSize)
        {
            UpdateLocation();

            Matrix4x4 projection = CreateProjection(clientSize);

            for (int i = DrawPlateCount - 1; 0 <= i; i--)
            {
                for (int x = PlateX - i; x <= PlateX + i; x++)
                {
                    int dz = int.Abs(x - PlateX) == i ? 1 : i * 2;
                    for (int z = PlateZ - i; z <= PlateZ + i; z += dz)
                    {
                        if (plates.TryGetValue(x, z, out LocatedPlate? plate))
                        {
                            PlateOffset plateOffset = new PlateOffset(x - PlateX, z - PlateZ);
                            DrawContext drawContext = new(deviceContext, constantBuffer, plateOffset, View, projection);
                            plate!.Plate.Draw(drawContext);
                        }
                    }
                }
            }
        }

        public void DrawBodies(ID3D11DeviceContext deviceContext, ID3D11Buffer constantBuffer, IEnumerable<RigidBody> bodies, GdiSize clientSize)
        {
            UpdateLocation();

            Matrix4x4 projection = CreateProjection(clientSize);

            foreach (RigidBody body in bodies)
            {
                PlateOffset plateOffset = new PlateOffset(body.PlateX - PlateX, body.PlateZ - PlateZ);
                DrawContext drawContext = new(deviceContext, constantBuffer, plateOffset, View, projection);
                body.Draw(drawContext);
            }
        }

        protected void UpdateLocation()
        {
            Locate(Viewpoints.Current.Source, Viewpoints.Current.Transform);
        }

        protected Matrix4x4 CreateProjection(GdiSize clientSize)
        {
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                Viewpoints.Current.Perspective * MathHelper.ToRadians(45), (float)clientSize.Width / clientSize.Height, 0.1f, 1000);
            return projection;
        }
    }
}
