using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

using Bus.Common.Rendering;
using Bus.Common.Scenery;
using Bus.Common.Vehicles;
using Bus.Sample.Vehicles.Drives;
using Bus.Sample.Vehicles.Input;

namespace Bus.Sample.Vehicles
{/*
    [VehicleIdentifier("Legacy")]
    public class LegacySampleBus : VehicleBase
    {
        private static readonly TimeSpan LimitComputingTime = TimeSpan.FromSeconds(1d / 60);


        private readonly LocatedModel Model;
        private readonly Matrix4x4 ToRotationOrigin;
        private readonly Matrix4x4 FromRotationOrigin;

        private readonly KeyboardInput Interfaces;
        private readonly DriveSet Drives;

        private double Speed = 0;
        private double Curvature = 0;

        public override string Title { get; } = "サンプルバス (レガシー)";
        public override string Description { get; } = "動作確認用のバスです。";
        public override string Author { get; } = "automatic9045";

        public LegacySampleBus(VehicleBuilder builder) : base(builder)
        {
            Locate(0, 0, new SixDoF(10, 0, 20));

            ModelFactory modelFactory = new ModelFactory(DXHost.Device, DXHost.Context);
            Model model = modelFactory.FromFile(@"Bus\Bus.obj");
            Model = new LocatedModel(model, Matrix4x4.CreateScale(0.01f));
            ToRotationOrigin = Matrix4x4.CreateTranslation(0, 0, 7.226f);
            Matrix4x4.Invert(ToRotationOrigin, out Matrix4x4 fromRotationOrigin);
            FromRotationOrigin = fromRotationOrigin;

            SoundFactory soundFactory = new SoundFactory(DXHost.XAudio2, DXHost.MasteringVoice);
            Interfaces = new KeyboardInputSet(InputManager, () => Speed);
            Drives = new DriveSet(Interfaces, soundFactory, this);
        }

        public override void Dispose()
        {
            Model.Model.Dispose();
            Drives.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            int computeTickCount = (int)double.Ceiling(elapsed / LimitComputingTime);
            TimeSpan computeElapsed = elapsed / computeTickCount;
            for (int i = 0; i < computeTickCount; i++)
            {
                Drives.ComputeTick(Speed / Spec.WheelRadius, computeElapsed);

                double a = Drives.Transmission.Acceleration;
                double r = 1.2 * Interfaces.Brake.Rate + 0.2;
                double oldSpeed = Speed;
                Speed += (a - double.Sign(Speed) * r) * computeElapsed.TotalSeconds;
                if (Speed * oldSpeed < 0) Speed = 0;
            }

            Interfaces.Tick(elapsed);
            Drives.Tick(elapsed);

            Drives.UpdateSound(DXHost.X3DAudio, Camera.Listener);

            Curvature = Interfaces.Steering.Rate / Spec.MinRadius;
            double rotation = Curvature * Speed * elapsed.TotalSeconds;

            Matrix4x4 rotationTransform = ToRotationOrigin * Matrix4x4.CreateRotationY((float)rotation) * FromRotationOrigin;
            Matrix4x4 translation = Matrix4x4.CreateTranslation(0, 0, (float)(Speed * elapsed.TotalSeconds));
            Move(rotationTransform * translation);
        }

        public override void Draw(ID3D11DeviceContext context, ID3D11Buffer constantBuffer, Matrix4x4 view, Matrix4x4 projection)
        {
            Model.Locator = Model.InitialLocator * Locator;
            Model.Draw(context, constantBuffer, view, projection);
        }
    }*/
}
