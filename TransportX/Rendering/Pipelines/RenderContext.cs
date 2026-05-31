using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering.Pipelines
{
    public class RenderContext
    {
        private GraphicsPipelineState CurrentState = GraphicsPipelineState.Empty;

        public ID3D11DeviceContext DeviceContext { get; }

        public RenderContext(ID3D11DeviceContext deviceContext)
        {
            DeviceContext = deviceContext;
        }

        public void ApplyState(GraphicsPipelineState newState)
        {
            if (CurrentState.VertexShader != newState.VertexShader)
            {
                DeviceContext.VSSetShader(newState.VertexShader);
            }

            if (CurrentState.PixelShader != newState.PixelShader)
            {
                DeviceContext.PSSetShader(newState.PixelShader);
            }

            if (CurrentState.InputLayout != newState.InputLayout)
            {
                DeviceContext.IASetInputLayout(newState.InputLayout);
            }

            if (CurrentState.RasterizerState != newState.RasterizerState)
            {
                DeviceContext.RSSetState(newState.RasterizerState);
            }

            if (CurrentState.BlendState != newState.BlendState
                || CurrentState.BlendFactor != newState.BlendFactor
                || CurrentState.SampleMask != newState.SampleMask)
            {
                DeviceContext.OMSetBlendState(newState.BlendState, newState.BlendFactor, newState.SampleMask);
            }

            if (CurrentState.DepthStencilState != newState.DepthStencilState ||
                CurrentState.StencilReference != newState.StencilReference)
            {
                DeviceContext.OMSetDepthStencilState(newState.DepthStencilState, newState.StencilReference);
            }

            if (CurrentState.PrimitiveTopology != newState.PrimitiveTopology)
            {
                DeviceContext.IASetPrimitiveTopology(newState.PrimitiveTopology);
            }

            CurrentState = newState;
        }

        public void InvalidateState()
        {
            CurrentState = GraphicsPipelineState.Empty;
        }
    }
}
