using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkEdge : NetworkElement
    {
        public override IReadOnlyList<ElementPath> Paths => [Path];
        public abstract ElementPath Path { get; }

        public NetworkEdge(int plateX, int plateZ, Matrix4x4 locator, bool isRoot) : base(plateX, plateZ, locator, isRoot)
        {
        }

        public virtual void SetChild(NetworkElement child)
        {
            if (Path.Child is not null) throw new InvalidOperationException("子は既に設定済です。");
            SetChild(0, child);
        }
    }
}
