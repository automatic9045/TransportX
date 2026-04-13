using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Sample.LV290.Vehicles.Input;

namespace TransportX.Sample.LV290.Vehicles.Interfaces
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

        public DoorSwitch DoorSwitch { get; }

        public InterfaceSet(IEnumerable<IInput> sources, IInput defaultSource)
        {
            SourceKey = defaultSource;

            Clutch = new Pedal(Source.Clutch);
            Brake = new Pedal(Source.Brake);
            Throttle = new Pedal(Source.Throttle);
            SteeringWheel = new SteeringWheel(Source.Steering);
            //ATShifter = new ATShifter(sources.Select(source => source.ATShifter));
            AMTShifter = new AMTShifter(Source.MTShifter);
            MTShifter = new MTShifter(Source.MTShifter);

            DoorSwitch = new DoorSwitch(Source.DoorSwitch);
        }

        public void Tick(double vehicleAngularVelocity, TimeSpan elapsed)
        {
            //ATShifter.Tick(vehicleAngularVelocity, elapsed);
            AMTShifter.Tick(elapsed);
            MTShifter.Tick(elapsed);
        }
    }
}
