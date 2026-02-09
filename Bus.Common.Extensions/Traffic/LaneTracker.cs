using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery.Networks;
using Bus.Common.Traffic;

using Bus.Common.Extensions.Utilities;

namespace Bus.Common.Extensions.Traffic
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
        public ParticipantDirection Heading { get; private set; } = ParticipantDirection.Forward;
        public float S { get; private set; } = 0;
        public float SVelocity { get; private set; } = 0;

        public IReadOnlyList<LanePathView> History => PathViewHistory;

        public event EventHandler<PathChangedEventArgs>? PathChanged;

        public LaneTracker(IRouteNavigator navigator, float width, float height, float length)
        {
            Navigator = navigator;
            Width = width;
            Height = height;
            Length = length;
        }

        public void Initialize(ILanePath path, ParticipantDirection heading, float s)
        {
            Path = path;
            Heading = heading;
            S = s;

            PathViewHistory.Clear();
            Navigator.Reset();

            IsEnabled = true;
            PathChanged?.Invoke(this, new PathChangedEventArgs(null, path));
        }

        public void Tick(float acceleration, TimeSpan elapsed)
        {
            if (!IsEnabled || Path is null) throw new InvalidOperationException();

            float oldVelocity = SVelocity;
            SVelocity += acceleration * (float)elapsed.TotalSeconds;
            if (float.Sign(oldVelocity * SVelocity) == -1) SVelocity = 0;

            LanePathView pathView = new(Path, Heading);
            Navigator.Update(pathView, pathView.ToViewVelocity(SVelocity) * 3.6f + 10);

            float viewS = pathView.ToViewS(S + SVelocity * (float)elapsed.TotalSeconds);
            while (Path.Length < viewS)
            {
                ILanePath oldPath = Path;
                PathViewHistory.Add(pathView);

                float totalHistoryLength = PathViewHistory.Sum(view => view.Source.Length);
                while (Length < totalHistoryLength && 0 < PathViewHistory.Count)
                {
                    totalHistoryLength -= PathViewHistory[PathViewHistory.Count - 1].Source.Length;
                    if (totalHistoryLength <= Length) break;

                    PathViewHistory.RemoveOldest();
                }

                LanePathView oldPathView = pathView;
                if (Navigator.TryPop(out pathView))
                {
                    viewS -= Path.Length;
                    Path = pathView.Source;
                    Heading = pathView.Reverse ? ParticipantDirection.Backward : ParticipantDirection.Forward;

                    if (pathView.Reverse != oldPathView.Reverse) SVelocity = -SVelocity;
                }
                else
                {
                    Path = null;
                    SVelocity = 0;
                    PathViewHistory.Clear();

                    IsEnabled = false;
                    PathChanged?.Invoke(this, new PathChangedEventArgs(oldPath, null));
                    break;
                }

                PathChanged?.Invoke(this, new PathChangedEventArgs(oldPath, Path));
            }

            S = pathView.FromViewS(viewS);
        }
    }
}
