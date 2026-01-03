using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Commands
{
    public class Templates
    {
        private readonly ScriptWorld World;

        private readonly Dictionary<string, SplineTemplate> SplinesKey = [];
        public IReadOnlyDictionary<string, SplineTemplate> Splines => SplinesKey;

        private readonly Dictionary<string, JunctionTemplate> JunctionsKey = [];
        public IReadOnlyDictionary<string, JunctionTemplate> Junctions => JunctionsKey;

        internal Templates(ScriptWorld world)
        {
            World = world;
        }

        public SplineTemplate CreateSpline(string key, string layoutKey)
        {
            SplineTemplate template = new(World, layoutKey);
            SplinesKey.Add(key, template);
            return template;
        }

        public JunctionTemplate CreateJunction(string key, string inletKey, string inletLayoutKey)
        {
            JunctionTemplate template = new(World, inletKey, inletLayoutKey);
            JunctionsKey.Add(key, template);
            return template;
        }
    }
}
