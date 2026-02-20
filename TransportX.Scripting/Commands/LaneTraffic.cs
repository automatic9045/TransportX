using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Extensions.Utilities;

namespace TransportX.Scripting.Commands
{
    public class LaneTraffic
    {
        private readonly ScriptWorld World;

        private readonly ScriptDictionary<string, LaneTrafficType> TypesKey;
        public IReadOnlyScriptDictionary<string, LaneTrafficType> Types => TypesKey;

        private readonly ScriptDictionary<string, LaneTrafficGroup> GroupsKey;
        public IReadOnlyScriptDictionary<string, LaneTrafficGroup> Groups => GroupsKey;

        private readonly Dictionary<LaneTrafficGroup, Vector4> GroupColors = [];

        internal LaneTraffic(ScriptWorld world)
        {
            World = world;

            TypesKey = new(World.ErrorCollector, "進路種別", key => LaneTrafficType.Empty());
            GroupsKey = new(World.ErrorCollector, "進路種別グループ", key => new LaneTrafficGroup());
        }

        public LaneTrafficType AddType(string key, LaneTrafficType type, string debugColor)
        {
            TypesKey.Add(key, type);

            LaneTrafficGroup group = new(type);
            AddGroup(key, group, debugColor);

            return type;
        }

        public LaneTrafficType AddType(string key, string name, string debugColor)
        {
            LaneTrafficType type = new(name);
            return AddType(key, type, debugColor);
        }

        public LaneTrafficGroup AddGroup(string key, LaneTrafficGroup group, string debugColor)
        {
            Color color = Color.White;
            try
            {
                color = ColorTranslator.FromHtml(debugColor);
            }
            catch
            {
                ScriptError error = new(ErrorLevel.Error, $"カラーコード '{debugColor}' は無効です。");
                World.ErrorCollector.Report(error);
            }

            GroupsKey.Add(key, group);
            GroupColors.Add(group, color.ToVector4());
            return group;
        }

        public LaneTrafficGroup AddGroup(string key, string typeKeys, string debugColor)
        {
            string[] typeKeysSplitted = typeKeys.Split('|', StringSplitOptions.TrimEntries);
            IEnumerable<LaneTrafficType> types = typeKeysSplitted
                .Select(key => TypesKey.GetValue(key, out LaneTrafficType type) ? type : null)
                .Where(type => type is not null)
                .Cast<LaneTrafficType>();

            LaneTrafficGroup group = new(types);
            return AddGroup(key, group, debugColor);
        }

        public Vector4 GetGroupColor(LaneTrafficGroup group)
        {
            return GroupColors.TryGetValue(group, out Vector4 color) ? color : Vector4.One;
        }
    }
}
