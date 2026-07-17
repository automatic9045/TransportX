using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Triggers
    {
        private readonly ScriptAvatar Avatar;
        private TickCommander? TickCommander = null;

        private event Action<Commander>? DisposeEvent;
        private event Action<Commander>? StartEvent;
        private event Action<TickCommander>? TickEvent;

        internal Triggers(ScriptAvatar avatar)
        {
            Avatar = avatar;
        }

        private void OnDispose(Action<Commander> action)
        {
            DisposeEvent += action;
        }

        public void OnDispose(Action action)
        {
            OnDispose(_ => action());
        }

        public void OnDispose(string scriptPath)
        {
            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(Avatar.AvatarContext, scriptPath, Avatar.ErrorCollector, false);
            OnDispose(commander => script.RunAsync(commander, Avatar.ErrorCollector).Wait());
        }

        private void OnStart(Action<Commander> action)
        {
            StartEvent += action;
        }

        public void OnStart(Action action)
        {
            OnStart(_ => action());
        }

        public void OnStart(string scriptPath)
        {
            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(Avatar.AvatarContext, scriptPath, Avatar.ErrorCollector, false);
            OnStart(commander => script.RunAsync(commander, Avatar.ErrorCollector).Wait());
        }

        private void OnTick(Action<TickCommander> action)
        {
            TickEvent += action;
        }

        public void OnTick(Action<TimeSpan> action)
        {
            OnTick(commander => action(commander.Elapsed));
        }

        public void OnTick(string scriptPath)
        {
            UserScript<TickCommander, object> script = UserScript<TickCommander, object>.FromFile(Avatar.AvatarContext, scriptPath, Avatar.ErrorCollector, false);
            OnTick(commander => script.RunAsync(commander, Avatar.ErrorCollector).Wait());
        }

        internal void Dispose()
        {
            DisposeEvent?.Invoke(Avatar.Commander);
        }

        internal void Start()
        {
            StartEvent?.Invoke(Avatar.Commander);
        }

        internal void Tick(TimeSpan elapsed)
        {
            TickCommander ??= new TickCommander(Avatar.Commander);
            TickCommander.Elapsed = elapsed;
            TickEvent?.Invoke(TickCommander);
        }
    }
}
