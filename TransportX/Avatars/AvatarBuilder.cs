using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;
using TransportX.Worlds;

namespace TransportX.Avatars
{
    public class AvatarBuilder
    {
        public IAvatarInfo Info { get; }

        public required WorldBase World { get; init; }

        public AvatarBuilder(IAvatarInfo info)
        {
            Info = info;
        }

        internal protected AvatarBase Build()
        {
            if (!File.Exists(Info.Path)) throw new FileNotFoundException("アバターファイルが見つかりません。", Info.Path);

            PluginLoadContext context = PluginLoadContext.CreateAndLoadPlugin(Info.Path, out Assembly assembly);
            World.WorldContext.Children.Add(context);

            Type[] avatarTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AvatarBase)))
                .ToArray();

            if (avatarTypes.Length == 0)
            {
                throw new InvalidOperationException($"'{Info.Path}' にはアバターが定義されていません。");
            }

            Type avatarType;
            if (Info.Identifier is null)
            {
                if (avatarTypes.Length == 1)
                {
                    avatarType = avatarTypes[0];
                }
                else
                {
                    throw new InvalidOperationException($"'{Info.Path}' には 2 つ以上のアバターが定義されています。");
                }
            }
            else
            {
                Type? type = null;
                foreach (Type t in avatarTypes)
                {
                    AvatarIdentifierAttribute? identifierAttribute = t.GetCustomAttribute<AvatarIdentifierAttribute>();
                    if (identifierAttribute?.Identifier == Info.Identifier)
                    {
                        type = t;
                    }
                }

                if (type is null)
                {
                    throw new InvalidOperationException($"'{Info.Path}' にはアバター '{Info.Identifier}' が定義されていません。");
                }
                avatarType = type;
            }

            ConstructorInfo constructor = avatarType.GetConstructor([typeof(PluginLoadContext), typeof(AvatarBuilder)])
                ?? throw new ArgumentException($"{avatarType.Name} にはパラメータが " +
                $"{nameof(PluginLoadContext)}, {nameof(AvatarBuilder)} のコンストラクタが定義されていません。");

            AvatarBase avatar = (AvatarBase)constructor.Invoke([context, this]);
            return avatar;
        }
    }
}
