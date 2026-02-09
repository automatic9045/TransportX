using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace TransportX.Scripting.Commands
{
    public class Triggers
    {
        private readonly ScriptWorld World;

        private event Action<Commander>? DisposeEvent;
        private event Action<TickCommander>? TickEvent;

        internal Triggers(ScriptWorld world)
        {
            World = world;
        }

        public void OnDispose(Action<Commander> action)
        {
            DisposeEvent += action;
        }

        public void OnDispose(string scriptPath)
        {
            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(scriptPath, World.ErrorCollector, false);
            OnDispose(commander => script.RunAsync(commander, World.ErrorCollector).Wait());
        }

        public void OnTick(Action<TickCommander> action)
        {
            TickEvent += action;
        }

        public void OnTick(string scriptPath)
        {
            UserScript<TickCommander, object> script = UserScript<TickCommander, object>.FromFile(scriptPath, World.ErrorCollector, false);
            OnTick(commander => script.RunAsync(commander, World.ErrorCollector).Wait());
        }

        internal void Dispose()
        {
            DisposeEvent?.Invoke(World.Commander);
        }

        internal void Tick(TimeSpan elapsed)
        {
            TickCommander commander = new TickCommander(World.Commander, elapsed);
            TickEvent?.Invoke(commander);
        }
    }
}
