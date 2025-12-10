using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;
using Vortice.XAudio2;

using GdiSize = System.Drawing.Size;

using Bus.Common.Bodies;
using Bus.Common.Scenery;

namespace Bus.Common.Rendering
{
    public class Camera : LocatableObject
    {
        private Matrix4x4 View = default;

        public int DrawPlateCount { get; set; } = 2;
        public Listener Listener { get; } = new Listener();
        public ViewpointSet Viewpoints { get; } = new ViewpointSet();

        public bool DrawColliderDebugModel { get; set; } = false;

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

        public void DrawBackground(CameraDrawContext context, IEnumerable<LocatedModel> models)
        {
            UpdateLocation();

            LocatedDrawContext drawContext = new()
            {
                DeviceContext = context.DeviceContext,
                VertexConstantBuffer = context.VertexConstantBuffer,
                PixelConstantBuffer = context.PixelConstantBuffer,
                PlateOffset = PlateOffset.Identity,
                View = View,
                Projection = CreateProjection(context.ClientSize),
                Light = context.Light,
            };

            foreach (LocatedModel model in models)
            {
                model.Transform = Matrix4x4.CreateTranslation(Transform.Translation);

                model.Draw(drawContext);
            }
        }

        public void DrawPlates(CameraDrawContext context, PlateCollection plates)
        {
            UpdateLocation();

            Matrix4x4 projection = CreateProjection(context.ClientSize);

            for (int i = DrawPlateCount - 1; 0 <= i; i--)
            {
                for (int x = PlateX - i; x <= PlateX + i; x++)
                {
                    int dz = int.Abs(x - PlateX) == i ? 1 : i * 2;
                    for (int z = PlateZ - i; z <= PlateZ + i; z += dz)
                    {
                        if (plates.TryGetValue(x, z, out LocatedPlate? plate))
                        {
                            LocatedDrawContext drawContext = new()
                            {
                                DeviceContext = context.DeviceContext,
                                VertexConstantBuffer = context.VertexConstantBuffer,
                                PixelConstantBuffer = context.PixelConstantBuffer,
                                PlateOffset = new PlateOffset(x - PlateX, z - PlateZ),
                                View = View,
                                Projection = projection,
                                Light = context.Light,
                                DrawColliderDebugModel = DrawColliderDebugModel,
                            };
                            plate!.Plate.Draw(drawContext);
                        }
                    }
                }
            }
        }

        public void DrawBodies(CameraDrawContext context, IEnumerable<RigidBody> bodies)
        {
            UpdateLocation();

            Matrix4x4 projection = CreateProjection(context.ClientSize);

            foreach (RigidBody body in bodies)
            {
                LocatedDrawContext drawContext = new()
                {
                    DeviceContext = context.DeviceContext,
                    VertexConstantBuffer = context.VertexConstantBuffer,
                    PixelConstantBuffer = context.PixelConstantBuffer,
                    PlateOffset = new PlateOffset(body.PlateX - PlateX, body.PlateZ - PlateZ),
                    View = View,
                    Projection = projection,
                    Light = context.Light,
                    DrawColliderDebugModel = DrawColliderDebugModel,
                };
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
