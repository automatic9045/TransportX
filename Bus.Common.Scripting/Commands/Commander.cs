using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;

namespace Bus.Common.Scripting.Commands
{
    public class Commander
    {
        public ScriptWorld World { get; }

        public Background Background { get; }
        public Debug Debug { get; }
        public Models Models { get; }
        public Plates Plates { get; }
        public Splines Splines { get; }
        public Triggers Triggers { get; }
        public Vehicles Vehicles { get; }

        internal Commander(ScriptWorld world)
        {
            World = world;

            Background = new Background(world);
            Debug = new Debug(world);
            Models = new Models(world);
            Plates = new Plates(world);
            Splines = new Splines(world);
            Triggers = new Triggers(world);
            Vehicles = new Vehicles(world);
        }

        private protected Commander(Commander parent)
        {
            World = parent.World;

            Background = parent.Background;
            Debug = parent.Debug;
            Models = parent.Models;
            Plates = parent.Plates;
            Splines = parent.Splines;
            Triggers = parent.Triggers;
            Vehicles = parent.Vehicles;
        }

        internal void Dispose()
        {
            Triggers.Dispose();
        }
    }
}
