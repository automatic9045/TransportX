using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;
using TransportX.Input;

using TransportX.Scripting.Avatars;
using TransportX.Scripting.Collections;

using TransportX.Domains.RoadVehicles.Powertrain.Controllers;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ControllerFactories
{
    public class ShifterSlotFactory<TKey> where TKey : notnull
    {
        private readonly ScriptAvatar Avatar;

        public ShifterFactory<TKey> Parent { get; }
        public TKey Key { get; }

        private readonly ScriptDictionary<ShifterDirection, ShifterAction> ActionsKey;
        public IReadOnlyScriptDictionary<ShifterDirection, ShifterAction> Actions => ActionsKey;

        private readonly ScriptDictionary<ShifterDirection, ShifterSlotFactory<TKey>> NeighborsKey;
        public IReadOnlyScriptDictionary<ShifterDirection, ShifterSlotFactory<TKey>> Neighbors => NeighborsKey;

        public ShifterSlot<TKey>? BuiltSlot { get; private set; } = null;

        public ShifterSlotFactory(ScriptAvatar avatar, ShifterFactory<TKey> parent, TKey key)
        {
            Avatar = avatar;
            Parent = parent;
            Key = key;

            ActionsKey = new ScriptDictionary<ShifterDirection, ShifterAction>(Avatar.ErrorCollector, "アクション", _ => new(new Signal<int>(0)));
            NeighborsKey = new ScriptDictionary<ShifterDirection, ShifterSlotFactory<TKey>>(Avatar.ErrorCollector, "隣接ポジション",
                direction => new ShifterSlotFactory<TKey>(Avatar, Parent, Parent.NeighborKeyFactoryFallback(Key, direction)));
        }

        public ShifterSlotFactory<TKey> Action(ShifterDirection direction, Signal<int> countSignal)
        {
            ShifterAction action = new(countSignal);
            ActionsKey.Add(direction, action);
            return this;
        }

        public ShifterSlotFactory<TKey> ActionUp(Signal<int> countSignal) => Action(ShifterDirection.Up, countSignal);
        public ShifterSlotFactory<TKey> ActionUp(string countSignalKey) => ActionUp(Avatar.Commander.Signals.Int(countSignalKey));

        public ShifterSlotFactory<TKey> ActionDown(Signal<int> countSignal) => Action(ShifterDirection.Down, countSignal);
        public ShifterSlotFactory<TKey> ActionDown(string countSignalKey) => ActionDown(Avatar.Commander.Signals.Int(countSignalKey));

        public ShifterSlotFactory<TKey> ActionLeft(Signal<int> countSignal) => Action(ShifterDirection.Left, countSignal);
        public ShifterSlotFactory<TKey> ActionLeft(string countSignalKey) => ActionLeft(Avatar.Commander.Signals.Int(countSignalKey));

        public ShifterSlotFactory<TKey> ActionRight(Signal<int> countSignal) => Action(ShifterDirection.Right, countSignal);
        public ShifterSlotFactory<TKey> ActionRight(string countSignalKey) => ActionRight(Avatar.Commander.Signals.Int(countSignalKey));

        public ShifterSlotFactory<TKey> Slot(ShifterDirection direction, TKey key, IButton? slotButton)
        {
            ShifterSlotFactory<TKey> factory = Parent.GetOrCreateSlot(key, slotButton);
            NeighborsKey.Add(direction, factory);
            return factory;
        }

        public ShifterSlotFactory<TKey> SlotUp(TKey key, IButton? slotButton) => Slot(ShifterDirection.Up, key, slotButton);
        public ShifterSlotFactory<TKey> SlotUp(TKey key, string? slotButton) => SlotUp(key, GetButton(slotButton));
        public ShifterSlotFactory<TKey> SlotUp(TKey key) => SlotUp(key, (IButton?)null);

        public ShifterSlotFactory<TKey> SlotUpParallel(TKey key, IButton? slotButton)
        {
            ShifterSlotFactory<TKey> upSlot = SlotUp(key, slotButton);
            upSlot.SlotDown(Key);
            return upSlot;
        }
        public ShifterSlotFactory<TKey> SlotUpParallel(TKey key, string? slotButton) => SlotUpParallel(key, GetButton(slotButton));
        public ShifterSlotFactory<TKey> SlotUpParallel(TKey key) => SlotUpParallel(key, (IButton?)null);

        public ShifterSlotFactory<TKey> SlotDown(TKey key, IButton? slotButton) => Slot(ShifterDirection.Down, key, slotButton);
        public ShifterSlotFactory<TKey> SlotDown(TKey key, string? slotButton) => SlotDown(key, GetButton(slotButton));
        public ShifterSlotFactory<TKey> SlotDown(TKey key) => SlotDown(key, (IButton?)null);

        public ShifterSlotFactory<TKey> SlotDownParallel(TKey key, IButton? slotButton)
        {
            ShifterSlotFactory<TKey> downSlot = SlotDown(key, slotButton);
            downSlot.SlotUp(Key);
            return downSlot;
        }
        public ShifterSlotFactory<TKey> SlotDownParallel(TKey key, string? slotButton) => SlotDownParallel(key, GetButton(slotButton));
        public ShifterSlotFactory<TKey> SlotDownParallel(TKey key) => SlotDownParallel(key, (IButton?)null);

        public ShifterSlotFactory<TKey> SlotLeft(TKey key, IButton? slotButton) => Slot(ShifterDirection.Left, key, slotButton);
        public ShifterSlotFactory<TKey> SlotLeft(TKey key, string? slotButton) => SlotLeft(key, GetButton(slotButton));
        public ShifterSlotFactory<TKey> SlotLeft(TKey key) => SlotLeft(key, (IButton?)null);

        public ShifterSlotFactory<TKey> SlotLeftParallel(TKey key, IButton? slotButton)
        {
            ShifterSlotFactory<TKey> LeftSlot = SlotLeft(key, slotButton);
            LeftSlot.SlotRight(Key);
            return LeftSlot;
        }
        public ShifterSlotFactory<TKey> SlotLeftParallel(TKey key, string? slotButton) => SlotLeftParallel(key, GetButton(slotButton));
        public ShifterSlotFactory<TKey> SlotLeftParallel(TKey key) => SlotLeftParallel(key, (IButton?)null);

        public ShifterSlotFactory<TKey> SlotRight(TKey key, IButton? slotButton) => Slot(ShifterDirection.Right, key, slotButton);
        public ShifterSlotFactory<TKey> SlotRight(TKey key, string? slotButton) => SlotRight(key, GetButton(slotButton));
        public ShifterSlotFactory<TKey> SlotRight(TKey key) => SlotRight(key, (IButton?)null);

        public ShifterSlotFactory<TKey> SlotRightParallel(TKey key, IButton? slotButton)
        {
            ShifterSlotFactory<TKey> rightSlot = SlotRight(key, slotButton);
            rightSlot.SlotLeft(Key);
            return rightSlot;
        }
        public ShifterSlotFactory<TKey> SlotRightParallel(TKey key, string? slotButton) => SlotRightParallel(key, GetButton(slotButton));
        public ShifterSlotFactory<TKey> SlotRightParallel(TKey key) => SlotRightParallel(key, (IButton?)null);

        private IButton? GetButton(string? buttonKey)
        {
            return buttonKey is null ? null
                : Avatar.Commander.Input.Buttons.GetValue(buttonKey, out IButton button) ? button
                : null;
        }

        internal ShifterSlot<TKey> Build()
        {
            if (BuiltSlot is not null) throw new InvalidOperationException();

            Dictionary<ShifterSlotFactory<TKey>, ShifterSlot<TKey>> factoryToBuilt = [];
            Dictionary<ShifterDirection, ShifterSlot<TKey>> keyToBuilt = [];

            BuiltSlot = new ShifterSlot<TKey>(Key, Actions, keyToBuilt);

            foreach ((ShifterDirection direction, ShifterSlotFactory<TKey> factory) in Neighbors)
            {
                if (!factoryToBuilt.TryGetValue(factory, out ShifterSlot<TKey>? built))
                {
                    built = factory.BuiltSlot ?? factory.Build();
                }

                keyToBuilt[direction] = built;
            }

            return BuiltSlot;
        }
    }
}
