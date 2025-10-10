using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Drives;
using Bus.Sample.Vehicles.Input;

namespace Bus.Sample.Vehicles.Interfaces
{
    internal class ATShifter
    {
        private TimeSpan SinceRequested = TimeSpan.Zero;

        public int RequestedPosition { get; private set; } = AT.MaxGear;
        public int Position { get; private set; } = AT.MaxGear;

        public ATShifter(IEnumerable<IATShifterInput> sources)
        {
            foreach (IATShifterInput source in sources)
            {
                source.RPressed += (sender, e) =>
                {
                    RequestedPosition = -1;
                    SinceRequested = TimeSpan.Zero;
                };

                source.NPressed += (sender, e) =>
                {
                    RequestedPosition = 0;
                    SinceRequested = TimeSpan.Zero;
                };

                source.DPressed += (sender, e) =>
                {
                    RequestedPosition = AT.MaxGear;
                    SinceRequested = TimeSpan.Zero;
                };
            }
        }

        public void Tick(double vehicleAngularVelocity, TimeSpan elapsed)
        {
            SinceRequested += elapsed;

            if (RequestedPosition != Position && 1 <= SinceRequested.TotalSeconds)
            {
                if (0 <= RequestedPosition * vehicleAngularVelocity || double.Abs(vehicleAngularVelocity) < 0.001)
                    Position = RequestedPosition;
            }
        }
    }
}
