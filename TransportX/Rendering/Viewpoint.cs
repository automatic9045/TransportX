using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Rendering
{
    public abstract class Viewpoint : LocatableObject
    {
        public float Perspective { get; protected set; } = 1;

        protected Viewpoint()
        {
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
            public Vector3 Translation { get; private set; }
            public Pose TranslationPose => new(Translation);
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
                Translation = new Vector3(0, 0, -Distance);
            }
        }

        protected sealed class Rotator
        {
            public Vector2 InitialAngle { get; }
            public Vector2 Angle { get; private set; }
            public Quaternion Rotation { get; private set; }
            public Pose RotationPose => new(Vector3.Zero, Rotation);

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
                Rotation = Quaternion.CreateFromYawPitchRoll(Angle.Y, Angle.X, 0);
            }
        }
    }


    public class FreeViewpoint : Viewpoint
    {
        private new readonly Rotator Rotator;

        public Vector2 Angle => Rotator.Angle;

        public FreeViewpoint(int plateX, int plateZ, Vector3 position, Vector2 angle) : base()
        {
            Rotator = new Rotator(angle);
            Locate(plateX, plateZ, new Pose(position, Rotator.Rotation));
        }

        public override void Move(Vector2 offset, Vector2 clientSize)
        {
            Vector2 amount = 0.1f * new Vector2(-offset.X, offset.Y);

            Vector3 right = Vector3.Transform(Vector3.UnitX, Pose.Orientation);
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, Pose.Orientation);
            right.Y = forward.Y = 0;

            right = Vector3.Normalize(right);
            forward = Vector3.Normalize(forward);

            Vector3 r = right * amount.X + forward * amount.Y;
            Locate(PlateX, PlateZ, Pose * new Pose(r));
        }

        public override void Rotate(Vector2 offset, Vector2 clientSize)
        {
            Rotator.Rotate(1.5f * Perspective * offset, clientSize);
            Locate(PlateX, PlateZ, new Pose(Pose.Position, Rotator.Rotation));
        }

        public override void Zoom(float delta)
        {
            Move(new Pose(0, 0, 0.02f * delta));
        }

        public override void Reset()
        {
            Rotator.Reset();

            Vector3 position = Pose.Position;
            position.Y = 10;
            Locate(PlateX, PlateZ, new Pose(position, Rotator.Rotation));
        }
    }


    public class DriverViewpoint : Viewpoint
    {
        private readonly ILocatable Source;
        private readonly Pose Offset;

        private new readonly Rotator Rotator = new(Vector2.Zero);

        public DriverViewpoint(ILocatable source, Pose offset) : base()
        {
            Source = source;
            Offset = offset;

            Source.Moved += _ => UpdateLocation();
            UpdateLocation();


            void UpdateLocation()
            {
                Locate(Source, Rotator.RotationPose * Offset * Source.Pose);
            }
        }

        public DriverViewpoint(ILocatable source, SixDoF offset) : this(source, offset.ToPose())
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
        private readonly ILocatable Source;
        private readonly Pose Offset;

        private new readonly Translator Translator;
        private new readonly Rotator Rotator;

        public BirdViewpoint(ILocatable source, Pose offset, float initialDistance, Vector2 initialAngle) : base()
        {
            Source = source;
            Offset = offset;

            Translator = new Translator(initialDistance);
            Rotator = new Rotator(initialAngle);

            Source.Moved += _ => UpdateLocation();
            UpdateLocation();


            void UpdateLocation()
            {
                Locate(Source, Translator.TranslationPose * Rotator.RotationPose * Offset * Source.Pose);
            }
        }

        public BirdViewpoint(ILocatable source, SixDoF offset, float initialDistance, Vector2 initialAngle)
            : this(source, offset.ToPose(), initialDistance, initialAngle)
        {
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
