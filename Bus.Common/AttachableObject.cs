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
        public Matrix4x4 Offset { get; set; }

        public AttachableObject(LocatableObject parent, Matrix4x4 offset) : base()
        {
            Parent = parent;
            Offset = offset;

            Parent.Moved += (sender, e) => Update();
            Update();


            void Update()
            {
                Locate(Parent, Offset * Parent.Transform);
            }
        }

        public AttachableObject(LocatableObject parent, SixDoF position) : this(parent, position.CreateTransform())
        {
        }
    }
}
