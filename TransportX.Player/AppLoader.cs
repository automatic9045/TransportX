using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;
using TransportX.Worlds;

namespace TransportX.Player.Launcher
{
    internal class AppLoader
    {
        private readonly Platform Platform;

        public AppLoader(Platform platform)
        {
            Platform = platform;
        }

        public (AppHost AppHost, IApp App) Load(AppReference reference, IAppParameters parameters)
        {
            PluginLoadContext context = null!;
            Type type;

            switch (reference)
            {
                case TypeAppReference typeReference:
                {
                    if (!IsAppType(typeReference.FactoryType))
                    {
                        throw new ArgumentException(
                            $"型 {typeReference.FactoryType.FullName} は {nameof(IAppFactory)} を実装した具象クラスではありません。", nameof(reference));
                    }

                    type = typeReference.FactoryType;
                    break;
                }

                case PathAppReference pathReference:
                {
                    context = PluginLoadContext.CreateAndLoadPlugin(pathReference.Path, out Assembly assembly);

                    if (pathReference.FactoryTypeFullName is null)
                    {
                        Type[] types = assembly.GetTypes().Where(IsAppType).ToArray();
                        switch (types.Length)
                        {
                            case 0:
                            {
                                string fileName = Path.GetFileName(pathReference.Path);
                                throw new ArgumentException($"{fileName} にはアプリケーションが定義されていません。", nameof(reference));
                            }

                            case 1:
                            {
                                type = types[0];
                                break;
                            }

                            default:
                            {
                                string fileName = Path.GetFileName(pathReference.Path);
                                throw new ArgumentException($"{fileName} には 2 つ以上のアプリケーションが定義されています。", nameof(reference));
                            }
                        }
                    }
                    else
                    {
                        string fileName = Path.GetFileName(pathReference.Path);
                        type = assembly.GetType(pathReference.FactoryTypeFullName)
                            ?? throw new ArgumentException($"{fileName} で型 {pathReference.FactoryTypeFullName} が見つかりませんでした。", nameof(reference));
                    }
                    break;
                }

                default:
                    throw new NotSupportedException();
            }

            IAppFactory appFactory = (IAppFactory)Activator.CreateInstance(type)!;

            AppHost appHost = new()
            {
                Context = context,
                Platform = Platform,
                CurrentReference = reference,
            };

            IApp app = appFactory.Create(appHost, parameters);
            return (appHost, app);


            bool IsAppType(Type type) => type.IsClass && !type.IsAbstract && typeof(IAppFactory).IsAssignableFrom(type);
        }
    }
}
