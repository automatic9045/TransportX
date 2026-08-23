using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Physics;

namespace TransportX.Rendering
{
    public readonly struct ModelResourceSet : IDisposable
    {
        public static ModelResourceSet Empty() => new(Rendering.Model.Empty());


        public required IModel Model { get; init; }
        public ICollider? Collider { get; init; }

        [SetsRequiredMembers]
        public ModelResourceSet(IModel model) : this()
        {
            Model = model;
        }

        public void Dispose()
        {
            Model.Dispose();
            Collider?.Dispose();
        }

        [MemberNotNull(nameof(Collider))]
        public ICollider GetCollider() => Collider ?? throw new NotSupportedException("物理モデルが指定されていません。");
    }
}
