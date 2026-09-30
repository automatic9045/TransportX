using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    public class RenderPassPurpose
    {
        public static readonly RenderPassPurpose Main = new("Main");
        public static readonly RenderPassPurpose IBL = new("IBL");
        public static readonly RenderPassPurpose ShadowDepth = new("ShadowDepth");
        public static readonly RenderPassPurpose Debug = new("Debug");


        public string Name { get; }

        public RenderPassPurpose(string name)
        {
            Name = name;
        }
    }
}
