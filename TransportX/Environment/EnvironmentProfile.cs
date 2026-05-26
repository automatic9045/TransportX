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
            Exposure = ExposureProfile.Default,
            ToneMap = ToneMapProfile.Default,
            Bloom = BloomProfile.Default,
        };


        public required IBLProfile IBL { get; init; }
        public required ExposureProfile Exposure { get; init; }
        public required ToneMapProfile ToneMap { get; init; }
        public required BloomProfile Bloom { get; init; }

        public EnvironmentProfile()
        {
        }
    }
}
