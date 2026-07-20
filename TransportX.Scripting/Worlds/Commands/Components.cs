using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Worlds.Commands
{
    public class Components
    {
        private readonly ScriptWorld World;
        private readonly ConcurrentDictionary<Type, IComponentCommand> Instances = [];

        internal Components(ScriptWorld world)
        {
            World = world;
        }

        /// <summary>
        /// ワールド全体にアタッチされているコンポーネントを取得します。指定したコンポーネントを初めて取得する場合は、自動的に新しいインスタンスが生成されます。
        /// </summary>
        /// <typeparam name="T">コンポーネントに対応するコマンドの型。</typeparam>
        /// <returns><typeparamref name="T"/> 型のコンポーネントコマンド。</returns>
        public T Get<T>() where T : class, IWorldInstantiable<T>, IComponentCommand
        {
            T instance = (T)Instances.GetOrAdd(typeof(T), type =>
            {
                try
                {
                    T component = T.Create(World);
                    World.Components.Add(component.Source.GetType(), component.Source);
                    return component;
                }
                catch (Exception ex)
                {
                    ScriptError error = new(ErrorLevel.Error, ex, $"型 '{typeof(T)}' のコンポーネントコマンドの自動生成に失敗しました。");
                    World.ErrorCollector.Report(error);
                    return default!;
                }
            });
            return instance;
        }
    }
}
