using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Sample.Vehicles.Drives
{
    internal class TorqueConverter
    {
        private static readonly double LockupThreshold = 0.9;


        private readonly Diagram TRDiagram;
        private readonly Diagram KDiagram;

        private readonly Engine Engine;

        public double Throttle = 0;

        public double Torque { get; private set; } = 0;
        public bool IsLockedup { get; private set; } = false;

        public TorqueConverter(Engine engine)
        {
            IEnumerable<DiagramPoint> trPoints = [new DiagramPoint(0, 3), new DiagramPoint(0.9, 1)];
            TRDiagram = new Diagram(trPoints);

            IEnumerable<DiagramPoint> kPoints = [new DiagramPoint(0, 0.029), new DiagramPoint(0.6, 0.027), new DiagramPoint(0.9, 0.06)];
            KDiagram = new Diagram(kPoints);

            Engine = engine;
        }

        public void ComputeTick(double transmissionInputAngularVelocity, double throttle, TimeSpan elapsed)
        {
            Throttle = throttle;
            Engine.ComputeTick(throttle, elapsed);

            if (Engine.AngularVelocity < 0.001)
            {
                IsLockedup = false;
                Torque = 0;
                return;
            }

            double speedRatio = transmissionInputAngularVelocity / Engine.AngularVelocity;

            if (LockupThreshold < speedRatio)
            {
                IsLockedup = true;
                Torque = Engine.Torque;
                return;
            }

            IsLockedup = false;

            double torqueRatio = TRDiagram.GetValue(speedRatio);
            double kRatio = KDiagram.GetValue(speedRatio);

            double inputTorque = kRatio * Engine.AngularVelocity * Engine.AngularVelocity;
            Torque = inputTorque * torqueRatio;

            Engine.Accelerate(Engine.Torque - inputTorque, elapsed);
        }
    }
}
