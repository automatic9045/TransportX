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

        public abstract Vector3 GetPoint(float s);
        public abstract Vector3 GetUp(float s);
        public abstract Pose GetPose(float s);

        public void PutProps(ID3D11Device device, IPhysicsHost physicsHost, IEnumerable<SplineProp> props)
        {
            Dictionary<ColliderMaterial, List<StaticTransformedModelTemplate>> modelsToMerge = [];
            foreach (SplineProp prop in props)
            {
                for (int i = 0; i < prop.Count; i++)
                {
                    float s = prop.From + prop.Interval * i;
                    if (Length < s) break;

                    TransformedModelTemplate template = prop.Models[i % prop.Models.Count];
                    Pose curvePose = GetSpanPose(s, prop.Span);

                    if (template is StaticTransformedModelTemplate staticTemplate && staticTemplate.CanMerge)
                    {
                        if (!modelsToMerge.TryGetValue(staticTemplate.Collider.Material, out List<StaticTransformedModelTemplate>? models))
                        {
                            models = [];
                            modelsToMerge.Add(staticTemplate.Collider.Material, models);
                        }

                        StaticTransformedModelTemplate compiled = new(physicsHost, staticTemplate.Resource, template.Pose * curvePose * WorldPose.Pose);
                        models.Add(compiled);
                    }
                    else
                    {
                        TransformedModel model = template.Build(pose => pose * curvePose * WorldPose.Pose);
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
            if (Length < 1e-3f) return;

            IReadOnlyList<float> sSamples = extrusion.Sampler.Sample(this, sOffset);
            if (sSamples.Count < 2) return;

            List<(Pose Pose, float S)> samples = new(sSamples.Count);
            for (int i = 0; i < sSamples.Count; i++)
            {
                float localS = float.Clamp(sSamples[i], 0, Length);
                samples.Add((GetPose(localS), localS + sOffset));
            }

            ModelResourceSet resource = extrusion.CrossSection.Build(
                device, physicsHost.Simulation, samples, extrusion.Offset, extrusion.VIntervalScale, extrusion.VOffset);
            TransformedModel model = StaticTransformedModel.CreateStaticOrNonCollision(physicsHost, resource, WorldPose.Pose);

            if (model is CollidableTransformedModel collidable)
            {
                collidable.Collider.CreateDebugModel(device);
                collidable.Collider.DebugModel.Color = DebugColors[DebugColorIndex++];
                if (DebugColorIndex == DebugColors.Count) DebugColorIndex = 0;
            }

            ModelsKey.Add(model);
        }

        protected Pose GetSpanPose(float s, float span)
        {
            Vector3 front = GetPoint(s + span);
            Vector3 back = GetPoint(s);
            Vector3 forward = front - back;
            if (forward.LengthSquared() < 1e-6f) return GetPose(s);

            Vector3 upFront = GetUp(s + span);
            Vector3 upBack = GetUp(s);
            Vector3 up = Vector3.Normalize(Vector3.Lerp(upFront, upBack, 0.5f));

            Vector3 tangent = Vector3.Normalize(forward);
            return Pose.CreateWorldLH(back, tangent, up);
        }
    }
}
