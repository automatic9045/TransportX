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
        public int DrawPlateCount { get; set; } = 3;
        public Listener Listener { get; } = new Listener();
        public ViewpointSet Viewpoints { get; } = new ViewpointSet();

        public VisualLayers VisibleLayers { get; set; } = VisualLayers.Normal;

        public Matrix4x4 View { get; protected set; } = default;
        public Matrix4x4 Projection { get; protected set; } = default;
        public BoundingFrustum Frustum { get; protected set; } = default;

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

        public void UpdateView()
        {
            Locate(Viewpoints.Current.Source, Viewpoints.Current.Pose);
        }

        public void UpdateProjection(GdiSize clientSize)
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                Viewpoints.Current.Perspective * MathHelper.ToRadians(45), (float)clientSize.Width / clientSize.Height, 0.1f, 1000);
            Frustum = new(View * Projection);
        }

        public void DrawBackground(in CameraDrawContext context, IEnumerable<LocatedModel> models)
        {
            if (!VisibleLayers.HasFlag(VisualLayers.Normal)) return;

            SetPixelShader(context, RenderPass.Normal);

            LocatedDrawContext drawContext = new()
            {
                DeviceContext = context.DeviceContext,
                SingleInstanceBuffer = context.SingleInstanceBuffer,
                MaterialBuffer = context.MaterialBuffer,
                PlateOffset = PlateOffset.Identity,
                View = View,
                Projection = Projection,
                Frustum = Frustum,
            };

            foreach (LocatedModel model in models)
            {
                model.Pose = new Pose(Pose.Position);
                model.Draw(drawContext);
            }
        }

        public void DrawPlates(in CameraDrawContext context, PlateCollection plates)
        {
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
                                    SingleInstanceBuffer = context.SingleInstanceBuffer,
                                    MaterialBuffer = context.MaterialBuffer,
                                    PlateOffset = new PlateOffset(x - PlateX, z - PlateZ),
                                    View = View,
                                    Projection = Projection,
                                    Frustum = Frustum,
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
            foreach (RigidBody body in bodies)
            {
                LocatedDrawContext drawContext = new()
                {
                    DeviceContext = context.DeviceContext,
                    SingleInstanceBuffer = context.SingleInstanceBuffer,
                    MaterialBuffer = context.MaterialBuffer,
                    PlateOffset = new PlateOffset(body.PlateX - PlateX, body.PlateZ - PlateZ),
                    View = View,
                    Projection = Projection,
                    Frustum = Frustum,
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
