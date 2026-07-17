using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Powertrain.Controllers
{
    public class ShifterSlot<TKey> where TKey : notnull
    {
        public TKey Key { get; }

        public IReadOnlyDictionary<ShifterDirection, ShifterAction> Actions { get; }
        public IReadOnlyDictionary<ShifterDirection, ShifterSlot<TKey>> Neighbors { get; }

        public ShifterSlot(TKey key, IReadOnlyDictionary<ShifterDirection, ShifterAction> actions, IReadOnlyDictionary<ShifterDirection, ShifterSlot<TKey>> neighbors)
        {
            Key = key;
            Actions = actions;
            Neighbors = neighbors;

            foreach (ShifterDirection actionDirection in Actions.Keys)
            {
                if (Neighbors.ContainsKey(actionDirection))
                {
                    throw new ArgumentException($"方向 '{actionDirection}' にアクション、隣接スロットの両方が定義されています。");
                }
            }
        }

        public static ShifterSlot<TKey> Empty(TKey key)
            => new(key, new Dictionary<ShifterDirection, ShifterAction>(), new Dictionary<ShifterDirection, ShifterSlot<TKey>>());
    }
}
