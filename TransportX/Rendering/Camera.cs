using System;
using System.Collections.Generic;
using GdiSize = System.Drawing.Size;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.Mathematics;
using Vortice.XAudio2;

using TransportX.Bodies;
using TransportX.Spatial;

namespace TransportX.Rendering
{
    public class Camera : LocatableObject
    {
        private Matrix4x4 View = default;

        public int DrawPlateCount { get; set; } = 3;
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

        {
            if (!VisibleLayers.HasFlag(VisualLayers.Normal)) return;

        public void DrawBackground(in CameraDrawContext context, IEnumerable<LocatedModel> models)
            Matrix4x4 projection = CreateProjection(context.ClientSize);
            BoundingFrustum frustum = new(View * projection);

            SetPixelShader(context, RenderPass.Normal);

            LocatedDrawContext drawContext = new()
            {
                DeviceContext = context.DeviceContext,
                TransformBuffer = context.TransformBuffer,
                MaterialBuffer = context.MaterialBuffer,
                PlateOffset = PlateOffset.Identity,
                View = View,
                Projection = projection,
                Frustum = frustum,
            };

            foreach (LocatedModel model in models)
            {
                model.Pose = new Pose(Pose.Position);
                model.Draw(drawContext);
            }
        }

        public void DrawPlates(in CameraDrawContext context, PlateCollection plates)
        {
            Matrix4x4 projection = CreateProjection(context.ClientSize);
            BoundingFrustum frustum = new(View * projection);

            if (VisibleLayers.HasFlag(VisualLayers.Normal)) Draw(context, RenderPass.Normal);
            if (VisibleLayers.HasFlag(VisualLayers.Colliders)) Draw(context, RenderPass.Colliders);
            if (VisibleLayers.HasFlag(VisualLayers.Network)) Draw(context, RenderPass.Network);


            void Draw(in CameraDrawContext context, RenderPass pass)
            {
                SetPixelShader(context, pass);

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
                                    TransformBuffer = context.TransformBuffer,
                                    MaterialBuffer = context.MaterialBuffer,
                                    PlateOffset = new PlateOffset(x - PlateX, z - PlateZ),
                                    View = View,
                                    Projection = projection,
                                    Frustum = frustum,
                                    Pass = pass,
                                };
                                plate!.Draw(drawContext);
                            }
                        }
                    }
                }
            }
        }

        public void DrawBodies(in CameraDrawContext context, IEnumerable<RigidBody> bodies)
        {
            Matrix4x4 projection = CreateProjection(context.ClientSize);
            BoundingFrustum frustum = new(View * projection);

            foreach (RigidBody body in bodies)
            {
                LocatedDrawContext drawContext = new()
                {
                    DeviceContext = context.DeviceContext,
                    TransformBuffer = context.TransformBuffer,
                    MaterialBuffer = context.MaterialBuffer,
                    PlateOffset = new PlateOffset(body.PlateX - PlateX, body.PlateZ - PlateZ),
                    View = View,
                    Projection = projection,
                    Frustum = frustum,
                    Pass = RenderPass.Normal,
                };

                if (VisibleLayers.HasFlag(VisualLayers.Normal))
                {
                    SetPixelShader(context, drawContext.Pass);
                    body.Draw(drawContext);
                }

                if (VisibleLayers.HasFlag(VisualLayers.Colliders))
                {
                    drawContext = drawContext with
                    {
                        Pass = RenderPass.Colliders,
                    };
                    SetPixelShader(context, drawContext.Pass);
                    body.Draw(drawContext);
                }

                if (VisibleLayers.HasFlag(VisualLayers.Traffic))
                {
                    drawContext = drawContext with
                    {
                        Pass = RenderPass.Traffic,
                    };
                    SetPixelShader(context, drawContext.Pass);
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

        protected void SetPixelShader(in CameraDrawContext context, RenderPass pass)
        {
            ID3D11PixelShader shader = pass == RenderPass.Normal ? context.PixelShader : context.DebugPixelShader;
            context.DeviceContext.PSSetShader(shader);
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
