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
        public Lane Definition { get; }
        public Matrix4x4 LocalTransform { get; }

        private readonly List<LanePath> SourcePathsKey = new List<LanePath>();
        public IReadOnlyList<LanePath> SourcePaths => SourcePathsKey;

        private readonly List<LanePath> DestPathsKey = new List<LanePath>();
        public IReadOnlyList<LanePath> DestPaths => DestPathsKey;

        public LanePin? ConnectedPin { get; private set; } = null;

        public LanePin(NetworkPort port, Lane definition)
        {
            Port = port;
            Definition = definition;
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

        public void ConnectTo(LanePin pin)
        {
            if (!Definition.IsOpposite(pin.Definition)) throw new NotSupportedException("ピンの形状が一致しません。");

            ConnectedPin = pin;
            pin.ConnectedPin = this;
        }

        public void Disconnect()
        {
            if (ConnectedPin is null) return;

            ConnectedPin.ConnectedPin = null;
            ConnectedPin = null;
        }
    }
}
