using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Domains.RoadVehicles.Physics
{
    public class Simulation
    {
        private static readonly int IterationCount = 10;

        public List<IConstraint> Constraints { get; } = [];

        private readonly List<IModule> RawModules = [];
        private bool IsDirty = false;

        private readonly List<IModule> SortedModules = [];
        private readonly List<IExternalModule> SortedExternalModules = [];

        private readonly List<IController> Controllers = [];

        public Simulation()
        {
        }

        public void AddModule(IModule module)
        {
            RawModules.Add(module);
            Constraints.AddRange(module.Constraints);
            IsDirty = true;
        }

        public void AddController(IController controller)
        {
            Controllers.Add(controller);
        }

        public void Tick(TimeSpan elapsed)
        {
            if (IsDirty)
            {
                BuildExecutionGraph();
            }

            foreach (IExternalModule module in SortedExternalModules)
            {
                module.Pull();
            }

            foreach (IController controller in Controllers)
            {
                controller.Tick(elapsed);
            }

            foreach (IModule module in SortedModules)
            {
                module.Tick(elapsed);
            }

            for (int i = 0; i < IterationCount; i++)
            {
                foreach (IConstraint constraint in Constraints)
                {
                    constraint.Solve();
                }
            }

            foreach (IModule module in SortedModules)
            {
                module.PropagateTorque();
            }

            foreach (IExternalModule module in SortedExternalModules)
            {
                module.Push();
            }
        }

        void BuildExecutionGraph()
        {
            SortedModules.Clear();
            SortedExternalModules.Clear();

            Dictionary<IModule, int> inDegrees = [];
            Dictionary<IModule, List<IModule>> graph = [];

            foreach (IModule module in RawModules)
            {
                inDegrees[module] = 0;
                graph[module] = [];
            }

            foreach (IModule moduleA in RawModules)
            {
                foreach (IModule moduleB in RawModules)
                {
                    if (moduleA == moduleB) continue;

                    bool hasDependency = false;
                    foreach (Shaft outShaft in moduleA.OutputShafts)
                    {
                        foreach (Shaft inShaft in moduleB.InputShafts)
                        {
                            if (outShaft == inShaft)
                            {
                                hasDependency = true;
                                break;
                            }
                        }
                        if (hasDependency) break;
                    }

                    if (hasDependency)
                    {
                        graph[moduleA].Add(moduleB);
                        inDegrees[moduleB]++;
                    }
                }
            }

            Queue<IModule> queue = [];
            foreach (KeyValuePair<IModule, int> item in inDegrees)
            {
                if (item.Value == 0)
                {
                    queue.Enqueue(item.Key);
                }
            }

            while (0 < queue.Count)
            {
                IModule current = queue.Dequeue();
                SortedModules.Add(current);

                foreach (IModule neighbor in graph[current])
                {
                    inDegrees[neighbor]--;
                    if (inDegrees[neighbor] == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (SortedModules.Count < RawModules.Count)
            {
                foreach (IModule module in RawModules)
                {
                    if (!SortedModules.Contains(module))
                    {
                        SortedModules.Add(module);
                    }
                }
            }

            SortedExternalModules.AddRange(SortedModules.OfType<IExternalModule>());

            IsDirty = false;
        }
    }
}
