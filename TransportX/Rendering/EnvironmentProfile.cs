using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Rendering
{
    public class EnvironmentProfile : IDisposable
    {
        public static readonly EnvironmentProfile Default = new()
        {
            DiffuseTexture = null,
            SpecularTexture = null,
            Intensity = 0,
            Saturation = 0,
        };


        public required ID3D11ShaderResourceView? DiffuseTexture { get; init; }
        public required ID3D11ShaderResourceView? SpecularTexture { get; init; }
        public required float Intensity { get; init; }
        public required float Saturation { get; init; }

        public EnvironmentProfile()
        {
        }

        public void Dispose()
        {
            DiffuseTexture?.Dispose();
            SpecularTexture?.Dispose();
        }
    }
}
