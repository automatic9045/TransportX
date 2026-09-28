using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Network;
using TransportX.Traffic;

namespace TransportX.Extensions.Traffic
{
    public class LaneTracker : ILaneTracker
    {
        private readonly IRouteNavigator Navigator;
        private readonly RingBuffer<LanePathView> PathViewHistory = [];

        public float Width { get; }
        public float Height { get; }
        public float Length { get; }

        public bool IsEnabled { get; private set; } = false;
        public ILanePath? Path { get; private set; } = null;
        public EntityDirection Heading { get; private set; } = EntityDirection.Forward;
        public float S { get; private set; } = 0;
        public float SVelocity { get; private set; } = 0;

        public IReadOnlyList<LanePathView> History => PathViewHistory;

        public event PathChangedEventHandler? PathChanged;

        public LaneTracker(IRouteNavigator navigator, float width, float height, float length)
        {
            Navigator = navigator;
            Width = width;
            Height = height;
            Length = length;
        }

        public void Initialize(ILanePath path, EntityDirection heading, float s)
        {
            Path = path;
            Heading = heading;
            S = s;

            PathViewHistory.Clear();
            Navigator.Reset();

            IsEnabled = true;
            PathChanged?.Invoke(null, path);
        }

        public void Tick(float acceleration, TimeSpan elapsed)
        {
            if (!IsEnabled || Path is null) throw new InvalidOperationException();

            float oldVelocity = SVelocity;
            SVelocity += acceleration * (float)elapsed.TotalSeconds;
            if (float.Sign(oldVelocity * SVelocity) == -1) SVelocity = 0;

            LanePathView pathView = new(Path, Heading);
            float planLength = float.Max(50, pathView.ToViewVelocity(SVelocity) * 5);
            Navigator.Update(pathView, planLength);

            float viewS = pathView.ToViewS(S + SVelocity * (float)elapsed.TotalSeconds);
            while (Path.Length < viewS)
            {
                ILanePath oldPath = Path;
                PathViewHistory.Add(pathView);

                float totalHistoryLength = 0;
                for (int i = 0; i < PathViewHistory.Count; i++)
                {
                    totalHistoryLength += PathViewHistory[i].Source.Length;
                }

                while (0 < PathViewHistory.Count)
                {
                    float oldestLength = PathViewHistory[0].Source.Length;
                    if (totalHistoryLength - oldestLength < Length) break;

                    totalHistoryLength -= oldestLength;
                    PathViewHistory.RemoveOldest();
                }

                LanePathView oldPathView = pathView;
                if (Navigator.TryPop(out pathView))
                {
                    viewS -= Path.Length;
                    Path = pathView.Source;
                    Heading = pathView.Reverse ? EntityDirection.Backward : EntityDirection.Forward;

                    if (pathView.Reverse != oldPathView.Reverse) SVelocity = -SVelocity;
                }
                else
                {
                    Path = null;
                    SVelocity = 0;
                    PathViewHistory.Clear();

                    IsEnabled = false;
                    PathChanged?.Invoke(oldPath, null);
                    break;
                }

                PathChanged?.Invoke(oldPath, Path);
            }

            S = pathView.FromViewS(viewS);
        }
    }
}
