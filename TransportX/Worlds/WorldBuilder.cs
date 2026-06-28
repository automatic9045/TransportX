using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using TransportX.Cameras;
using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering.Backend;

namespace TransportX.Worlds
{
    public class WorldBuilder
    {
        public IWorldInfo Info { get; }

        public required Platform Platform { get; init; }
        public required IDXHost DXHost { get; init; }
        public required IDXClient DXClient { get; init; }
        public required IPhysicsHost PhysicsHost { get; init; }
        public required WorldOptions Options { get; init; }
        public required IErrorCollector ErrorCollector { get; init; }
        public required PluginLoadContext AppContext { get; init; }
        public required TimeManager TimeManager { get; init; }
        public required InputManager InputManager { get; init; }
        public required Camera Camera { get; init; }

        public WorldBuilder(IWorldInfo info)
        {
            Info = info;
        }

        internal protected WorldBase Build()
        {
            string path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Info.InfoPath)!, Info.Path));
            if (!File.Exists(path)) throw new FileNotFoundException("ワールドファイルが見つかりません。", path);

            PluginLoadContext context = PluginLoadContext.CreateAndLoadPlugin(path, out Assembly assembly);
            AppContext.Children.Add(context);

            Type[] worldTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(WorldBase)))
                .ToArray();

            if (worldTypes.Length == 0)
            {
                throw new InvalidOperationException($"'{path}' にはワールドが定義されていません。");
            }

            Type worldType;
            if (Info.Identifier is null)
            {
                if (worldTypes.Length == 1)
                {
                    worldType = worldTypes[0];
                }
                else
                {
                    throw new InvalidOperationException($"'{path}' には 2 つ以上のワールドが定義されています。");
                }
            }
            else
            {
                Type? type = null;
                foreach (Type t in worldTypes)
                {
                    WorldIdentifierAttribute? identifierAttribute = t.GetCustomAttribute<WorldIdentifierAttribute>();
                    if (identifierAttribute?.Identifier == Info.Identifier)
                    {
                        type = t;
                    }
                }

                if (type is null)
                {
                    throw new InvalidOperationException($"'{path}' にはワールド '{Info.Identifier}' が定義されていません。");
                }
                worldType = type;
            }

            ConstructorInfo constructor = worldType.GetConstructor([typeof(PluginLoadContext), typeof(WorldBuilder)])
                ?? throw new InvalidOperationException($"{worldType.Name} にはパラメータが" +
                $" {nameof(PluginLoadContext)}, {nameof(WorldBuilder)} のコンストラクタが定義されていません。");

            WorldBase world = (WorldBase)constructor.Invoke([context, this]);
            return world;
        }
    }
}
