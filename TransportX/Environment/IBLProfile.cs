using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

namespace TransportX.Environment
{
    public class IBLProfile
    {
        public static readonly IBLProfile Default = new()
        {
            Intensity = 0,
            Saturation = 0,
        };


        public required float Intensity { get; init; }
        public required float Saturation { get; init; }

        public IBLProfile()
        {
        }
    }
}
