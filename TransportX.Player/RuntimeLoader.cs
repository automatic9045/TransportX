using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;
using TransportX.Rendering;
using TransportX.Worlds;

namespace TransportX
{
    internal class RuntimeLoader
    {
        private readonly Platform Platform;
        private readonly IDXHost DXHost;
        private readonly IDXClient DXClient;

        public RuntimeLoader(Platform platform, IDXHost dxHost, IDXClient dxClient)
        {
            Platform = platform;
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

            RuntimeHost runtimeHost = new()
            {
                Context = context,
                Platform = Platform,
                DXHost = DXHost,
                DXClient = DXClient,
            };
            IRuntime runtime = runtimeFactory.Create(runtimeHost, worldInfo);
            return runtime;
        }
    }
}
