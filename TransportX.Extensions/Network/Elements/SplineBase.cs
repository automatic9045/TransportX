using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using TransportX.Network;
using TransportX.Physics;
using TransportX.Spatial;

namespace TransportX.Extensions.Network.Elements
{
    public abstract class SplineBase : NetworkEdge
    {
        private static readonly IReadOnlyList<Vector4> DebugColors = [new(0, 0, 1, 1), new(0, 0.75f, 1, 1), new(0, 0.375f, 1, 1), new(0, 0, 0.625f, 1)];
        private static int DebugColorIndex = 0;


        private readonly List<LocatedModel> ModelsKey = [];
        public override IReadOnlyList<LocatedModel> Models => ModelsKey;

        public abstract float Length { get; }

        public SplineBase(int plateX, int plateZ, Pose pose) : base(plateX, plateZ, pose)
        {
        }

        public abstract Vector3 GetPoint(float s);
        public abstract Vector3 GetUp(float s);
        public abstract Pose GetPose(float s);

        public void PutStructures(ID3D11Device device, IPhysicsHost physicsHost, IEnumerable<SplineStructure> structures)
        {
            List<KinematicLocatedModelTemplate> modelsToMerge = [];
            foreach (SplineStructure structure in structures)
            {
                for (int i = 0; i < structure.Count; i++)
                {
                    float s = structure.From + structure.Interval * i;
                    if (Length < s) break;

                    LocatedModelTemplate template = structure.Models[i % structure.Models.Count];
                    Pose curvePose = GetSpanPose(s, structure.Span);
                    Pose pose = template.Pose * curvePose * Pose;

                    LocatedModelTemplate compiled = KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(physicsHost, template.Model, pose);
                    if (compiled is KinematicLocatedModelTemplate compiledKinematic && compiledKinematic.CanMerge)
                    {
                        modelsToMerge.Add(compiledKinematic);
                    }
                    else
                    {
                        LocatedModel model = compiled.Build();
                        ModelsKey.Add(model);
                    }
                }
            }

            if (0 < modelsToMerge.Count)
            {
                MergedKinematicLocatedModel mergedModel = MergedKinematicLocatedModel.Create(physicsHost, modelsToMerge);
                mergedModel.Model.CreateColliderDebugModel(device);
                mergedModel.Model.ColliderDebugModel!.Color = DebugColors[DebugColorIndex];
                ModelsKey.Add(mergedModel);

                DebugColorIndex++;
                if (DebugColorIndex == DebugColors.Count) DebugColorIndex = 0;
            }
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
