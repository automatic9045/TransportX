using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Scripting.Worlds.Commands
{
    public class Triggers
    {
        private readonly ScriptWorld World;
        private TickCommander? TickCommander = null;

        private event Action<Commander>? DisposeEvent;
        private event Action<Commander>? StartEvent;
        private event Action<TickCommander>? TickEvent;

        internal Triggers(ScriptWorld world)
        {
            World = world;
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
            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(World.WorldContext, scriptPath, World.ErrorCollector, false);
            OnDispose(commander => script.RunAsync(commander, World.ErrorCollector).Wait());
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
            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(World.WorldContext, scriptPath, World.ErrorCollector, false);
            OnStart(commander => script.RunAsync(commander, World.ErrorCollector).Wait());
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
            UserScript<TickCommander, object> script = UserScript<TickCommander, object>.FromFile(World.WorldContext, scriptPath, World.ErrorCollector, false);
            OnTick(commander => script.RunAsync(commander, World.ErrorCollector).Wait());
        }

        internal void Dispose()
        {
            DisposeEvent?.Invoke(World.Commander);
        }

        internal void Start()
        {
            StartEvent?.Invoke(World.Commander);
        }

        internal void Tick(TimeSpan elapsed)
        {
            TickCommander ??= new TickCommander(World.Commander);
            TickCommander.Elapsed = elapsed;
            TickEvent?.Invoke(TickCommander);
        }
    }
}
