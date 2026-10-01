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
        public float FieldOfView { get; protected set; } = 1;

        protected Viewpoint()
        {
        }

        public virtual void Tick(TimeSpan elapsed) { }
        public virtual void Move(Vector2 offset, SizeI clientSize) { }
        public virtual void Rotate(Vector2 offset, SizeI clientSize) { }
        public virtual void Zoom(float delta) { }
        public virtual void NextPreset() { }
        public virtual void PreviousPreset() { }
        public virtual void Reset() { }


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
}
