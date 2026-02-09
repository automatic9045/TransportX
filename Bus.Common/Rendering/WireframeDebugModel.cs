using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Bus.Common.Rendering
{
    public class WireframeDebugModel : IDebugModel
    {
        private static ID3D11Device? TargetDevice = null;
        private static int ReferenceCount = 0;

        protected static ID3D11DepthStencilState? DepthState = null;
        protected static ID3D11BlendState? AlphaBlendState = null;
        protected static ID3D11RasterizerState? RasterizerState = null;


        protected readonly IEnumerable<IMesh> Meshes;

        public virtual string? DebugName
        {
            get => field;
            set
            {
                field = value;
                foreach (IMesh mesh in Meshes) mesh.DebugName = value;
            }
        } = null;

        public virtual Vector4 Color
        {
            get => field;
            set
            {
                field = value;
                foreach (IMesh mesh in Meshes) mesh.Material.BaseColor = value;
            }
        } = Vector4.One;

        public WireframeDebugModel(IEnumerable<IMesh> meshes)
        {
            ReferenceCount++;
            Meshes = meshes;
        }

        public static WireframeDebugModel CreateBoundingBox(ID3D11Device device, Material material, Vector3 min, Vector3 max)
        {
            Vector3[] points = [
                new(min.X, min.Y, min.Z),
                new(max.X, min.Y, min.Z),
                new(min.X, max.Y, min.Z),
                new(max.X, max.Y, min.Z),
                new(min.X, min.Y, max.Z),
                new(max.X, min.Y, max.Z),
                new(min.X, max.Y, max.Z),
                new(max.X, max.Y, max.Z),
            ];

            Vertex[] vertices = points.Select(p => new Vertex(p, Vector4.One)).ToArray();

            int[] indices = [
                0, 1, 1, 3, 3, 2, 2, 0,
                4, 5, 5, 7, 7, 6, 6, 4,
                0, 4, 1, 5, 2, 6, 3, 7,
            ];

            Mesh mesh = Mesh.Create(device, vertices, indices, material, PrimitiveTopology.LineList);
            return new WireframeDebugModel([mesh]);
        }

        public virtual void Dispose()
        {
            foreach (IMesh mesh in Meshes) mesh.Dispose();

            ReferenceCount--;
            if (ReferenceCount == 0)
            {
                TargetDevice = null;

                DepthState?.Dispose();
                AlphaBlendState?.Dispose();
                RasterizerState?.Dispose();

                DepthState = null;
                AlphaBlendState = null;
                RasterizerState = null;
            }
        }

        public virtual void Draw(DrawContext context)
        {
            if (TargetDevice != context.DeviceContext.Device)
            {
                DepthState?.Dispose();
                AlphaBlendState?.Dispose();
                RasterizerState?.Dispose();

                TargetDevice = context.DeviceContext.Device;

                DepthStencilDescription dDesc = new()
                {
                    DepthEnable = false,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthFunc = ComparisonFunction.Always,
                    StencilEnable = false,
                };
                DepthState = context.DeviceContext.Device.CreateDepthStencilState(dDesc);

                BlendDescription bDesc = new();
                bDesc.RenderTarget[0] = new RenderTargetBlendDescription()
                {
                    BlendEnable = true,
                    SourceBlend = Blend.SourceAlpha,
                    DestinationBlend = Blend.InverseSourceAlpha,
                    BlendOperation = BlendOperation.Add,
                    SourceBlendAlpha = Blend.One,
                    DestinationBlendAlpha = Blend.Zero,
                    BlendOperationAlpha = BlendOperation.Add,
                    RenderTargetWriteMask = ColorWriteEnable.All,
                };
                AlphaBlendState = context.DeviceContext.Device.CreateBlendState(bDesc);

                RasterizerDescription rsDesc = new()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Wireframe,
                    DepthClipEnable = true,
                };
                RasterizerState = context.DeviceContext.Device.CreateRasterizerState(rsDesc);
            }

            context.DeviceContext.OMGetDepthStencilState(out ID3D11DepthStencilState? oldDState, out uint oldRef);
            ID3D11BlendState? oldBState = context.DeviceContext.OMGetBlendState(out Color4 oldBFactor, out uint oldBMask);
            ID3D11RasterizerState? oldRSState = context.DeviceContext.RSGetState();

            context.DeviceContext.OMSetDepthStencilState(DepthState);
            context.DeviceContext.OMSetBlendState(AlphaBlendState);
            context.DeviceContext.RSSetState(RasterizerState);

            foreach (IMesh mesh in Meshes) mesh.Draw(context);

            context.DeviceContext.OMSetDepthStencilState(oldDState, oldRef);
            context.DeviceContext.OMSetBlendState(oldBState, oldBFactor, oldBMask);
            context.DeviceContext.RSSetState(oldRSState);

            oldDState?.Dispose();
            oldBState?.Dispose();
            oldRSState?.Dispose();
        }
    }
}
