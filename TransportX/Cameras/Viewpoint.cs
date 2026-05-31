using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Cameras
{
    public abstract class Viewpoint : WorldObject
    {
        public float Perspective { get; protected set; } = 1;

        protected Viewpoint()
        {
        }

        public virtual void Move(Vector2 offset, SizeI clientSize)
        {
        }

        public virtual void Rotate(Vector2 offset, SizeI clientSize)
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
                Update(Distance - 1f * delta);
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
            public Vector2 InitialAngle { get; set; } = Vector2.Zero;
            public Vector2 Angle { get; private set; }
            public Quaternion Rotation { get; private set; }
            public Pose RotationPose => new(Vector3.Zero, Rotation);

            public Rotator()
            {
                Update(InitialAngle);
            }

            public void Rotate(Vector2 velocity, SizeI clientSize)
            {
                Vector2 angle = Angle + new Vector2(-velocity.Y / int.Max(1, clientSize.Height), -velocity.X / int.Max(1, clientSize.Width));
                Update(angle);
            }

            public void Reset()
            {
                Update(InitialAngle);
            }

            public void Update(Vector2 angle)
            {
                Angle = new Vector2(float.Clamp(angle.X, -float.Pi / 2 + 0.001f, float.Pi / 2 - 0.001f), angle.Y % float.Tau);
                Rotation = Quaternion.CreateFromYawPitchRoll(Angle.Y, Angle.X, 0);
            }
        }
    }


    public class FreeViewpoint : Viewpoint
    {
        private new readonly Rotator Rotator;

        public Vector2 Angle => Rotator.Angle;

        public FreeViewpoint() : base()
        {
            Rotator = new Rotator();
        }

        public void Locate(CameraPose cameraPose)
        {
            Rotator.Update(cameraPose.Angle);
            Spatial.WorldPose worldPose = new(cameraPose.Chunk, new Pose(cameraPose.Position, Rotator.Rotation));
            Locate(worldPose);
        }

        public override void Move(Vector2 offset, SizeI clientSize)
        {
            Vector2 amount = 0.1f * new Vector2(-offset.X, offset.Y);

            Vector3 right = Vector3.Transform(Vector3.UnitX, WorldPose.Pose.Orientation);
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, WorldPose.Pose.Orientation);
            right.Y = forward.Y = 0;

            right = Vector3.Normalize(right);
            forward = Vector3.Normalize(forward);

            Vector3 r = right * amount.X + forward * amount.Y;
            Spatial.WorldPose worldPose = WorldPose * new Pose(r);
            Locate(worldPose);
        }

        public override void Rotate(Vector2 offset, SizeI clientSize)
        {
            Rotator.Rotate(1.5f * Perspective * offset, clientSize);
            Spatial.WorldPose worldPose = WorldPose.ChangePose(new Pose(WorldPose.Pose.Position, Rotator.Rotation));
            Locate(worldPose);
        }

        public override void Zoom(float delta)
        {
            Move(new Pose(0, 0, 2f * delta));
        }

        public override void Reset()
        {
            Rotator.Reset();

            Vector3 position = WorldPose.Pose.Position with
            {
                Y = 10,
            };
            Spatial.WorldPose worldPose = WorldPose.ChangePose(new Pose(position, Rotator.Rotation));
            Locate(worldPose);
        }
    }


    public class DriverViewpoint : Viewpoint
    {
        private readonly IWorldObject Source;
        private readonly Pose Offset;

        private new readonly Rotator Rotator = new();

        public DriverViewpoint(IWorldObject source, Pose offset) : base()
        {
            Source = source;
            Offset = offset;

            Source.Moved += _ => UpdateLocation();
            UpdateLocation();


            void UpdateLocation()
            {
                Locate(Rotator.RotationPose * Offset * Source.WorldPose);
            }
        }

        public DriverViewpoint(IWorldObject source, SixDoF offset) : this(source, offset.ToPose())
        {
        }

        public override void Rotate(Vector2 offset, SizeI clientSize)
        {
            Rotator.Rotate(1.5f * Perspective * offset, clientSize);
        }

        public override void Zoom(float delta)
        {
            Perspective = float.Clamp(Perspective - 0.05f * delta, 0.01f, 1.25f);
        }

        public override void Reset()
        {
            Perspective = 1;
            Rotator.Reset();
        }
    }


    public class BirdViewpoint : Viewpoint
    {
        private readonly IWorldObject Source;
        private readonly Pose Offset;

        private new readonly Translator Translator;
        private new readonly Rotator Rotator;

        public BirdViewpoint(IWorldObject source, Pose offset, float initialDistance, Vector2 initialAngle) : base()
        {
            Source = source;
            Offset = offset;

            Translator = new Translator(initialDistance);
            Rotator = new Rotator();

            Rotator.InitialAngle = initialAngle;
            Rotator.Reset();

            Source.Moved += _ => UpdateLocation();
            UpdateLocation();


            void UpdateLocation()
            {
                Locate(Translator.TranslationPose * Rotator.RotationPose * Offset * Source.WorldPose);
            }
        }

        public BirdViewpoint(IWorldObject source, SixDoF offset, float initialDistance, Vector2 initialAngle)
            : this(source, offset.ToPose(), initialDistance, initialAngle)
        {
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
