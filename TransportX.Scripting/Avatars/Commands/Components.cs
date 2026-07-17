using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Components
    {
        private readonly ScriptAvatar Avatar;
        private readonly ConcurrentDictionary<Type, IComponentCommand> Instances = [];

        internal Components(ScriptAvatar avatar)
        {
            Avatar = avatar;
        }

        /// <summary>
        /// アバター全体にアタッチされているコンポーネントを取得します。指定したコンポーネントを初めて取得する場合は、自動的に新しいインスタンスが生成されます。
        /// </summary>
        /// <typeparam name="T">コンポーネントに対応するコマンドの型。</typeparam>
        /// <returns><typeparamref name="T"/> 型のコンポーネントコマンド。</returns>
        public T Get<T>() where T : class, IAvatarInstantiable<T>, IComponentCommand
        {
            T instance = (T)Instances.GetOrAdd(typeof(T), type =>
            {
                try
                {
                    T component = T.Create(Avatar);
                    Avatar.Components.Add(component.Source.GetType(), component.Source);
                    return component;
                }
                catch (Exception ex)
                {
                    ScriptError error = new(ErrorLevel.Error, ex, $"型 '{typeof(T)}' のコンポーネントコマンドの自動生成に失敗しました。");
                    Avatar.ErrorCollector.Report(error);
                    return default!;
                }
            });
            return instance;
        }
    }
}
