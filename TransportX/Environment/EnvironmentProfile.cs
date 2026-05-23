using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Environment
{
    public class EnvironmentProfile
    {
        public static readonly EnvironmentProfile Default = new()
        {
            IBL = IBLProfile.Default,
            Bloom = BloomProfile.Default,
        };


        public required IBLProfile IBL { get; init; }
        public required BloomProfile Bloom { get; init; }

        public EnvironmentProfile()
        {
        }
    }
}
