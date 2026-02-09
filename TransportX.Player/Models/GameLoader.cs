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
    internal class GameLoader
    {
        private readonly IDXHost DXHost;
        private readonly IDXClient DXClient;

        public GameLoader(IDXHost dxHost, IDXClient dxClient)
        {
            DXHost = dxHost;
            DXClient = dxClient;
        }

        public IGame Load(IWorldInfo worldInfo)
        {
            PluginLoadContext context = PluginLoadContext.CreateAndLoadPlugin(worldInfo.GamePath, out Assembly assembly);
            Type[] types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IGameFactory).IsAssignableFrom(t))
                .ToArray();

            switch (types.Length)
            {
                case 0:
                {
                    string fileName = Path.GetFileName(worldInfo.GamePath);
                    throw new ArgumentException($"{fileName} にはゲームファクトリーが定義されていません。", nameof(worldInfo));
                }

                case 1:
                    break;

                default:
                {
                    string fileName = Path.GetFileName(worldInfo.GamePath);
                    throw new ArgumentException($"{fileName} には 2 つ以上のゲームファクトリーが定義されています。", nameof(worldInfo));
                }
            }

            Type type = types[0];
            IGameFactory gameFactory = (IGameFactory)Activator.CreateInstance(type)!;
            IGame game = gameFactory.Create(context, DXHost, DXClient, worldInfo);
            return game;
        }
    }
}
