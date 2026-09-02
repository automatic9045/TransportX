using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

using TransportX.Rendering.Backend;
using TransportX.Rendering.Pipelines;
using TransportX.Spatial;

namespace TransportX.Domains.Equipment.Cameras
{
    public class SceneCaptureCamera : ISceneCaptureCamera
    {
        public IWorldObject AttachedTo { get; }

        public RenderTexture RenderTarget { get; }
        public DepthTexture DepthTarget { get; }

        public float FieldOfView { get; set; } = float.Pi / 4;
        public float AspectRatio { get; set; } = 1;
        public RenderPassFlags RenderFlags { get; set; } = RenderPassFlags.None;

        public SceneCaptureCamera(IWorldObject attachedTo, RenderTexture renderTarget, DepthTexture depthTarget)
        {
            AttachedTo = attachedTo;
            RenderTarget = renderTarget;
            DepthTarget = depthTarget;
        }

        public SceneCaptureCamera(ID3D11Device device, IWorldObject attachedTo, SizeI textureSize)
        {
            AttachedTo = attachedTo;

            Texture2DDescription renderDesc = new()
            {
                Width = (uint)textureSize.Width,
                Height = (uint)textureSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R11G11B10_Float,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
            };
            RenderTarget = new(device, renderDesc);

            Texture2DDescription depthDesc = new()
            {
                Width = (uint)textureSize.Width,
                Height = (uint)textureSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
            };
            DepthTarget = new(device, depthDesc);
        }

        public void Dispose()
        {
            RenderTarget.Dispose();
            DepthTarget.Dispose();
        }

        public ViewContext CreateViewContext(in ViewContext cameraViewContext)
        {
            WorldPose pose = AttachedTo.WorldPose;
            Vector3 positionInMonitorChunk = pose.Pose.Position;

            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(FieldOfView, AspectRatio, 0.1f, 100);
            Vector3 cameraDirection;

            if (RenderFlags.HasFlag(RenderPassFlags.Reflect))
            {
                Vector3 positionInCameraChunk = cameraViewContext.WorldPose.Pose.Position + cameraViewContext.WorldPose.GetOffset(pose);
                Vector3 cameraPosition = cameraViewContext.WorldPose.Pose.Position;

                Vector3 viewDirection = cameraPosition == positionInCameraChunk ? pose.Pose.Direction : Vector3.Normalize(positionInCameraChunk - cameraPosition);
                cameraDirection = Vector3.Reflect(viewDirection, pose.Pose.Direction);

                projection.M11 = -projection.M11;
            }
            else
            {
                cameraDirection = pose.Pose.Direction;
            }

            Matrix4x4 view = Matrix4x4.CreateLookAtLeftHanded(positionInMonitorChunk, positionInMonitorChunk + cameraDirection, pose.Pose.Up);
            Pose monitorCameraPose = Pose.CreateWorldLH(positionInMonitorChunk, cameraDirection, pose.Pose.Up);
            WorldPose monitorCameraWorldPose = new(pose.Chunk, monitorCameraPose);

            return cameraViewContext with
            {
                View = view,
                WorldPose = monitorCameraWorldPose,
                Projection = projection,
            };
        }
    }
}
