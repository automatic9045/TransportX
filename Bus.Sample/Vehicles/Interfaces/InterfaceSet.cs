using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Sample.Vehicles.Input;

namespace Bus.Sample.Vehicles.Interfaces
{
    internal class InterfaceSet
    {
        private IInput SourceKey;
        public IInput Source
        {
            get => SourceKey;
            set
            {
                SourceKey = value;

                Clutch.Source = SourceKey.Clutch;
                Brake.Source = SourceKey.Brake;
                Throttle.Source = SourceKey.Throttle;
                Steering.Source = SourceKey.Steering;
            }
        }

        public Pedal Clutch { get; }
        public Pedal Brake { get; }
        public Pedal Throttle { get; }
        public Steering Steering { get; }
        public ATShifter ATShifter { get; }

        public InterfaceSet(IEnumerable<IInput> sources, IInput defaultSource)
        {
            SourceKey = defaultSource;

            Clutch = new Pedal(SourceKey.Clutch);
            Brake = new Pedal(SourceKey.Brake);
            Throttle = new Pedal(SourceKey.Throttle);
            Steering = new Steering(SourceKey.Steering);
            ATShifter = new ATShifter(sources.Select(source => source.ATShifter));
        }

        public void Tick(double vehicleAngularVelocity, TimeSpan elapsed)
        {
            ATShifter.Tick(vehicleAngularVelocity, elapsed);
        }
    }
}
