using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Network
{
    public class LanePin
    {
        public NetworkPort Port { get; }
        public int Index { get; }
        public Pose LocalPose { get; }

        public Lane Definition => Port.Layout.Lanes[Index];
        public LanePin? ConnectedPin => Port.ConnectedPort is null ? null : Port.ConnectedPort.Pins[Port.Pins.Count - 1 - Index];

        private readonly List<ILanePath> SourcePathsKey = new List<ILanePath>();
        public IReadOnlyList<ILanePath> SourcePaths => SourcePathsKey;

        private readonly List<ILanePath> DestPathsKey = new List<ILanePath>();
        public IReadOnlyList<ILanePath> DestPaths => DestPathsKey;

        public LanePin(NetworkPort port, int index)
        {
            Port = port;
            Index = index;
            LocalPose = new Pose(new Vector3(Definition.Position, 0)) * Port.Offset;
        }

        public void Wire(ILanePath path)
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
