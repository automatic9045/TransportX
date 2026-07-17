using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Scripting;
using TransportX.Scripting.Avatars;
using TransportX.Scripting.Avatars.Commands;

using TransportX.Domains.RoadVehicles.Physics;
using TransportX.Domains.RoadVehicles.Powertrain.Controllers;

namespace TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ControllerFactories
{
    public class ScriptableControllerFactory : ControllerFactoryBase
    {
        private readonly TickCommander.Factory TickCommanderFactory;

        private Action OnInitValue = () => { };
        private Action<TimeSpan> OnTickValue = _ => { };

        public new ScriptableController? BuiltController { get; private set; } = null;

        internal ScriptableControllerFactory(ScriptAvatar avatar, string key) : base(avatar, key)
        {
            TickCommanderFactory = new TickCommander.Factory(Avatar.Commander);
        }

        public ScriptableControllerFactory OnInit(Action action)
        {
            OnInitValue = action;
            return this;
        }

        public ScriptableControllerFactory OnInit(string scriptPath)
        {
            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(Avatar.AvatarContext, scriptPath, Avatar.ErrorCollector, false);
            return OnInit(() => script.RunAsync(Avatar.Commander, Avatar.ErrorCollector).Wait());
        }

        public ScriptableControllerFactory OnTick(Action<TimeSpan> action)
        {
            OnTickValue = action;
            return this;
        }

        public ScriptableControllerFactory OnTick(string scriptPath)
        {
            UserScript<Commander, object> script = UserScript<Commander, object>.FromFile(Avatar.AvatarContext, scriptPath, Avatar.ErrorCollector, false);
            return OnTick(elapded =>
            {
                TickCommander commander = TickCommanderFactory.Tick(elapded);
                script.RunAsync(commander, Avatar.ErrorCollector).Wait();
            });
        }

        protected override IController OnBuild()
        {
            BuiltController = new ScriptableController(OnInitValue)
            {
                OnTick = OnTickValue,
            };
            return BuiltController;
        }
    }
}
