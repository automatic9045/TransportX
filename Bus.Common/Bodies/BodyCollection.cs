using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery;

namespace Bus.Common.Bodies
{
    public class BodyCollection : List<RigidBody>, IDisposable
    {
        public BodyCollection() : base()
        {
        }

        public void Dispose()
        {
            foreach (RigidBody body in this) body.Dispose();
        }

        public void SetCameraPosition(ILocatable camera)
        {
            foreach (RigidBody body in this)
            {
                PlateOffset fromCamera = camera.GetPlateOffset(body);
                body.SetFromCamera(fromCamera);
            }
        }

        public void SubTick(TimeSpan elapsed, ILocatable camera)
        {
            foreach (RigidBody body in this)
            {
                body.SubTick(elapsed);
                PlateOffset fromCamera = camera.GetPlateOffset(body);
                body.SetFromCamera(fromCamera);
            }
        }

        public void Tick(TimeSpan elapsed)
        {
            foreach (RigidBody body in this) body.Tick(elapsed);
        }
    }
}
