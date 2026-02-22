using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using ColliderMesh = BepuPhysics.Collidables.Mesh;
using BepuUtilities.Memory;
using Vortice.Direct3D11;
using Vortice.Mathematics;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public class MergedKinematicLocatedModel : KinematicLocatedModel
    {
        protected readonly List<InstancedGroup> Groups;

        protected MergedKinematicLocatedModel(Simulation simulation, ICollidableModel physicsWrapper, BodyHandle handle, List<InstancedGroup> groups)
            : base(simulation, physicsWrapper, handle, Pose.Identity)
        {
            Groups = groups;
        }

        public static MergedKinematicLocatedModel Create(ID3D11Device device, IPhysicsHost physicsHost, IReadOnlyList<KinematicLocatedModelTemplate> sources)
        {
            if (sources.Count == 0) throw new ArgumentException("結合するモデルがありません。", nameof(sources));

            int totalTriangles = sources.Sum(m => m.Model.Collider is ColliderBase<ColliderMesh> meshCollider ? meshCollider.Shape.Triangles.Length : 0);
            physicsHost.Simulation.BufferPool.Take(totalTriangles, out Buffer<Triangle> combinedTriangles);

            int writeIndex = 0;
            for (int i = 0; i < sources.Count; i++)
            {
                KinematicLocatedModelTemplate source = sources[i];

                if (source.Model.Collider is ColliderBase<ColliderMesh> meshCollider)
                {
                    for (int j = 0; j < meshCollider.Shape.Triangles.Length; j++)
                    {
                        Triangle triangle = meshCollider.Shape.Triangles[j];
                        combinedTriangles[writeIndex] = new Triangle(
                            Pose.Transform(triangle.A, source.ColliderToBase),
                            Pose.Transform(triangle.B, source.ColliderToBase),
                            Pose.Transform(triangle.C, source.ColliderToBase)
                        );

                        writeIndex++;
                    }
                }
                else
                {
                    physicsHost.Simulation.BufferPool.Return(ref combinedTriangles);
                    throw new NotSupportedException("メッシュ以外のコライダーを結合することはできません。");
                }
            }

            ColliderMesh newMesh = new(combinedTriangles, Vector3.One, physicsHost.Simulation.BufferPool);
            Vector3 center = newMesh.ComputeOpenCenterOfMass();
            newMesh.Recenter(center);

            ColliderMaterial material = sources[0].Model.Collider.Material;
            ColliderBase<ColliderMesh> newCollider = ColliderFactory.Mesh(physicsHost.Simulation, newMesh, material, new Pose(center), true);
            CollidableModel physicsWrapper = new(newCollider)
            {
                DebugName = $"Merged{{{sources[0].Model.DebugName}, others: {sources.Count - 1}}}",
            };

            BodyDescription desc = BodyDescription.CreateKinematic(newCollider.Offset.ToRigidPose(), newCollider.ShapeIndex, 0.01f);
            BodyHandle handle = physicsHost.Simulation.Bodies.Add(desc);
            physicsHost.SetMaterial(handle, material);


            IEnumerable<IGrouping<IModel, KinematicLocatedModelTemplate>> groupedSources = sources.GroupBy(x => x.Model);
            List<InstancedGroup> groups = [];
            foreach (IGrouping<IModel, KinematicLocatedModelTemplate> group in groupedSources)
            {
                Pose[] poses = group.Select(x => x.Pose).ToArray();
                groups.Add(new InstancedGroup(device, group.Key, poses));
            }

            return new MergedKinematicLocatedModel(physicsHost.Simulation, physicsWrapper, handle, groups);
        }

        public override void Dispose()
        {
            base.Dispose();
            Model.Dispose();
            for (int i = 0; i < Groups.Count; i++) Groups[i].Dispose();
        }

        public override void Draw(in LocatedDrawContext context)
        {
            if (context.Pass == RenderPass.Normal)
            {
                Pose mergedPose = Pose * context.PlateOffset.Pose;

                for (int i = 0; i < Groups.Count; i++)
                {
                    InstancedGroup group = Groups[i];

                    int visibleCount = 0;
                    for (int j = 0; j < group.LocalPoses.Count; j++)
                    {
                        Matrix4x4 world = (group.LocalPoses[j] * mergedPose).ToMatrix4x4();
                        BoundingBox worldBox = BoundingBox.Transform(group.Model.BoundingBox, world);

                        if (context.Frustum.Contains(worldBox) != ContainmentType.Disjoint)
                        {
                            group.MappedData[visibleCount] = new InstanceData()
                            {
                                World = Matrix4x4.Transpose(world),
                            };
                            visibleCount++;
                        }
                    }

                    if (0 < visibleCount)
                    {
                        ReadOnlySpan<InstanceData> visibleInstances = new(group.MappedData, 0, visibleCount);
                        context.UpdateInstanceBuffer(group.InstanceBuffer, visibleInstances);

                        group.Model.Draw(new DrawContext()
                        {
                            DeviceContext = context.DeviceContext,
                            InstanceBuffer = group.InstanceBuffer,
                            InstanceCount = visibleCount,
                            MaterialBuffer = context.MaterialBuffer,
                        });
                    }
                }
            }

            base.Draw(context);
        }


        protected class InstancedGroup : IDisposable
        {
            public IModel Model { get; }
            public ID3D11Buffer InstanceBuffer { get; }
            public IReadOnlyList<Pose> LocalPoses { get; }
            public InstanceData[] MappedData { get; }

            public InstancedGroup(ID3D11Device device, IModel model, IReadOnlyList<Pose> localPoses)
            {
                Model = model;
                LocalPoses = localPoses;
                MappedData = new InstanceData[localPoses.Count];

                BufferDescription desc = new()
                {
                    Usage = ResourceUsage.Dynamic,
                    ByteWidth = (uint)(InstanceData.Size * localPoses.Count),
                    BindFlags = BindFlags.VertexBuffer,
                    CPUAccessFlags = CpuAccessFlags.Write,
                    MiscFlags = 0,
                };
                InstanceBuffer = device.CreateBuffer(desc);
            }

            public void Dispose()
            {
                InstanceBuffer.Dispose();
            }
        }
    }
}
