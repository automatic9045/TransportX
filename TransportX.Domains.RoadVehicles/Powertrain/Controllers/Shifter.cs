using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Input;

using TransportX.Domains.RoadVehicles.Physics;

namespace TransportX.Domains.RoadVehicles.Powertrain.Controllers
{
    public class Shifter<TKey> : IController, IDisposable where TKey : notnull
    {
        private readonly IReadOnlyDictionary<ShifterDirection, ButtonEventHandler> OnDirectionButtonPressed;
        private readonly IReadOnlyDictionary<TKey, ButtonEventHandler> OnSlotButtonPressed;

        public ShifterLever<TKey> Lever { get; }
        public IReadOnlyDictionary<ShifterDirection, IButton> DirectionButtons { get; }
        public IReadOnlyDictionary<TKey, IButton> SlotButtons { get; }

        public Shifter(ShifterLever<TKey> lever,
            IReadOnlyDictionary<ShifterDirection, IButton> directionButtons, IReadOnlyDictionary<TKey, IButton> slotButtons)
        {
            Lever = lever;
            DirectionButtons = directionButtons;
            SlotButtons = slotButtons;

            Dictionary<ShifterDirection, ButtonEventHandler> onDirectionButtonPressed = [];
            foreach ((ShifterDirection direction, IButton button) in DirectionButtons)
            {
                button.Pressed += OnPressed;
                onDirectionButtonPressed[direction] = OnPressed;

                void OnPressed(IButton sender)
                {
                    Lever.Move(direction);
                }
            }
            OnDirectionButtonPressed = onDirectionButtonPressed;

            Dictionary<TKey, ButtonEventHandler> onSlotButtonPressed = [];
            foreach ((TKey slotKey, IButton button) in SlotButtons)
            {
                button.Pressed += OnPressed;
                onSlotButtonPressed[slotKey] = OnPressed;

                void OnPressed(IButton sender)
                {
                    Lever.MoveTo(slotKey);
                }
            }
            OnSlotButtonPressed = onSlotButtonPressed;
        }

        public void Dispose()
        {
            foreach ((ShifterDirection direction, ButtonEventHandler handler) in OnDirectionButtonPressed)
            {
                DirectionButtons[direction].Pressed -= handler;
            }

            foreach ((TKey slotKey, ButtonEventHandler handler) in OnSlotButtonPressed)
            {
                SlotButtons[slotKey].Pressed -= handler;
            }
        }

        public void Tick(TimeSpan elapsed)
        {
        }
    }
}
