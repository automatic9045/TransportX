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
    }
}
