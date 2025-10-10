using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Commands
{
    public class Splines
    {
        private readonly ScriptWorld World;

        private readonly Dictionary<string, SplineTemplate> TemplatesKey = new();
        public IReadOnlyDictionary<string, SplineTemplate> Templates => TemplatesKey;

        private readonly Dictionary<string, LaneConnector> PortsKey = new();
        public IReadOnlyDictionary<string, LaneConnector> Ports => PortsKey;

        internal Splines(ScriptWorld world)
        {
            World = world;
        }

        public SplineTemplate CreateTemplate(string key, string portKey)
        {
            SplineTemplate template = new SplineTemplate(World, portKey);
            TemplatesKey.Add(key, template);
            return template;
        }
    }
}
