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
        public Matrix4x4 Transform { get; set; }

        public AttachableObject(LocatableObject parent, Matrix4x4 transform) : base()
        {
            Parent = parent;
            Transform = transform;

            Parent.Moved += (sender, e) => Update();
            Update();


            void Update()
            {
                Locate(Parent, Transform * Parent.Locator);
            }
        }

        public AttachableObject(LocatableObject parent, SixDoF position) : this(parent, position.CreateTransform())
        {
        }
    }
}
