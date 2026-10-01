using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Cameras
{
    public class BirdViewpoint : Viewpoint
    {
        private readonly IWorldObject Source;
        private readonly Pose Offset;

        private new readonly Translator Translator;
        private new readonly Rotator Rotator;

        public BirdViewpoint(IWorldObject source, Pose offset, float initialDistance, Vector2 initialAngle, float fieldOfView = float.Pi / 4)
            : base()
        {
            Source = source;
            Offset = offset;
            FieldOfView = fieldOfView;

            Translator = new Translator(initialDistance);

            Rotator = new Rotator()
            {
                InitialAngle = initialAngle
            };
            Rotator.Reset();

            Tick(TimeSpan.Zero);
        }

        public BirdViewpoint(IWorldObject source, SixDoF offset, float initialDistance, Vector2 initialAngle, float fieldOfView = float.Pi / 4)
            : this(source, offset.ToPose(), initialDistance, initialAngle, fieldOfView)
        {
        }

        public override void Tick(TimeSpan elapsed)
        {
            Locate(Translator.TranslationPose * Rotator.RotationPose * Offset * Source.WorldPose);
        }

        public override void Rotate(Vector2 offset, SizeI clientSize)
        {
            Rotator.Rotate(-1.5f * Translator.ZoomRatio * offset, clientSize);
        }

        public override void Zoom(float delta)
        {
            Translator.Zoom(delta);
        }

        public override void Reset()
        {
            Rotator.Reset();
            Translator.Reset();
        }
    }
}
