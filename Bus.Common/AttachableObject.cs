using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common
{
    public sealed class AttachableObject : LocatableObject
    {
        public LocatableObject Parent { get; }
        public Pose Offset { get; }

        public AttachableObject(LocatableObject parent, Pose offset) : base()
        {
            Parent = parent;
            Offset = offset;

            Parent.Moved += (sender, e) => Update();
            Update();


            void Update()
            {
                Locate(Parent, Parent.Pose * Offset);
            }
        }

        public AttachableObject(LocatableObject parent, SixDoF position) : this(parent, position.ToPose())
        {
        }
    }
}
