using System;
using System.Collections.Generic;
using IOPath = System.IO.Path;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Avatars;
using TransportX.Cameras;
using TransportX.Collections;
using TransportX.Communication;
using TransportX.Dependency;
using TransportX.Network;
using TransportX.Physics;
using TransportX.Traffic;

using TransportX.Scripting.Avatars.Commands;
using TransportX.Scripting.Collections;

namespace TransportX.Scripting.Avatars
{
    public class ScriptAvatar : AvatarBase
    {
        public new Viewpoint DriverViewpoint
        {
            get => base.DriverViewpoint;
            set => base.DriverViewpoint = value;
        }

        public new Viewpoint BirdViewpoint
        {
            get => base.BirdViewpoint;
            set => base.BirdViewpoint = value;
        }

        public new float Width
        {
            get => base.Width;
            set => base.Width = value;
        }

        public new float Height
        {
            get => base.Height;
            set => base.Height = value;
        }

        public new float Length
        {
            get => base.Length;
            set => base.Length = value;
        }

        public override bool IsEnabled => true;
        public override ILanePath? Path => null;
        public override EntityDirection Heading => EntityDirection.Forward;
        public override float S => 0;
        public override float SVelocity => 0;

        internal ScriptModelCollection ModelsKey { get; }
        public IModelCollection Models => ModelsKey;

        internal ScriptSoundCollection SoundsKey { get; }
        public ISoundCollection Sounds => SoundsKey;

        public SignalBus SignalBus { get; }

        public ScriptDictionary<string, ColliderGroupHandle> ColliderGroups { get; }

        public string ScriptPath { get; }
        public Commander Commander { get; }

        public ScriptAvatar(PluginLoadContext context, AvatarBuilder builder) : base(context, builder)
        {
            if (Info.Args.Count == 0) throw new InvalidOperationException("アバターファイルのパスが指定されていません。");


            ModelsKey = new ScriptModelCollection(ErrorCollector);
            SoundsKey = new ScriptSoundCollection(ErrorCollector);

            SignalBus = new SignalBus();

            ColliderGroups = new ScriptDictionary<string, ColliderGroupHandle>(ErrorCollector, "衝突判定グループ", key => ColliderGroupHandle.NewGroup())
            {
                { string.Empty, Structure.DefaultGroup },
                { "__Skip", ColliderGroupHandle.Skip },
            };


            ScriptPath = IOPath.GetFullPath(IOPath.Combine(IOPath.GetDirectoryName(Info.InfoPath)!, Info.Args[0]));
            ScriptError.DefaultLocation = ScriptPath;
            BaseDirectory = IOPath.GetDirectoryName(ScriptPath)!;

            Commander = new Commander(this);

            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(AvatarContext, ScriptPath, ErrorCollector, true);
            if (ErrorCollector.HasFatalError) return;

            script.RunAsync(Commander, ErrorCollector).Wait();


            Commander.RegisterComponents();
        }

        public override void Dispose()
        {
            Commander.Dispose();
            base.Dispose();
            Models.Dispose();
        }

        public override void OnStart()
        {
            base.OnStart();
            Commander.OnStart();
        }

        public override void SubTick(TimeSpan elapsed)
        {
            base.SubTick(elapsed);
            Commander.SubTick(elapsed);
        }

        public override void Tick(TimeSpan elapsed)
        {
            base.Tick(elapsed);
            Commander.Tick(elapsed);
        }

        public override bool Spawn(ILanePath path, EntityDirection heading, float s)
        {
            throw new NotSupportedException();
        }
    }
}
