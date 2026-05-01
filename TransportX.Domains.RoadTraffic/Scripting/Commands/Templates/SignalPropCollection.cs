using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Spatial;

using TransportX.Extensions.Network.Elements;

using TransportX.Scripting;

using TransportX.Domains.RoadTraffic.Network;

namespace TransportX.Domains.RoadTraffic.Scripting.Commands.Templates
{
    internal class SignalPropCollection : ITemplateComponent<Junction>
    {
        private readonly List<Item> Items = [];

        public SignalPropCollection()
        {
        }

        public void Add(TransformedModelTemplate prop, ISignalController controller, string groupKey, SignalLampRole role)
        {
            if ((prop as KinematicTransformedModelTemplate)?.CanMerge ?? false)
            {
                throw new ArgumentException("信号ストラクチャーは結合不可能である必要があります。", nameof(prop));
            }

            Item item = new(prop, controller, groupKey, role);
            Items.Add(item);
        }

        public void Build(Junction parent, IErrorCollector errorCollector)
        {
            List<SignalProp> props = Items.ConvertAll(item => item.Build());
            SignalPropCollectionComponent component = new(props);
            parent.Components.Add(component);
        }


        private class Item
        {
            private readonly TransformedModelTemplate Prop;
            private readonly ISignalController Controller;
            private readonly string GroupKey;
            private readonly SignalLampRole Role;

            private TransformedModel? BuiltModel = null;

            public Item(TransformedModelTemplate prop, ISignalController controller, string groupKey, SignalLampRole role)
            {
                Prop = prop;
                Controller = controller;
                GroupKey = groupKey;
                Role = role;

                Prop.Built += (sender, e) => BuiltModel = e.Result;
            }

            public SignalProp Build()
            {
                if (BuiltModel is null) throw new InvalidOperationException("ストラクチャーがビルドされていません。");

                SignalProp prop = new(BuiltModel, Controller, GroupKey, Role);
                return prop;
            }
        }
    }
}
