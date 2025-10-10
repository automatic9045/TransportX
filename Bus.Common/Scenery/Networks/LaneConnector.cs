using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class LaneConnector
    {
        public IReadOnlyList<LanePin> Pins { get; }

        public LaneConnector(IReadOnlyList<LanePin> pins)
        {
            Pins = pins;
        }

        public LaneConnector(params LanePin[] pins) : this((IReadOnlyList<LanePin>)pins)
        {
        }

        public bool CanConnectTo(LaneConnector other)
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

        public LaneConnector CreateCopy()
        {
            LanePin[] newPins = Pins
                .Select(pin => pin.CreateCopy())
                .ToArray();

            return new LaneConnector(newPins);
        }

        public LaneConnector CreateOpposition()
        {
            LanePin[] newPins = Pins
                .Reverse()
                .Select(pin => pin.CreateOpposite())
                .ToArray();

            return new LaneConnector(newPins);
        }
    }
}
