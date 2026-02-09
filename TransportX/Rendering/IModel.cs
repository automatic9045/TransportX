using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Physics;

namespace TransportX.Rendering
{
    public interface IModel : IDisposable
    {
        public string? DebugName { get; set; }

        void Draw(DrawContext context);
    }


    public interface ICollidableModel : IModel
    {
        ICollider Collider { get; }
        IDebugModel? ColliderDebugModel { get; }

        void CreateColliderDebugModel(ID3D11Device device);
    }
}
