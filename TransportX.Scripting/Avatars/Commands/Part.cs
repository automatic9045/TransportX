using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Components;
using TransportX.Spatial;

namespace TransportX.Scripting.Avatars.Commands
{
    public class Part
    {
        private readonly ScriptAvatar Avatar;

        public string Key { get; }
        public TransformedModel Model { get; }

        public IComponentCollection<IComponent> Components { get; } = new ComponentCollection<IComponent>();

        internal Part(ScriptAvatar avatar, string key, TransformedModel model)
        {
            Avatar = avatar;

            Key = key;
            Model = model;
        }

        public static Part Empty(ScriptAvatar avatar, string key)
        {
            TransformedModel model = new(Rendering.Model.Empty(), Pose.Identity);
            return new Part(avatar, key, model);
        }

        internal void RegisterComponents()
        {
            Avatar.ComponentEngine.Register(Components);
        }
    }

    public class KinematicPart : Part
    {
        public new KinematicTransformedModel Model { get; }

        internal KinematicPart(ScriptAvatar avatar, string key, KinematicTransformedModel model) : base(avatar, key, model)
        {
            Model = model;
        }

        public static KinematicPart InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return new KinematicPart(avatar, key, null!);
        }
    }

    public class DynamicPart : Part
    {
        public new DynamicTransformedModel Model { get; }

        internal DynamicPart(ScriptAvatar avatar, string key, DynamicTransformedModel model) : base(avatar, key, model)
        {
            Model = model;
        }

        public static DynamicPart InvalidEmpty(ScriptAvatar avatar, string key)
        {
            return new DynamicPart(avatar, key, null!);
        }
    }
}
