using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Scripting.Worlds.Commands
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
            IModel model = World.Models.GetModel(modelKey);
            TransformedModel transformedModel = new(model, Pose.Identity);
            World.BackgroundModels.Add(transformedModel);
        }
    }
}
