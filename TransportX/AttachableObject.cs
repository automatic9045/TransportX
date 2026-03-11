using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX
{
    public sealed class AttachableObject : LocatableObject
    {
        public ILocatable Parent { get; }
        public Pose Offset { get; }

        public AttachableObject(ILocatable parent, Pose offset) : base()
        {
            Parent = parent;
            Offset = offset;

            Parent.Moved += _ => Update();
            Update();


            void Update()
            {
                Locate(Parent, Offset * Parent.Pose);
            }
        }

        public AttachableObject(ILocatable parent, SixDoF position) : this(parent, position.ToPose())
        {
        }
    }
}
