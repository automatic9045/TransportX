using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Input;

namespace Bus.Sample.Vehicles.Interfaces
{
    internal class MTShifter
    {
        private bool LeverMoved = false;

        public IMTShifterInput Source { get; set; }

        public int Gear { get; private set; } = 0;

        public MTShifter(IMTShifterInput defaultSource)
        {
            Source = defaultSource;
        }

        public void Tick(TimeSpan elapsed)
        {
            if (LeverMoved)
            {
                if (Source.Direction.Y == 0) LeverMoved = false;
            }
            else
            {
                if (Source.Direction.Y != 0)
                {
                    Gear = int.Max(-1, int.Min(Gear + float.Sign(Source.Direction.Y), 6));
                    LeverMoved = true;
                }
            }
        }
    }
}
