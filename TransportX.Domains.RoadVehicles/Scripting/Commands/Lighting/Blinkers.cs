using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

using TransportX.Domains.RoadVehicles.Lighting;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Lighting
{
    public class Blinkers
    {
        private readonly Lighting Parent;

        public BlinkerLampGroup Left { get; }
        public BlinkerLampGroup Right { get; }

        public Blinkers(Lighting parent)
        {
            Parent = parent;

            Left = new BlinkerLampGroup(Parent, Parent.Source.Blinker.Left.Lamps, "左ウィンカーランプ");
            Right = new BlinkerLampGroup(Parent, Parent.Source.Blinker.Right.Lamps, "右ウィンカーランプ");
        }

        public Blinkers Lever(Signal<int> signal)
        {
            Parent.Source.Blinker.LeverSignal = signal;
            return this;
        }

        public Blinkers Lever(string intSignalKey)
            => Lever(Parent.Avatar.Commander.Signals.Int(intSignalKey));

        public Blinkers Hazard(Signal<bool> signal)
        {
            Parent.Source.Blinker.HazardSignal = signal;
            return this;
        }

        public Blinkers Hazard(string boolSignalKey)
            => Hazard(Parent.Avatar.Commander.Signals.Bool(boolSignalKey));

        public Blinkers RelayOnCount(Signal<int> signal)
        {
            Parent.Source.Blinker.RelayOnCountSignal = signal;
            return this;
        }

        public Blinkers RelayOnCount(string intSignalKey)
            => RelayOnCount(Parent.Avatar.Commander.Signals.Int(intSignalKey));

        public Blinkers RelayOffCount(Signal<int> signal)
        {
            Parent.Source.Blinker.RelayOffCountSignal = signal;
            return this;
        }

        public Blinkers RelayOffCount(string intSignalKey)
            => RelayOffCount(Parent.Avatar.Commander.Signals.Int(intSignalKey));


        public Blinkers Period(double seconds)
        {
            Parent.Source.Blinker.Period = TimeSpan.FromSeconds(seconds);
            return this;
        }

        public Blinkers DutyCycle(double ratio)
        {
            Parent.Source.Blinker.DutyCycle = (float)ratio;
            return this;
        }


        public Blinkers AutoCancel(IBlinkerAutoCancel autoCancel)
        {
            Parent.Source.Blinker.AutoCancel = autoCancel;
            return this;
        }

        public Blinkers SteeringThresholdAutoCancel(Signal<float> steeringSignal, double steeringEngageThreshold, double steeringDisengageThreshold)
        {
            SteeringThresholdAutoCancel autoCancel = new(steeringSignal)
            {
                EngageThreshold = (float)steeringEngageThreshold,
                DisengageThreshold = (float)steeringDisengageThreshold,
            };
            return AutoCancel(autoCancel);
        }

        public Blinkers SteeringThresholdAutoCancel(string steeringFloatSignalKey, double steeringEngageThreshold, double steeringDisengageThreshold)
            => SteeringThresholdAutoCancel(Parent.Avatar.Commander.Signals.Float(steeringFloatSignalKey), steeringEngageThreshold, steeringDisengageThreshold);

        public Blinkers CamAutoCancel(Signal<float> steeringSignal, IReadOnlyList<float> camPositions, double margin)
        {
            CamAutoCancel autoCancel = new(steeringSignal, camPositions)
            {
                Margin = (float)margin,
            };
            return AutoCancel(autoCancel);
        }

        public Blinkers CamAutoCancel(string steeringFloatSignalKey, IReadOnlyList<float> camPositions, double margin)
            => CamAutoCancel(Parent.Avatar.Commander.Signals.Float(steeringFloatSignalKey), camPositions, margin);

        public Blinkers CamAutoCancel(Signal<float> steeringSignal, IReadOnlyList<double> camPositions, double margin)
            => CamAutoCancel(steeringSignal, camPositions.Select(x => (float)x).ToArray(), margin);

        public Blinkers CamAutoCancel(string steeringFloatSignalKey, IReadOnlyList<double> camPositions, double margin)
            => CamAutoCancel(Parent.Avatar.Commander.Signals.Float(steeringFloatSignalKey), camPositions, margin);
    }
}
