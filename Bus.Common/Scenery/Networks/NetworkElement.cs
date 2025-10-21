using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Rendering;

namespace Bus.Common.Scenery.Networks
{
    public abstract class NetworkElement : LocatableObject, IDrawable
    {
        public bool IsRoot { get; }
        public abstract LaneConnector Port { get; }
        public abstract IReadOnlyList<ElementPath> Paths { get; }

        public virtual IReadOnlyList<LocatedModel> Models { get; } = [];

        public NetworkElement(int plateX, int plateZ, Matrix4x4 transform, bool isRoot) : base(plateX, plateZ, transform)
        {
            IsRoot = isRoot;
        }

        protected void SetChild(int pathIndex, NetworkElement element)
        {
            Paths[pathIndex].SetChild(element);
        }

        public void Draw(DrawContext context)
        {
            foreach (LocatedModel model in Models)
            {
                model.Draw(context);
            }
        }


        public class ElementPath
        {
            public Matrix4x4 Transition { get; }
            public LaneConnector Port { get; }

            public NetworkElement? Child { get; private set; } = null;

            public ElementPath(Matrix4x4 transition, LaneConnector port)
            {
                Transition = transition;
                Port = port;
            }

            protected internal void SetChild(NetworkElement child)
            {
                if (child.IsRoot) throw new ArgumentException($"{nameof(IsRoot)} が true の {nameof(NetworkElement)} を子に設定することはできません。", nameof(child));
                if (!Port.CanConnectTo(child.Port)) throw new ArgumentException("進路の接続部形状が一致しません。", nameof(child));

                Child = child;
            }
        }
    }
}
