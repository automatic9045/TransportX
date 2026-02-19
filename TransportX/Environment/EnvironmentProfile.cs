using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Environment
{
    public class EnvironmentProfile : IDisposable
    {
        public static readonly EnvironmentProfile Default = new()
        {
            IBL = IBL.Default,
            Bloom = Bloom.Default,
        };


        public required IBL IBL { get; init; }
        public required Bloom Bloom { get; init; }

        public EnvironmentProfile()
        {
        }

        public void Dispose()
        {
            IBL.Dispose();
        }
    }
}
