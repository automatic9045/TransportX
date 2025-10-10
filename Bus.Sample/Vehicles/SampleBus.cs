using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vortice.Direct3D11;

using Bus.Common.Rendering;
using Bus.Common.Scenery;
using Bus.Common.Vehicles;

using Bus.Sample.Vehicles.Drives;
using Bus.Sample.Vehicles.Input;
using Bus.Sample.Vehicles.Interfaces;

namespace Bus.Sample.Vehicles
{
    [VehicleIdentifier("Sample")]
    public class SampleBus : VehicleBase
    {
        private readonly LocatedModel Model;

        private readonly IReadOnlyList<IInput> Inputs;
        private readonly InterfaceSet Interfaces;
        private readonly DriveSet Drives;

        public override string Title { get; } = "サンプルバス";
        public override string Description { get; } = "動作確認用のバスです。";
        public override string Author { get; } = "automatic9045";

        public SampleBus(VehicleBuilder builder) : base(builder)
        {
            Locate(0, 0, new SixDoF(10, 10, 20, 0.1f, 0, 0));

            ModelFactory modelFactory = new ModelFactory(DXHost.Device, DXHost.Context);
            Model model = modelFactory.FromFile(@"Bus\Bus.obj");
            Model = new LocatedModel(model, Matrix4x4.CreateScale(0.01f));

            SoundFactory soundFactory = new SoundFactory(DXHost.XAudio2, DXHost.MasteringVoice);
            Inputs = [new KeyboardInput(InputManager)];
            Interfaces = new InterfaceSet(Inputs, Inputs[0]);
            Drives = new DriveSet(Interfaces, soundFactory, this);
        }

        public override void Dispose()
        {
            Model.Model.Dispose();
            Drives.Dispose();
        }

        public override void ComputeTick(TimeSpan elapsed)
        {
            Drives.ComputeTick(elapsed);
        }

        public override void Tick(TimeSpan elapsed)
        {
            foreach (IInput input in Inputs) input.Tick(elapsed);
            Interfaces.Tick(Drives.Chassis.AverageAngularVelocity, elapsed);
            Drives.Tick(elapsed);

            Drives.UpdateSound(DXHost.X3DAudio, Camera.Listener);

            Application.Current.MainWindow.Title =
                $"{Drives.Engine.Rpm:f0}rpm, r={((AT)Drives.Transmission).TorqueConverter.Throttle:f2}, Te={((Engine)Drives.Engine).Torque:f0}, " +
                $"Ttc={((AT)Drives.Transmission).TorqueConverter.Torque:f0}, Ttr={Drives.Transmission.Torque:f0}";

            /*Tire tire = Drives.Chassis.Tires[2];
            Application.Current.MainWindow.Title =
                $"Tt={Drives.Transmission.Torque:F1}; " +
                $"M=a={tire.AngularAcceleration:F1}, v={tire.AngularVelocity:F1}; " +
                $"Fx={tire.LateralForce:F1}, Fy={tire.VerticalLoad:F1}, Fz={tire.LongitudinalForce:F1}; " +
                $"Mx={tire.RollingResistanceMoment:F1}, My={tire.SelfAligningTorque:F1}";*/
        }

        public override void Draw(ID3D11DeviceContext context, ID3D11Buffer constantBuffer, Matrix4x4 view, Matrix4x4 projection)
        {
            Model.Locator = Model.InitialLocator * Locator;
            Model.Draw(context, constantBuffer, view, projection);
        }
    }
}
