using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Cameras
{
    public class PassengerViewpoint : Viewpoint
    {
        private const float MinFieldOfView = float.Pi * 0.0025f;
        private const float MaxFieldOfView = float.Pi * 0.375f;


        private readonly IWorldObject Source;
        private readonly IReadOnlyList<ViewpointPreset> Presets;

        private new readonly Rotator Rotator = new();

        private int PresetIndex = 0;

        public PassengerViewpoint(IWorldObject source, IReadOnlyList<ViewpointPreset> presets) : base()
        {
            if (presets.Count == 0) throw new ArgumentException($"{nameof(presets)} は 1 つ以上の要素を持っている必要があります。", nameof(presets));

            Source = source;
            Presets = presets;

            SetPreset(0);
            Tick(TimeSpan.Zero);
        }

        public override void Tick(TimeSpan elapsed)
        {
            ViewpointPreset preset = Presets[PresetIndex];

            float pitch = float.Clamp(preset.Offset.Rotation.X + Rotator.Angle.X, -float.Pi * 0.499f, float.Pi * 0.499f);
            float yaw = preset.Offset.Rotation.Y + Rotator.Angle.Y;
            float roll = preset.Offset.Rotation.Z;

            Quaternion orientation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            Pose cameraOffset = new(preset.Offset.Translation, orientation);

            Locate(cameraOffset * Source.WorldPose);
        }

        public override void Rotate(Vector2 offset, SizeI clientSize)
        {
            Rotator.Rotate(1.5f * FieldOfView * offset, clientSize);
        }

        public override void Zoom(float delta)
        {
            FieldOfView = float.Clamp(FieldOfView - float.Pi * 0.0125f * delta, MinFieldOfView, MaxFieldOfView);
        }

        public override void NextPreset() => SetPreset(PresetIndex + 1);
        public override void PreviousPreset() => SetPreset(PresetIndex - 1);
        public override void Reset() => SetPreset(0);

        private void SetPreset(int index)
        {
            if (Presets.Count == 0) throw new InvalidOperationException();

            PresetIndex = (index + Presets.Count) % Presets.Count;
            FieldOfView = Presets[PresetIndex].FieldOfView;
            Rotator.Reset();
        }
    }
}
