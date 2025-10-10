using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Bus.Common.Scripting.Commands
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
            Script script = CreateScript<Commander>(scriptPath);
            OnDispose(commander => script.RunAsync(commander));
        }

        public void OnTick(Action<TickCommander> action)
        {
            TickEvent += action;
        }

        public void OnTick(string scriptPath)
        {
            Script script = CreateScript<TickCommander>(scriptPath);
            OnTick(commander => script.RunAsync(commander));
        }

        private Script CreateScript<TGlobals>(string scriptPath)
        {
            string path = Path.GetFullPath(Path.Combine(World.BaseDirectory, scriptPath));
            string code = File.ReadAllText(path);
            ScriptOptions options = ScriptWorld.ScriptOptions.WithFilePath(path);
            Script script = CSharpScript.Create(code, options, typeof(TGlobals));
            return script;
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
