using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Scenery;

namespace Bus.Common.Scripting.Commands
{
    public class Background
    {
        private readonly ScriptWorld World;

        internal Background(ScriptWorld world)
        {
            World = world;
        }

        public void Add(string modelKey)
        {
            LocatedModel model = new LocatedModel(World.Models[modelKey], Matrix4x4.Identity);
            World.BackgroundModels.Add(model);
        }
    }
}
