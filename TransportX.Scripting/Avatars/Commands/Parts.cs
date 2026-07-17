using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;
using TransportX.Rendering;
using TransportX.Scripting.Collections;
using TransportX.Spatial;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Parts
    {
        private readonly ScriptAvatar Avatar;

        private readonly ScriptKeyedList<string, Part> AllKey;
        public IReadOnlyScriptKeyedList<string, Part> All => AllKey;

        internal Parts(ScriptAvatar avatar)
        {
            Avatar = avatar;

            AllKey = new ScriptKeyedList<string, Part>(part => part.Key, Avatar.ErrorCollector, "パーツ", key => Part.Empty(Avatar, key));
        }

        public PartFactory Add(string key, string modelKey, Pose pose)
        {
            IModel model = Avatar.Models.GetModel(modelKey);
            PartFactory factory = new(Avatar, key, model, pose);
            return factory;
        }

        public PartFactory Add(string key, string modelKey, double x, double y, double z, double rotationX, double rotationY, double rotationZ)
        {
            SixDoF position = SixDoF.FromDegrees((float)x, (float)y, (float)z, (float)rotationX, (float)rotationY, (float)rotationZ);
            return Add(key, modelKey, position.ToPose());
        }

        public PartFactory Add(string key, string modelKey, double x, double y, double z)
        {
            return Add(key, modelKey, x, y, z, 0, 0, 0);
        }

        public Part Add(string key, TransformedModel model)
        {
            if (!Avatar.Structure.Contains(model))
            {
                ScriptError error = new(ErrorLevel.Error, $"アバターのボディストラクチャーに登録されていないモデルを指定することはできません。");
                Avatar.ErrorCollector.Report(error);
                return Part.Empty(Avatar, key);
            }

            Part part = AsPart(key, model);
            return part;
        }

        internal Part AsPart(string key, TransformedModel model)
        {
            Part part = model switch
            {
                KinematicTransformedModel kinematicModel => new KinematicPart(Avatar, key, kinematicModel),
                DynamicTransformedModel dynamicModel => new DynamicPart(Avatar, key, dynamicModel),
                _ => new Part(Avatar, key, model),
            };

            AllKey.Add(part);
            return part;
        }

        internal void RegisterComponents()
        {
            foreach (Part part in All)
            {
                part.RegisterComponents();
            }
        }
    }
}
