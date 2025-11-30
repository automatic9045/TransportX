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
        public virtual Matrix4x4 Transform => Source.Transform;
        public float Perspective { get; protected set; } = 1;

        public Viewpoint(LocatableObject source)
        {
            Source = source;
        }

        public virtual void Move(Vector2 offset, Vector2 clientSize)
        {
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
        private new readonly SourceObject Source;
        public override Matrix4x4 Transform => Source.Transform;

        public FreeViewpoint() : base(new SourceObject())
        {
            Source = (SourceObject)base.Source;
        }

        public override void Move(Vector2 offset, Vector2 clientSize)
        {
            Source.Move(0.1f * new Vector2(-offset.X, offset.Y));
        }

        public override void Rotate(Vector2 offset, Vector2 clientSize)
        {
            Source.Rotate(1.5f * Perspective * offset, clientSize);
        }

        public override void Zoom(float delta)
        {
            Source.Zoom(0.02f * delta);
        }

        public override void Reset()
        {
            Source.Reset();
        }


        private class SourceObject : LocatableObject
        {
            private readonly Rotator Rotator = new Rotator(Vector2.Zero);

            public SourceObject() : base(0, 0, new SixDoF(0, 10, 0))
            {
            }

            public void Move(Vector2 amount)
            {
                Vector3 right = Vector3.Transform(Vector3.UnitX, Orientation);
                Vector3 forward = Vector3.Transform(Vector3.UnitZ, Orientation);
                right.Y = forward.Y = 0;

                right = Vector3.Normalize(right);
                forward = Vector3.Normalize(forward);

                Vector3 r = right * amount.X + forward * amount.Y;
                Locate(PlateX, PlateZ, Transform * Matrix4x4.CreateTranslation(r));
            }

            public void Rotate(Vector2 offset, Vector2 clientSize)
            {
                Rotator.Rotate(offset, clientSize);
                Locate(PlateX, PlateZ, Rotator.Rotation * Matrix4x4.CreateTranslation(Position));
            }

            public void Zoom(float amount)
            {
                Move(Matrix4x4.CreateTranslation(0, 0, amount));
            }

            public void Reset()
            {
                Rotator.Reset();
                Locate(PlateX, PlateZ, Rotator.Rotation * Matrix4x4.CreateTranslation(Position.X, 10, Position.Z));
            }
        }
    }


    public class DriverViewpoint : Viewpoint
    {
        private new readonly Rotator Rotator = new Rotator(Vector2.Zero);
        public override Matrix4x4 Transform => Rotator.Rotation * Source.Transform;

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
        public override Matrix4x4 Transform => Translator.Translation * Rotator.Rotation * Source.Transform;

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
