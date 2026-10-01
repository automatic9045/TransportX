using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Cameras
{
    public class DriverViewpoint : Viewpoint
    {
        private const float MinFieldOfView = float.Pi * 0.0025f;
        private const float MaxFieldOfView = float.Pi * 0.375f;
        private const float TransitionDuration = 1.25f;


        private readonly IWorldObject Source;
        private readonly IReadOnlyList<ViewpointPreset> Presets;

        private new readonly Rotator Rotator = new();

        private int PresetIndex = 0;
        private ViewpointPreset FromPreset;
        private ViewpointPreset CurrentPreset;
        private float TransitionTime = TransitionDuration;

        public DriverViewpoint(IWorldObject source, IReadOnlyList<ViewpointPreset> presets) : base()
        {
            if (presets.Count == 0) throw new ArgumentException($"{nameof(presets)} は 1 つ以上の要素を持っている必要があります。", nameof(presets));

            Source = source;
            Presets = presets;

            SetPreset(0, true);
            Tick(TimeSpan.Zero);
        }

        public override void Tick(TimeSpan elapsed)
        {
            if (TransitionTime < TransitionDuration)
            {
                TransitionTime += (float)elapsed.TotalSeconds;
                float t = 0 < TransitionDuration ? float.Clamp(TransitionTime / TransitionDuration, 0, 1) : 1;
                float easedT = t == 1 ? 1 : 1 - float.Pow(2, -10 * t);

                ViewpointPreset targetPreset = Presets[PresetIndex];
                CurrentPreset = ViewpointPreset.Lerp(FromPreset, targetPreset, easedT);
                FieldOfView = CurrentPreset.FieldOfView;

                if (1 <= t)
                {
                    CurrentPreset = targetPreset;
                    FieldOfView = targetPreset.FieldOfView;
                }
            }

            Locate(Rotator.RotationPose * CurrentPreset.Offset * Source.WorldPose);
        }

        public override void Rotate(Vector2 offset, SizeI clientSize)
        {
            Rotator.Rotate(1.5f * FieldOfView * offset, clientSize);
        }

        public override void Zoom(float delta)
        {
            FieldOfView = float.Clamp(FieldOfView - float.Pi * 0.0125f * delta, MinFieldOfView, MaxFieldOfView);
            CurrentPreset = new ViewpointPreset(CurrentPreset.Offset, FieldOfView);
        }

        public override void NextPreset() => SetPreset(PresetIndex + 1);
        public override void PreviousPreset() => SetPreset(PresetIndex - 1);
        public override void Reset() => SetPreset(0);

        public void SetPreset(int index, bool immediate = false)
        {
            if (Presets.Count == 0) throw new InvalidOperationException();

            PresetIndex = (index + Presets.Count) % Presets.Count;
            ViewpointPreset targetPreset = Presets[PresetIndex];

            if (immediate)
            {
                TransitionTime = TransitionDuration;
                CurrentPreset = targetPreset;
                FromPreset = targetPreset;
                FieldOfView = targetPreset.FieldOfView;
                Rotator.Reset();
                return;
            }

            FromPreset = CurrentPreset;
            TransitionTime = 0;
            Rotator.Reset();
        }
    }
}
