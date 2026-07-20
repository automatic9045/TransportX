using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Communication;

using CommonCommands = TransportX.Scripting.Commands;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Commander
    {
        public ScriptAvatar Avatar { get; }

        public Components Components { get; }
        public Debug Debug { get; }
        public CommonCommands.Input Input { get; }
        public Models Models { get; }
        public CommonCommands.Signals Signals { get; }
        public Sounds Sounds { get; }
        public Spec Spec { get; }
        public Structure Structure { get; }
        public Triggers Triggers { get; }
        public Viewpoints Viewpoints { get; }

        internal Commander(ScriptAvatar avatar)
        {
            Avatar = avatar;

            Signals = new CommonCommands.Signals(avatar.SignalBus);

            Components = new Components(avatar);
            Debug = new Debug(avatar);
            Input = new CommonCommands.Input(Signals, avatar.InputManager, avatar.ErrorCollector, avatar);
            Models = new Models(avatar);
            Sounds = new Sounds(avatar);
            Spec = new Spec(avatar);
            Structure = new Structure(avatar);
            Triggers = new Triggers(avatar);
            Viewpoints = new Viewpoints(avatar);
        }

        private protected Commander(Commander parent)
        {
            Avatar = parent.Avatar;

            Components = parent.Components;
            Signals = parent.Signals;
            Debug = parent.Debug;
            Input = parent.Input;
            Models = parent.Models;
            Sounds = parent.Sounds;
            Spec = parent.Spec;
            Structure = parent.Structure;
            Triggers = parent.Triggers;
            Viewpoints = parent.Viewpoints;
        }

        internal void Dispose()
        {
            Input.Dispose();
            Sounds.Dispose();
            Structure.Dispose();
            Triggers.Dispose();
        }

        /// <summary>
        /// アバター全体にアタッチされているコンポーネントを取得します。指定したコンポーネントを初めて取得する場合は、自動的に新しいインスタンスが生成されます。
        /// </summary>
        /// <typeparam name="T">コンポーネントに対応するコマンドの型。</typeparam>
        /// <returns><typeparamref name="T"/> 型のコンポーネントコマンド。</returns>
        public T Component<T>() where T : class, IAvatarInstantiable<T>, IComponentCommand
        {
            return Components.Get<T>();
        }

        internal void RegisterComponents()
        {
            Structure.RegisterComponents();
        }

        internal void OnStart()
        {
            Triggers.Start();
        }

        internal void SubTick(TimeSpan elapsed)
        {
            Signals.Source.SubTick(elapsed);
        }

        internal void Tick(TimeSpan elapsed)
        {
            Input.Tick(elapsed);
            Sounds.Tick(elapsed);
            Triggers.Tick(elapsed);
        }
    }
}
