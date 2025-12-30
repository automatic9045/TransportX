using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bus.Common.Scenery.Networks
{
    public class LanePin
    {
        public NetworkPort Port { get; }
        public int Index { get; }
        public Matrix4x4 LocalTransform { get; }

        public Lane Definition => Port.Layout.Lanes[Index];
        public LanePin? ConnectedPin => Port.ConnectedPort is null ? null : Port.ConnectedPort.Pins[Port.Pins.Count - 1 - Index];

        private readonly List<LanePath> SourcePathsKey = new List<LanePath>();
        public IReadOnlyList<LanePath> SourcePaths => SourcePathsKey;

        private readonly List<LanePath> DestPathsKey = new List<LanePath>();
        public IReadOnlyList<LanePath> DestPaths => DestPathsKey;

        public LanePin(NetworkPort port, int index)
        {
            Port = port;
            Index = index;
            LocalTransform = Matrix4x4.CreateTranslation(new Vector3(Definition.Position, 0)) * Port.Offset;
        }

        public void Wire(LanePath path)
        {
            bool isWired = false;

            if (path.From == this)
            {
                isWired = true;
                SourcePathsKey.Add(path);
            }

            if (path.To == this)
            {
                isWired = true;
                DestPathsKey.Add(path);
            }

            if (!isWired) throw new ArgumentException("開始点、終了点のどちらもこの端子ではありません。");
        }
    }
}
