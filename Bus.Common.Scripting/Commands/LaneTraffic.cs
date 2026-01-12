using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Scripting.Commands
{
    public class LaneTraffic
    {
        private readonly ScriptWorld World;

        private readonly ScriptDictionary<string, LaneTrafficType> TypesKey;
        public IReadOnlyScriptDictionary<string, LaneTrafficType> Types => TypesKey;

        private readonly ScriptDictionary<string, LaneTrafficGroup> GroupsKey;
        public IReadOnlyScriptDictionary<string, LaneTrafficGroup> Groups => GroupsKey;

        internal LaneTraffic(ScriptWorld world)
        {
            World = world;

            TypesKey = new(World.ErrorCollector, "進路種別", key => LaneTrafficType.Empty());
            GroupsKey = new(World.ErrorCollector, "進路種別グループ", key => new LaneTrafficGroup());
        }

        public LaneTrafficType AddType(string key, LaneTrafficType type, bool generateGroup = true)
        {
            TypesKey.Add(key, type);
            if (generateGroup)
            {
                LaneTrafficGroup group = new(type);
                AddGroup(key, group);
            }

            return type;
        }

        public LaneTrafficType AddType(string key, string name, bool generateGroup = true)
        {
            LaneTrafficType type = new(name);
            return AddType(key, type, generateGroup);
        }

        public LaneTrafficGroup AddGroup(string key, LaneTrafficGroup group)
        {
            GroupsKey.Add(key, group);
            return group;
        }

        public LaneTrafficGroup AddGroup(string key, string typeKeys)
        {
            string[] typeKeysSplitted = typeKeys.Split('|', StringSplitOptions.TrimEntries);
            IEnumerable<LaneTrafficType> types = typeKeysSplitted
                .Select(key => TypesKey.GetValue(key, out LaneTrafficType type) ? type : null)
                .Where(type => type is not null)
                .Cast<LaneTrafficType>();

            LaneTrafficGroup group = new(types);
            return AddGroup(key, group);
        }
    }
}
