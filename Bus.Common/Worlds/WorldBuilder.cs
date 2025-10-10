using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Bus.Common.Input;
using Bus.Common.Rendering;

namespace Bus.Common.Worlds
{
    public class WorldBuilder
    {
        public IWorldInfo Info { get; }

        public required IDXHost DXHost { get; init; }
        public required TimeManager TimeManager { get; init; }
        public required InputManager InputManager { get; init; }
        public required Camera Camera { get; init; }

        public WorldBuilder(IWorldInfo info)
        {
            Info = info;
        }

        internal protected WorldBase Build()
        {
            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Info.Path);
            Type[] worldTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(WorldBase)))
                .ToArray();

            if (worldTypes.Length == 0)
            {
                string fileName = Path.GetFileName(Info.Path);
                throw new ArgumentException($"{fileName} にはワールドが定義されていません。", nameof(Info));
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
                    string fileName = Path.GetFileName(Info.Path);
                    throw new ArgumentException($"{fileName} には 2 つ以上のワールドが定義されています。", nameof(Info));
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
                    string fileName = Path.GetFileName(Info.Path);
                    throw new ArgumentException($"{fileName} にはワールド '{Info.Identifier}' が定義されていません。", nameof(Info));
                }
                worldType = type;
            }

            ConstructorInfo constructor = worldType.GetConstructor([typeof(WorldBuilder)])
                ?? throw new ArgumentException($"{worldType.Name} にはパラメータが {nameof(WorldBuilder)} のコンストラクタが定義されていません。", nameof(Info));

            WorldBase world = (WorldBase)constructor.Invoke([this]);
            return world;
        }
    }
}
