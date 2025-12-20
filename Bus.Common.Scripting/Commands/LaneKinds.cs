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
    public class LaneKinds : IReadOnlyDictionary<string, LaneKind>
    {
        private readonly ScriptWorld World;
        private readonly Dictionary<string, LaneKind> Items = [];

        public LaneKind this[string key] => Items[key];
        public IEnumerable<string> Keys => Items.Keys;
        public IEnumerable<LaneKind> Values => Items.Values;
        public int Count => Items.Count;

        internal LaneKinds(ScriptWorld world)
        {
            World = world;
        }

        public LaneKind? Add(string key, LaneKind kind)
        {
            try
            {
                Items.Add(key, kind);
                return kind;
            }
            catch (ArgumentException ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex, $"キー '{key}' の進路種別は既に存在します。");
                World.ErrorCollector.Report(error);
                return null;
            }
        }

        public LaneKind? Add(string key, string baseKeys)
        {
            string[] baseKeysSplitted = baseKeys.Split('+', StringSplitOptions.TrimEntries);
            LaneKind? kind = null;
            foreach (string baseKey in baseKeysSplitted)
            {
                LaneKind baseKind = Items[baseKey];
                kind = kind is null ? baseKind : kind + baseKind;
            }

            if (kind is null)
            {
                ScriptError error = new(ErrorLevel.Error, $"指定された複合キー '{baseKeys}' が空です。");
                World.ErrorCollector.Report(error);
                return null;
            }

            Items.Add(key, kind);
            return kind;
        }

        public RootLaneKind? Create(string key, string name)
        {
            RootLaneKind kind = new(name);
            return (RootLaneKind?)Add(key, kind);
        }

        public bool ContainsKey(string key) => Items.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, LaneKind>> GetEnumerator() => Items.GetEnumerator();
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out LaneKind value) => Items.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
