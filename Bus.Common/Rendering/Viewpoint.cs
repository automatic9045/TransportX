using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Rendering
{
    public class Viewpoint
    {
        public LocatableObject Source { get; }
        public virtual Matrix4x4 Locator => Source.Locator;
        public float Perspective { get; protected set; } = 1;

        public Viewpoint(LocatableObject source)
        {
            Source = source;
        }

        public virtual void Rotate(Vector2 offset, Vector2 clientSize)
        {
        }

        public virtual void Zoom(float delta)
        {
        }

        public virtual void Reset()
        {
        }


        protected sealed class Translator
        {
            public float InitialDistance { get; }
            public float Distance { get; private set; }
            public Matrix4x4 Translation { get; private set; }
            public float ZoomRatio => InitialDistance / Distance;

            public Translator(float initialDistance)
            {
                Update(initialDistance);
                InitialDistance = Distance;
            }

            public void Zoom(float delta)
            {
                Update(Distance - 0.01f * delta);
            }

            public void Reset()
            {
                Update(InitialDistance);
            }

            private void Update(float distance)
            {
                Distance = float.Max(1f, float.Min(distance, 100));
                Translation = Matrix4x4.CreateTranslation(0, 0, -Distance);
            }
        }

        protected sealed class Rotator
        {
            public Vector2 InitialAngle { get; }
            public Vector2 Angle { get; private set; }
            public Matrix4x4 Rotation { get; private set; }

            public Rotator(Vector2 initialAngle)
            {
                Update(initialAngle);
                InitialAngle = Angle;
            }

            public void Rotate(Vector2 velocity, Vector2 clientSize)
            {
                Vector2 angle = Angle + new Vector2(-velocity.Y / float.Max(1, clientSize.Y), -velocity.X / float.Max(1, clientSize.X));
                Update(angle);
            }

            public void Reset()
            {
                Update(InitialAngle);
            }

            private void Update(Vector2 angle)
            {
                Angle = new Vector2(float.Max(-float.Pi / 2 + 0.001f, float.Min(angle.X, float.Pi / 2 - 0.001f)), angle.Y % float.Tau);
                Rotation = Matrix4x4.CreateRotationX(Angle.X) * Matrix4x4.CreateRotationY(Angle.Y);
            }
        }
    }


    public class FreeViewpoint : Viewpoint
    {
        private new readonly Rotator Rotator = new Rotator(Vector2.Zero);
        public override Matrix4x4 Locator => Rotator.Rotation * Source.Locator;

        public FreeViewpoint(LocatableObject source) : base(source)
        {
        }

        public override void Rotate(Vector2 offset, Vector2 clientSize)
        {
            Rotator.Rotate(1.5f * Perspective * offset, clientSize);
        }

        public override void Zoom(float delta)
        {
            Perspective = float.Max(0.01f, float.Min(Perspective - 0.0005f * delta, 1));
        }

        public override void Reset()
        {
            Perspective = 1;
            Rotator.Reset();
        }
    }


    public class DriverViewpoint : Viewpoint
    {
        private new readonly Rotator Rotator = new Rotator(Vector2.Zero);
        public override Matrix4x4 Locator => Rotator.Rotation * Source.Locator;

        public DriverViewpoint(LocatableObject source) : base(source)
        {
        }

        public override void Rotate(Vector2 offset, Vector2 clientSize)
        {
            Rotator.Rotate(1.5f * Perspective * offset, clientSize);
        }

        public override void Zoom(float delta)
        {
            Perspective = float.Max(0.01f, float.Min(Perspective - 0.0005f * delta, 1));
        }

        public override void Reset()
        {
            Perspective = 1;
            Rotator.Reset();
        }
    }


    public class BirdViewpoint : Viewpoint
    {
        private new readonly Translator Translator;
        private new readonly Rotator Rotator;
        public override Matrix4x4 Locator => Translator.Translation * Rotator.Rotation * Source.Locator;

        public BirdViewpoint(LocatableObject source, float initialDistance, Vector2 initialAngle) : base(source)
        {
            Translator = new Translator(initialDistance);
            Rotator = new Rotator(initialAngle);
        }

        public override void Rotate(Vector2 offset, Vector2 clientSize)
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
