using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using TransportX;
using TransportX.Dependency;
using TransportX.Rendering;
using TransportX.Worlds;

namespace TransportX.Models
{
    internal class RuntimeLoader
    {
        private readonly IDXHost DXHost;
        private readonly IDXClient DXClient;

        public RuntimeLoader(IDXHost dxHost, IDXClient dxClient)
        {
            DXHost = dxHost;
            DXClient = dxClient;
        }

        public IRuntime Load(IWorldInfo worldInfo)
        {
            PluginLoadContext context = PluginLoadContext.CreateAndLoadPlugin(worldInfo.RuntimePath, out Assembly assembly);
            Type[] types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IRuntimeFactory).IsAssignableFrom(t))
                .ToArray();

            switch (types.Length)
            {
                case 0:
                {
                    string fileName = Path.GetFileName(worldInfo.RuntimePath);
                    throw new ArgumentException($"{fileName} にはランタイムが定義されていません。", nameof(worldInfo));
                }

                case 1:
                    break;

                default:
                {
                    string fileName = Path.GetFileName(worldInfo.RuntimePath);
                    throw new ArgumentException($"{fileName} には 2 つ以上のランタイムが定義されています。", nameof(worldInfo));
                }
            }

            Type type = types[0];
            IRuntimeFactory runtimeFactory = (IRuntimeFactory)Activator.CreateInstance(type)!;
            IRuntime runtime = runtimeFactory.Create(context, DXHost, DXClient, worldInfo);
            return runtime;
        }
    }
}
