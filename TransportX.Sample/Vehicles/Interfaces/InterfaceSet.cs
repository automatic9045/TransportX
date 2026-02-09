using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.Vehicles.Input;

namespace TransportX.Sample.Vehicles.Interfaces
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
                SteeringWheel.Source = SourceKey.Steering;
                MTShifter.Source = SourceKey.MTShifter;
            }
        }

        public Pedal Clutch { get; }
        public Pedal Brake { get; }
        public Pedal Throttle { get; }
        public SteeringWheel SteeringWheel { get; }
        //public ATShifter ATShifter { get; }
        public AMTShifter AMTShifter { get; }
        public MTShifter MTShifter { get; }

        public InterfaceSet(IEnumerable<IInput> sources, IInput defaultSource)
        {
            SourceKey = defaultSource;

            Clutch = new Pedal(SourceKey.Clutch);
            Brake = new Pedal(SourceKey.Brake);
            Throttle = new Pedal(SourceKey.Throttle);
            SteeringWheel = new SteeringWheel(SourceKey.Steering);
            //ATShifter = new ATShifter(sources.Select(source => source.ATShifter));
            AMTShifter = new AMTShifter(SourceKey.MTShifter);
            MTShifter = new MTShifter(SourceKey.MTShifter);
        }

        public void Tick(double vehicleAngularVelocity, TimeSpan elapsed)
        {
            //ATShifter.Tick(vehicleAngularVelocity, elapsed);
            AMTShifter.Tick(elapsed);
            MTShifter.Tick(elapsed);
        }
    }
}
