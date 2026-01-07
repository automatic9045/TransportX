using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Physics;

namespace Bus.Common.Rendering
{
    public interface IModel : IDisposable
    {
        public string? DebugName { get; set; }

        void Draw(DrawContext context);
    }


    public interface ICollidableModel : IModel
    {
        ICollider Collider { get; }
    }
}
