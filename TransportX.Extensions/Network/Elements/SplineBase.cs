using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Mathematics;
using TransportX.Network;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Spatial;

namespace TransportX.Extensions.Network.Elements
{
    public abstract class SplineBase : NetworkEdge, IPoseCurve
    {
        private static readonly IReadOnlyList<Vector4> DebugColors = [new(0, 0, 1, 1), new(0, 0.75f, 1, 1), new(0, 0.375f, 1, 1), new(0, 0, 0.625f, 1)];
        private static int DebugColorIndex = 0;


        private readonly List<TransformedModel> ModelsKey = [];
        public override IReadOnlyList<TransformedModel> Models => ModelsKey;

        public abstract float Length { get; }

        public SplineBase(WorldPose worldPose) : base(worldPose)
        {
        }

        public abstract Pose GetPose(float s);

        public void PutProps(ID3D11Device device, IPhysicsHost physicsHost, IEnumerable<SplineProp> props)
        {
            Dictionary<ColliderMaterial, List<StaticTransformedModelTemplate>> modelsToMerge = [];
            foreach (SplineProp prop in props)
            {
                IEnumerable<TransformedModelTemplate> templates = prop.Build(physicsHost, this);
                foreach (TransformedModelTemplate template in templates)
                {
                    if (template is StaticTransformedModelTemplate staticTemplate && staticTemplate.CanMerge)
                    {
                        if (!modelsToMerge.TryGetValue(staticTemplate.Collider.Material, out List<StaticTransformedModelTemplate>? models))
                        {
                            models = [];
                            modelsToMerge.Add(staticTemplate.Collider.Material, models);
                        }

                        StaticTransformedModelTemplate compiled = new(physicsHost, staticTemplate.Resource, staticTemplate.Pose * WorldPose.Pose);
                        models.Add(compiled);
                    }
                    else
                    {
                        TransformedModel model = template.Build(pose => pose * WorldPose.Pose);
                        ModelsKey.Add(model);
                    }
                }
            }

            foreach (List<StaticTransformedModelTemplate> models in modelsToMerge.Values)
            {
                MergedStaticTransformedModel mergedModel = MergedStaticTransformedModel.Create(physicsHost, models);

                mergedModel.Collider.CreateDebugModel(device);
                mergedModel.Collider.DebugModel.Color = DebugColors[DebugColorIndex++];
                if (DebugColorIndex == DebugColors.Count) DebugColorIndex = 0;

                ModelsKey.Add(mergedModel);
            }
        }

        public void PutExtrusion(ID3D11Device device, IPhysicsHost physicsHost, SplineExtrusion extrusion, float sOffset)
        {
            TransformedModel? model = extrusion.Build(device, physicsHost, this, WorldPose.Pose, sOffset);
            if (model is null) return;

            if (model is CollidableTransformedModel collidable)
            {
                collidable.Collider.CreateDebugModel(device);
                collidable.Collider.DebugModel.Color = DebugColors[DebugColorIndex++];
                if (DebugColorIndex == DebugColors.Count) DebugColorIndex = 0;
            }

            ModelsKey.Add(model);
        }
    }
}
