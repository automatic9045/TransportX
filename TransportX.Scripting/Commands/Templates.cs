using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Commands
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

        public JunctionTemplate CreateJunction(string key)
        {
            JunctionTemplate template = new(World);
            JunctionsKey.Add(key, template);
            return template;
        }

        internal SplineTemplate? GetSpline(string key)
        {
            if (!Splines.TryGetValue(key, out SplineTemplate? template))
            {
                ScriptError error = new(ErrorLevel.Error, $"スプラインテンプレート '{key}' が見つかりません。");
                World.ErrorCollector.Report(error);

                return null;
            }

            return template;
        }

        internal JunctionTemplate? GetJunction(string key)
        {
            if (!Junctions.TryGetValue(key, out JunctionTemplate? template))
            {
                ScriptError error = new(ErrorLevel.Error, $"ジャンクションテンプレート '{key}' が見つかりません。");
                World.ErrorCollector.Report(error);

                return null;
            }

            return template;
        }
    }
}
