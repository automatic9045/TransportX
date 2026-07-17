using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;

namespace TransportX.Domains.RoadVehicles.Powertrain.Controllers
{
    public class ShifterLever<TKey> where TKey : notnull
    {
        public IReadOnlyKeyedList<TKey, ShifterSlot<TKey>> Slots { get; }
        public ShifterSlot<TKey> Position { get; private set; }

        public event PositionChangedEventHandler? PositionChanged;

        public ShifterLever(ShifterSlot<TKey> initialPosition)
        {
            KeyedList<TKey, ShifterSlot<TKey>> slots = new(slot => slot.Key);
            AddSlots(initialPosition);

            Slots = slots;
            Position = initialPosition;


            void AddSlots(ShifterSlot<TKey> root)
            {
                slots.Add(root);

                foreach (ShifterSlot<TKey> child in root.Neighbors.Values)
                {
                    if (slots.Contains(child)) continue;
                    AddSlots(child);
                }
            }
        }

        public bool Move(ShifterDirection direction)
        {
            ShifterSlot<TKey> oldPosition = Position;

            if (Position.Neighbors.TryGetValue(direction, out ShifterSlot<TKey>? neighbor))
            {
                Position = neighbor;
                PositionChanged?.Invoke(oldPosition, Position);
                return true;
            }
            else if (Position.Actions.TryGetValue(direction, out ShifterAction? action))
            {
                action.Invoke();
                return true;
            }

            return false;
        }

        public void MoveTo(TKey slotKey)
        {
            if (!Slots.TryGetValue(slotKey, out ShifterSlot<TKey>? slot))
            {
                throw new ArgumentException("指定されたキーのポジションはシフター内に存在しません。", nameof(slotKey));
            }

            ShifterSlot<TKey> oldPosition = Position;
            Position = slot;
            PositionChanged?.Invoke(oldPosition, Position);
        }


        public delegate void PositionChangedEventHandler(ShifterSlot<TKey> oldPosition, ShifterSlot<TKey> newPosition);
    }
}
