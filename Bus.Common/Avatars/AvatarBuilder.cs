using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Dependency;
using Bus.Common.Diagnostics;
using Bus.Common.Input;
using Bus.Common.Physics;
using Bus.Common.Rendering;
using Bus.Common.Worlds;

namespace Bus.Common.Avatars
{
    public class AvatarBuilder
    {
        public required IDXHost DXHost { get; init; }
        public required IDXClient DXClient { get; init; }
        public required IPhysicsHost PhysicsHost { get; init; }
        public required IErrorCollector ErrorCollector { get; init; }
        public required PluginLoadContext GameContext { get; init; }
        public required PluginLoadContext WorldContext { get; init; }
        public required ITimeManager TimeManager { get; init; }
        public required InputManager InputManager { get; init; }
        public required Camera Camera { get; init; }
        public required WorldBase World { get; init; }

        public AvatarBuilder()
        {
        }

        internal protected AvatarBase Build(string path, string? identifier)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("アバターファイルが見つかりません。", path);

            PluginLoadContext context = PluginLoadContext.CreateAndLoadPlugin(path, out Assembly assembly);
            WorldContext.Children.Add(context);

            Type[] avatarTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AvatarBase)))
                .ToArray();

            if (avatarTypes.Length == 0)
            {
                string fileName = Path.GetFileName(path);
                throw new ArgumentException($"{fileName} にはアバターが定義されていません。", nameof(path));
            }

            Type avatarType;
            if (identifier is null)
            {
                if (avatarTypes.Length == 1)
                {
                    avatarType = avatarTypes[0];
                }
                else
                {
                    string fileName = Path.GetFileName(path);
                    throw new ArgumentException($"{fileName} には 2 つ以上のアバターが定義されています。", nameof(path));
                }
            }
            else
            {
                Type? type = null;
                foreach (Type t in avatarTypes)
                {
                    AvatarIdentifierAttribute? identifierAttribute = t.GetCustomAttribute<AvatarIdentifierAttribute>();
                    if (identifierAttribute?.Identifier == identifier)
                    {
                        type = t;
                    }
                }

                if (type is null)
                {
                    string fileName = Path.GetFileName(path);
                    throw new ArgumentException($"{fileName} にはアバター '{identifier}' が定義されていません。", nameof(identifier));
                }
                avatarType = type;
            }

            ConstructorInfo constructor = avatarType.GetConstructor([typeof(PluginLoadContext), typeof(AvatarBuilder)])
                ?? throw new ArgumentException($"{avatarType.Name} にはパラメータが " +
                $"{nameof(PluginLoadContext)}, {nameof(AvatarBuilder)} のコンストラクタが定義されていません。", nameof(path));

            AvatarBase avatar = (AvatarBase)constructor.Invoke([context, this]);
            return avatar;
        }
    }
}
