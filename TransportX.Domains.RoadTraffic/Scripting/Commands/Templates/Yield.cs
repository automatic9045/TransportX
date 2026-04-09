using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Scripting;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands.Templates
{
    internal class Yield : ITemplateComponent<ILanePath>
    {
        public IReadOnlyList<JunctionPathSegment> PrioritySegments { get; }

        public Yield(IReadOnlyList<JunctionPathSegment> prioritySegments)
        {
            PrioritySegments = prioritySegments;
        }

        public void Build(ILanePath parent, IErrorCollector errorCollector)
        {
            List<LanePathSegment> prioritySegments = new(PrioritySegments.Count);
            for (int i = 0; i < PrioritySegments.Count; i++)
            {
                JunctionPathSegment segmentSource = PrioritySegments[i];
                ILanePath? priorityPath = parent.Owner.Paths.FirstOrDefault(path => path.Name == segmentSource.PathKey);
                if (priorityPath is null)
                {
                    Error error = new(ErrorLevel.Error, $"進路パス '{segmentSource.PathKey}' が見つかりません。", null);
                    errorCollector.Report(error);
                    continue;
                }

                float minS = float.Clamp((float)segmentSource.MinS, 0, priorityPath.Length);
                float maxS = float.Clamp((float)segmentSource.MaxS, 0, priorityPath.Length);

                LanePathSegment segment = new(priorityPath, minS, maxS);
                prioritySegments.Add(segment);
            }

            YieldComponent component = new(prioritySegments);
            parent.Components.Add(component);
        }
    }
}
