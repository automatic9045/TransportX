using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Vortice.Direct3D11;

using Bus.Common.Physics;
using Bus.Common.Scenery;
using Bus.Common.Scenery.Networks;

namespace Bus.Common.Extensions.Networks.Elements
{
    public abstract class SplineBase : NetworkEdge
    {
        protected readonly ID3D11Device Device;
        protected readonly IPhysicsHost PhysicsHost;

        private readonly List<LocatedModel> ModelsKey = [];
        public override IReadOnlyList<LocatedModel> Models => ModelsKey;

        public abstract float Length { get; }

        public SplineBase(ID3D11Device device, IPhysicsHost physicsHost, int plateX, int plateZ, Pose pose)
            : base(plateX, plateZ, pose)
        {
            Device = device;
            PhysicsHost = physicsHost;
        }

        public abstract Vector3 GetPoint(float s);
        public abstract Vector3 GetUp(float s);
        public abstract Pose GetPose(float s);

        public void AddStructure(SplineStructure structure)
        {
            List<KinematicLocatedModelTemplate> modelsToMerge = [];
            for (int i = 0; i < structure.Count; i++)
            {
                float s = structure.From + structure.Interval * i;
                if (Length < s) break;

                LocatedModelTemplate source = structure.Models[i % structure.Models.Count];
                Pose curvePose = GetSpanPose(s, structure.Span);
                Pose pose = source.Pose * curvePose * Pose;

                LocatedModelTemplate compiled = KinematicLocatedModelTemplate.CreateKinematicOrNonCollision(PhysicsHost, source.Model, pose);
                if (compiled is KinematicLocatedModelTemplate compiledKinematic)
                {
                    modelsToMerge.Add(compiledKinematic);
                }
                else
                {
                    LocatedModel model = compiled.Build();
                    ModelsKey.Add(model);
                }
            }

            if (0 < modelsToMerge.Count)
            {
                MergedKinematicLocatedModel mergedModel = MergedKinematicLocatedModel.Create(PhysicsHost, modelsToMerge);
                mergedModel.Model.Collider.CreateDebugModel(Device);
                mergedModel.Model.Collider.DebugModelColor = new Vector4(0, 0, 1, 1);
                ModelsKey.Add(mergedModel);
            }
        }

        public void AddStructures(IEnumerable<SplineStructure> structures)
        {
            foreach (SplineStructure structure in structures)
            {
                AddStructure(structure);
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
