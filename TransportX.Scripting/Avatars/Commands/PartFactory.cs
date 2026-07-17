using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Scripting.Avatars.Commands
{
    public class PartFactory
    {
        private readonly ScriptAvatar Avatar;

        private readonly string Key;
        private readonly IModel Model;
        private readonly Pose Pose;

        private ColliderGroupHandle ColliderGroup;

        public IComponentCollection<ITemplateComponent<Part>> Components { get; } = new ComponentCollection<ITemplateComponent<Part>>();
        public IErrorCollector ErrorCollector => Avatar.ErrorCollector;

        internal PartFactory(ScriptAvatar avatar, string key, IModel model, Pose pose)
        {
            Avatar = avatar;

            Key = key;
            Model = model;
            Pose = pose;

            ColliderGroup = Avatar.Structure.DefaultGroup;
        }

        public PartFactory Group(string groupKey)
        {
            if (!Avatar.ColliderGroups.TryGetValue(groupKey, out ColliderGroup))
            {
                ColliderGroup = ColliderGroupHandle.NewGroup();
                Avatar.ColliderGroups.Add(groupKey, ColliderGroup);
            }

            return this;
        }

        public PartFactory GroupSkip()
        {
            return Group("__Skip");
        }

        public Part BuildNonCollision()
        {
            TransformedModel model = Avatar.Structure.Attach(Model, Pose);
            Part part = Build(model);
            return part;
        }

        public Part BuildAuto()
        {
            TransformedModel model = Avatar.Structure.AttachKinematicOrNonCollision(Model, ColliderGroup, Pose);
            Part part = Build(model);
            return part;
        }

        public KinematicPart BuildKinematic()
        {
            if (!IsCollidableOrReport(out ICollidableModel? collidable)) return KinematicPart.InvalidEmpty(Avatar, Key);

            KinematicTransformedModel model = Avatar.Structure.AttachKinematic(collidable, ColliderGroup, Pose);
            KinematicPart part = (KinematicPart)Build(model);
            return part;
        }

        public DynamicPart BuildDynamic(double mass)
        {
            if (!IsCollidableOrReport(out ICollidableModel? collidable)) return DynamicPart.InvalidEmpty(Avatar, Key);

            DynamicTransformedModel model = Avatar.Structure.AttachDynamic(collidable, (float)mass, ColliderGroup, Pose);
            DynamicPart part = (DynamicPart)Build(model);
            return part;
        }

        private bool IsCollidableOrReport([MaybeNullWhen(false)] out ICollidableModel collidable)
        {
            collidable = Model as ICollidableModel;

            if (collidable is null)
            {
                ScriptError error = new(ErrorLevel.Error, $"モデルに衝突判定が定義されていません。");
                Avatar.ErrorCollector.Report(error);
                return false;
            }
            else
            {
                return true;
            }
        }

        private Part Build(TransformedModel model)
        {
            Part part = Avatar.Commander.Structure.Parts.AsPart(Key, model);

            IErrorCollector componentErrorCollector = IErrorCollector.Default();
            componentErrorCollector.Reported += (sender, e) =>
            {
                ScriptError error = ScriptError.CreateFrom(e.Error);
                Avatar.ErrorCollector.Report(error);
            };
            foreach (ITemplateComponent<Part> component in Components.Values)
            {
                component.Build(part, componentErrorCollector);
            }

            return part;
        }
    }
}
