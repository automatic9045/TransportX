using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace TransportX.Rendering.Pipelines
{
    public record class GraphicsPipelineState : IDisposable
    {
        public static readonly GraphicsPipelineState Empty = new()
        {
            VertexShader = null,
            PixelShader = null,
            InputLayout = null,

            RasterizerState = null,
            BlendState = null,
            DepthStencilState = null,
        };


        public required ID3D11VertexShader? VertexShader { get; init; }
        public required ID3D11PixelShader? PixelShader { get; init; }
        public required ID3D11InputLayout? InputLayout { get; init; }

        public required ID3D11RasterizerState? RasterizerState { get; init; }
        public required ID3D11BlendState? BlendState { get; init; }
        public required ID3D11DepthStencilState? DepthStencilState { get; init; }

        public Color4 BlendFactor { get; init; } = Colors.White;
        public uint SampleMask { get; init; } = 0xffffffff;
        public uint StencilReference { get; init; } = 0;

        public PrimitiveTopology PrimitiveTopology { get; init; } = PrimitiveTopology.TriangleList;

        public void Dispose()
        {
            VertexShader?.Dispose();
            PixelShader?.Dispose();
            InputLayout?.Dispose();

            RasterizerState?.Dispose();
            BlendState?.Dispose();
            DepthStencilState?.Dispose();
        }
    }
}
