using Bus.Common.Scenery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void SetCameraPosition(LocatableObject camera)
        {
            foreach (RigidBody body in this)
            {
                PlateOffset fromCamera = camera.GetPlateOffset(body);
                body.SetFromCamera(fromCamera);
            }
        }
    }
}
