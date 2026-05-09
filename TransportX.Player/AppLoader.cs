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
    internal class AppLoader
    {
        private readonly Platform Platform;

        public AppLoader(Platform platform)
        {
            Platform = platform;
        }

        public IApp Load(IWorldInfo worldInfo)
        {
            PluginLoadContext context = PluginLoadContext.CreateAndLoadPlugin(worldInfo.AppPath, out Assembly assembly);
            Type[] types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IAppFactory).IsAssignableFrom(t))
                .ToArray();

            switch (types.Length)
            {
                case 0:
                {
                    string fileName = Path.GetFileName(worldInfo.AppPath);
                    throw new ArgumentException($"{fileName} にはランタイムが定義されていません。", nameof(worldInfo));
                }

                case 1:
                    break;

                default:
                {
                    string fileName = Path.GetFileName(worldInfo.AppPath);
                    throw new ArgumentException($"{fileName} には 2 つ以上のランタイムが定義されています。", nameof(worldInfo));
                }
            }

            Type type = types[0];
            IAppFactory appFactory = (IAppFactory)Activator.CreateInstance(type)!;

            AppHost appHost = new()
            {
                Context = context,
                Platform = Platform,
            };
            IApp app = appFactory.Create(appHost, worldInfo);
            return app;
        }
    }
}
