using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Scripting.Commands
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
            if (!World.Models.TryGetValue(modelKey, out IModel? model))
            {
                ScriptError error = new(ErrorLevel.Error, $"モデル '{modelKey}' が見つかりません。");
                World.ErrorCollector.Report(error);

                model = Model.Empty();
            }

            LocatedModel locatedModel = new LocatedModel(model, Pose.Identity);
            World.BackgroundModels.Add(locatedModel);
        }
    }
}
