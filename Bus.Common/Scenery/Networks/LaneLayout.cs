using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class LaneLayout
    {
        public IReadOnlyList<LanePin> Pins { get; }

        public LaneLayout(IReadOnlyList<LanePin> pins)
        {
            Pins = pins;
        }

        public LaneLayout(params LanePin[] pins) : this((IReadOnlyList<LanePin>)pins)
        {
        }

        public bool CanConnectTo(LaneLayout other)
        {
            if (Pins.Count != other.Pins.Count) return false;

            for (int i = 0; i < Pins.Count; i++)
            {
                LanePin from = Pins[i];
                LanePin to = other.Pins[Pins.Count - 1 - i];

                if (!from.IsOpposite(to)) return false;
            }

            return true;
        }

        public LaneLayout CreateCopy()
        {
            LanePin[] newPins = Pins
                .Select(pin => pin.CreateCopy())
                .ToArray();

            return new LaneLayout(newPins);
        }

        public LaneLayout CreateOpposition()
        {
            LanePin[] newPins = Pins
                .Reverse()
                .Select(pin => pin.CreateOpposite())
                .ToArray();

            return new LaneLayout(newPins);
        }
    }
}
