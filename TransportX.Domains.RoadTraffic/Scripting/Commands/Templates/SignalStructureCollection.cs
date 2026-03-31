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
    internal class SignalStructureCollection : ITemplateComponent<Junction>
    {
        private readonly List<Item> Items = [];

        public SignalStructureCollection()
        {
        }

        public void Add(LocatedModelTemplate structure, ISignalController controller, string groupKey, SignalLampRole role)
        {
            if ((structure as KinematicLocatedModelTemplate)?.CanMerge ?? false)
            {
                throw new ArgumentException("信号ストラクチャーは結合不可能である必要があります。", nameof(structure));
            }

            Item item = new(structure, controller, groupKey, role);
            Items.Add(item);
        }

        public void Build(Junction parent, IErrorCollector errorCollector)
        {
            List<SignalStructure> structures = Items.ConvertAll(item => item.Build());
            SignalStructureCollectionComponent component = new(structures);
            parent.Components.Add(component);
        }


        private class Item
        {
            private readonly LocatedModelTemplate Structure;
            private readonly ISignalController Controller;
            private readonly string GroupKey;
            private readonly SignalLampRole Role;

            private LocatedModel? BuiltModel = null;

            public Item(LocatedModelTemplate structure, ISignalController controller, string groupKey, SignalLampRole role)
            {
                Structure = structure;
                Controller = controller;
                GroupKey = groupKey;
                Role = role;

                Structure.Built += (sender, e) => BuiltModel = e.Result;
            }

            public SignalStructure Build()
            {
                if (BuiltModel is null) throw new InvalidOperationException("ストラクチャーがビルドされていません。");

                SignalStructure structure = new(BuiltModel, Controller, GroupKey, Role);
                return structure;
            }
        }
    }
}
