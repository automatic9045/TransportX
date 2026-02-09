using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;
using Vortice.XAudio2;

using GdiSize = System.Drawing.Size;

using TransportX.Bodies;
using TransportX.Spatial;

namespace TransportX.Rendering
{
    public class Camera : LocatableObject
    {
        private Matrix4x4 View = default;

        public int DrawPlateCount { get; set; } = 2;
        public Listener Listener { get; } = new Listener();
        public ViewpointSet Viewpoints { get; } = new ViewpointSet();

        public VisualLayers VisibleLayers { get; set; } = VisualLayers.Normal;

        public Camera() : base()
        {
            Moved += (sender, e) =>
            {
                Listener.OrientFront = Pose.Direction;
                Listener.OrientTop = Pose.Up;
                Listener.Position = Pose.Position;
                Listener.Velocity = Velocity;

                View = Pose.Inverse(Pose).ToMatrix4x4();
            };
        }

        public void UpdateLocation()
        {
            Locate(Viewpoints.Current.Source, Viewpoints.Current.Pose);
        }

        public void DrawBackground(CameraDrawContext context, IEnumerable<LocatedModel> models)
        {
            if (!VisibleLayers.HasFlag(VisualLayers.Normal)) return;

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
                model.Pose = new Pose(Pose.Position);
                model.Draw(drawContext);
            }
        }

        public void DrawPlates(CameraDrawContext context, PlateCollection plates)
        {
            Matrix4x4 projection = CreateProjection(context.ClientSize);

            if (VisibleLayers.HasFlag(VisualLayers.Normal)) Draw(RenderPass.Normal);
            if (VisibleLayers.HasFlag(VisualLayers.Colliders)) Draw(RenderPass.Colliders);
            if (VisibleLayers.HasFlag(VisualLayers.Network)) Draw(RenderPass.Network);


            void Draw(RenderPass pass)
            {
                for (int i = DrawPlateCount - 1; 0 <= i; i--)
                {
                    for (int x = PlateX - i; x <= PlateX + i; x++)
                    {
                        int dz = int.Abs(x - PlateX) == i ? 1 : i * 2;
                        for (int z = PlateZ - i; z <= PlateZ + i; z += dz)
                        {
                            if (plates.TryGetValue(x, z, out Plate? plate))
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
                                    Pass = pass,
                                };
                                plate!.Draw(drawContext);
                            }
                        }
                    }
                }
            }
        }

        public void DrawBodies(CameraDrawContext context, IEnumerable<RigidBody> bodies)
        {
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
                    Pass = RenderPass.Normal,
                };

                if (VisibleLayers.HasFlag(VisualLayers.Normal))
                {
                    body.Draw(drawContext);
                }

                if (VisibleLayers.HasFlag(VisualLayers.Colliders))
                {
                    drawContext = drawContext with
                    {
                        Pass = RenderPass.Colliders,
                    };
                    body.Draw(drawContext);
                }

                if (VisibleLayers.HasFlag(VisualLayers.Traffic))
                {
                    drawContext = drawContext with
                    {
                        Pass = RenderPass.Traffic,
                    };
                    body.Draw(drawContext);
                }
            }
        }

        protected Matrix4x4 CreateProjection(GdiSize clientSize)
        {
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                Viewpoints.Current.Perspective * MathHelper.ToRadians(45), (float)clientSize.Width / clientSize.Height, 0.1f, 1000);
            return projection;
        }


        [Flags]
        public enum VisualLayers
        {
            None = 0b0000,
            Normal = 0x0001,
            Colliders = 0b0010,
            Network = 0b0100,
            Traffic = 0b1000,
        }
    }
}
