using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Input;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Collections;
using TransportX.Scripting.Commands;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Controllers;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ControllerFactories
{
    public class ShifterFactory<TKey> : ControllerFactoryBase where TKey : notnull
    {
        private readonly TKey DefaultKey;

        private readonly ScriptKeyedList<TKey, ShifterSlotFactory<TKey>> SlotsKey;
        private readonly ScriptDictionary<ShifterDirection, IButton> DirectionButtonsKey;
        private readonly ScriptDictionary<TKey, IButton> SlotButtonsKey;

        private bool IsRootSlotDefined = false;

        internal Func<TKey, ShifterDirection, TKey> NeighborKeyFactoryFallback { get; }
        public ShifterSlotFactory<TKey> InitialPosition { get; private set; }

        public ScriptKeyedList<TKey, ShifterSlotFactory<TKey>> Slots => SlotsKey;
        public IReadOnlyScriptDictionary<ShifterDirection, IButton> DirectionButtons => DirectionButtonsKey;
        public IReadOnlyScriptDictionary<TKey, IButton> SlotButtons => SlotButtonsKey;

        public new Shifter<TKey>? BuiltController { get; private set; } = null;

        public ShifterFactory(ScriptAvatar avatar, string key, TKey defaultKey, Func<TKey, ShifterDirection, TKey> neighborKeyFactoryFallback) : base(avatar, key)
        {
            DefaultKey = defaultKey;
            NeighborKeyFactoryFallback = neighborKeyFactoryFallback;

            InitialPosition = new ShifterSlotFactory<TKey>(Avatar, this, DefaultKey);

            SlotsKey = new ScriptKeyedList<TKey, ShifterSlotFactory<TKey>>(
                slot => slot.Key, Avatar.ErrorCollector, "ポジション", key => new ShifterSlotFactory<TKey>(Avatar, this, key));
            DirectionButtonsKey = new ScriptDictionary<ShifterDirection, IButton>(
                Avatar.ErrorCollector, "方向ボタン", direction => IButton.Empty($"ShifterDirectionButton_{direction}_{Guid.NewGuid()}"));
            SlotButtonsKey = new ScriptDictionary<TKey, IButton>(
                Avatar.ErrorCollector, "ポジションボタン", key => IButton.Empty($"ShifterSlotButton_{key}_{Guid.NewGuid()}"));
        }

        public ShifterFactory<TKey> UpDownButton(ButtonCommand? up, ButtonCommand? down)
        {
            if (up is not null) DirectionButtonsKey[ShifterDirection.Up] = up.Button;
            if (down is not null) DirectionButtonsKey[ShifterDirection.Down] = down.Button;
            return this;
        }

        public ShifterFactory<TKey> UpDownButton(string? upButtonKey, string? downButtonKey)
        {
            ButtonCommand? up = GetButton(upButtonKey);
            ButtonCommand? down = GetButton(downButtonKey);
            return UpDownButton(up, down);
        }

        public ShifterFactory<TKey> LeftRightButton(ButtonCommand? left, ButtonCommand? right)
        {
            if (left is not null) DirectionButtonsKey[ShifterDirection.Left] = left.Button;
            if (right is not null) DirectionButtonsKey[ShifterDirection.Right] = right.Button;
            return this;
        }

        public ShifterFactory<TKey> LeftRightButton(string? leftButtonKey, string? rightButtonKey)
        {
            ButtonCommand? left = GetButton(leftButtonKey);
            ButtonCommand? right = GetButton(rightButtonKey);
            return LeftRightButton(left, right);
        }

        public ShifterSlotFactory<TKey> RootSlot(TKey key, ButtonCommand? slotButton)
        {
            if (IsRootSlotDefined)
            {
                ScriptError error = new(ErrorLevel.Error, "シフターの既定ポジションは既に定義されています。");
                Avatar.ErrorCollector.Report(error);
                return InitialPosition;
            }

            ShifterSlotFactory<TKey> factory = GetOrCreateSlot(key, slotButton);
            InitialPosition = factory;

            IsRootSlotDefined = true;
            return factory;
        }

        public ShifterSlotFactory<TKey> RootSlot(TKey key, string? slotButtonKey) => RootSlot(key, GetButton(slotButtonKey));
        public ShifterSlotFactory<TKey> RootSlot(TKey key) => RootSlot(key, (ButtonCommand?)null);

        internal ShifterSlotFactory<TKey> GetOrCreateSlot(TKey key, ButtonCommand? slotButton)
        {
            if (Slots.TryGetValue(key, out ShifterSlotFactory<TKey>? factory))
            {
                if (slotButton is not null)
                {
                    ScriptError error = new(ErrorLevel.Error, $"シフターポジション '{key}' は既に存在するため、ポジションボタンを新たに指定することはできません。");
                    Avatar.ErrorCollector.Report(error);
                }
            }
            else
            {
                factory = new(Avatar, this, key);
                SlotsKey.Add(factory);

                if (slotButton is not null) SlotButtonsKey[key] = slotButton.Button;
            }

            return factory;
        }

        private ButtonCommand? GetButton(string? buttonKey)
        {
            return buttonKey is null ? null
                : Avatar.Commander.Input.Buttons.GetValue(buttonKey, out ButtonCommand button) ? button
                : null;
        }

        protected override IController OnBuild()
        {
            ShifterSlot<TKey> initialPosition = InitialPosition.Build();
            ShifterLever<TKey> lever = new(initialPosition);
            BuiltController = new Shifter<TKey>(lever, DirectionButtons, SlotButtons);
            return BuiltController;
        }
    }
}
