using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Network;

using TransportX.Scripting;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Commands
{
    internal class YieldTemplateComponent : ITemplateComponent<ILanePath>
    {
        public IReadOnlyList<string> PriorityPathKeys { get; }

        public YieldTemplateComponent(IReadOnlyList<string> priorityPathKeys)
        {
            PriorityPathKeys = priorityPathKeys;
        }

        public void Build(ILanePath parent, IErrorCollector errorCollector)
        {
            List<ILanePath> priorityPaths = new(PriorityPathKeys.Count);
            for (int i = 0; i < PriorityPathKeys.Count; i++)
            {
                string key = PriorityPathKeys[i];
                ILanePath? priorityPath = parent.Owner.Paths.FirstOrDefault(path => path.Name == key);
                if (priorityPath is null)
                {
                    Error error = new(ErrorLevel.Error, $"進路パス '{key}' が見つかりません。", null);
                    errorCollector.Report(error);
                    continue;
                }

                priorityPaths.Add(priorityPath);
            }

            YieldComponent component = new(priorityPaths);
            parent.Components.Add(component);
        }
    }
}
