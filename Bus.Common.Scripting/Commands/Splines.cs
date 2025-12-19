using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Commands
{
    public class Splines
    {
        private readonly ScriptWorld World;

        private readonly Dictionary<string, SplineTemplate> TemplatesKey = [];
        public IReadOnlyDictionary<string, SplineTemplate> Templates => TemplatesKey;

        private readonly Dictionary<string, LaneLayout> LayoutsKey = [];
        public IReadOnlyDictionary<string, LaneLayout> Layouts => LayoutsKey;

        internal Splines(ScriptWorld world)
        {
            World = world;
        }

        public SplineTemplate CreateTemplate(string key, string layoutKey)
        {
            if (!Layouts.TryGetValue(layoutKey, out LaneLayout? layout))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデル '{layoutKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                layout = new LaneLayout();
            }

            SplineTemplate template = new SplineTemplate(World, layout);
            TemplatesKey.Add(key, template);
            return template;
        }
    }
}
