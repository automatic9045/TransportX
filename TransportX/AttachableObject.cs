using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX
{
    public sealed class AttachableObject : WorldObject
    {
        public IWorldObject Parent { get; }
        public Pose Offset { get; }

        public AttachableObject(IWorldObject parent, Pose offset) : base()
        {
            Parent = parent;
            Offset = offset;

            Parent.Moved += _ => Update();
            Update();


            void Update()
            {
                Locate(Offset * Parent.WorldPose);
            }
        }

        public AttachableObject(IWorldObject parent, SixDoF position) : this(parent, position.ToPose())
        {
        }
    }
}
