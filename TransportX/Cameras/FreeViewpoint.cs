using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Mathematics;

namespace TransportX.Cameras
{
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
            Rotator.Rotate(1.5f * FieldOfView * offset, clientSize);
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
}
