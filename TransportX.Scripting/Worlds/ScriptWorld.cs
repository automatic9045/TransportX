using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Collections;
using TransportX.Communication;
using TransportX.Dependency;
using TransportX.Environment;
using TransportX.Worlds;

using TransportX.Scripting.Collections;
using TransportX.Scripting.Worlds.Commands;

namespace TransportX.Scripting.Worlds
{
    public class ScriptWorld : WorldBase
    {
        public override IModelCollection Models => ModelsKey;
        internal ScriptModelCollection ModelsKey { get; }

        internal ScriptSoundCollection SoundsKey { get; }
        public override ISoundCollection Sounds => SoundsKey;

        public SignalBus SignalBus { get; }

        public new EnvironmentProfile DefaultEnvironment
        {
            get => base.DefaultEnvironment;
            set => base.DefaultEnvironment = value;
        }

        public new TransportX.Worlds.DirectionalLight DirectionalLight
        {
            get => base.DirectionalLight;
            set => base.DirectionalLight = value;
        }

        public string ScriptPath { get; }
        public Commander Commander { get; }

        public ScriptWorld(PluginLoadContext context, WorldBuilder builder) : base(context, builder)
        {
            ModelsKey = new ScriptModelCollection(ErrorCollector);
            SoundsKey = new ScriptSoundCollection(ErrorCollector);
            SignalBus = new SignalBus();

            if (Info.Args.Count == 0) throw new InvalidOperationException("ワールドファイルのパスが指定されていません。");

            ScriptPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Info.InfoPath)!, Info.Args[0]));
            ScriptError.DefaultLocation = ScriptPath;
            BaseDirectory = Path.GetDirectoryName(ScriptPath)!;

            Commander = new Commander(this);

            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(WorldContext, ScriptPath, ErrorCollector, true);
            if (ErrorCollector.HasFatalError) return;

            script.RunAsync(Commander, ErrorCollector).Wait();
        }

        public override void Dispose()
        {
            Commander.Dispose();
            base.Dispose();
        }

        public override void Tick(TimeSpan elapsed)
        {
            Commander.Tick(elapsed);
            base.Tick(elapsed);
        }

        public override void OnStart()
        {
            base.OnStart();
            Commander.OnStart();
        }
    }
}
