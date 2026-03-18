using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using Vortice.Mathematics;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public abstract class CollidableLocatedModel : LocatedModel, IDisposable
    {
        protected readonly IPhysicsHost PhysicsHost;
        protected readonly BodyDescription Description;

        private Pose FrozenPose = Pose.Identity;
        private BodyVelocity FrozenBodyVelocity = default;

        public new ICollidableModel Model { get; }

        /// <summary>
        /// 視点が位置するプレートから、このモデルが位置するプレートまでの距離を取得します。
        /// </summary>
        public PlateOffset FromCamera { get; private set; } = PlateOffset.Identity;

        public bool IsActive { get; private set; } = true;

        public BodyHandle Handle { get; }
        public BodyReference Body => PhysicsHost.Simulation.Bodies[Handle];

        public Vector3 Velocity => Pose.TransformNormal(Body.Velocity.Linear, Model.Collider.OffsetInverse);
        public Vector3 AngularVelocity => Pose.TransformNormal(Body.Velocity.Angular, Model.Collider.OffsetInverse);

        /// <summary>
        /// 物理モデルの姿勢を、LocatedModel 座標系に変換した形で取得・設定します。
        /// </summary>
        /// <remarks>
        /// LocatedModel 座標系はモデルが位置するプレートを基準とする一方、物理モデル座標系は視点が位置するプレートを基準とするため、
        /// <see cref="ICollider.Offset"/> と <see cref="FromCamera"/> プロパティの値を参照して変換されます。
        /// </remarks>
        protected Pose ColliderPose
        {
            get => Model.Collider.OffsetInverse * Body.Pose.ToPose() * FromCamera.PoseInverse;
            set
            {
                PhysicsHost.Simulation.Awakener.AwakenBody(Handle);
                Body.Pose = (Model.Collider.Offset * value * FromCamera.Pose).Validated().ToRigidPose();
            }
        }

        public Pose BaseToCollider => BasePoseInverse * Model.Collider.OffsetInverse;
        public Pose ColliderToBase => Model.Collider.Offset * BasePose;

        internal protected CollidableLocatedModel(IPhysicsHost physicsHost, ICollidableModel model, BodyDescription description, Pose basePose)
            : base(model, basePose, false)
        {
            PhysicsHost = physicsHost;
            Model = model;
            Description = description;

            Handle = PhysicsHost.Simulation.Bodies.Add(description);
            PhysicsHost.SetMaterial(Handle, Model.Collider.Material);
        }

        public virtual void Dispose()
        {
            PhysicsHost.Simulation.Bodies.Remove(Handle);
        }

        /// <summary>
        /// 視点が位置するプレートと、このモデルが位置するプレートの位置関係を設定します。
        /// </summary>
        /// <remarks>
        /// ここで設定した値は、LocatedModel 座標系と物理モデル座標系の差を計算するために使用されます。
        /// <paramref name="fromCamera"/> パラメータの値が <see cref="FromCamera"/> プロパティと異なっている場合は、同プロパティの値を更新します。
        /// 派生クラスによっては <see cref="ColliderPose"/> プロパティの値もあわせて更新されます。
        /// </remarks>
        /// <param name="fromCamera">視点が位置するプレートから、このモデルが位置するプレートまでの距離。</param>
        /// <returns><see cref="FromCamera"/> プロパティの値が更新されたかどうか。</returns>
        public virtual bool SetFromCamera(PlateOffset fromCamera)
        {
            if (fromCamera != FromCamera)
            {
                FromCamera = fromCamera;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Draw(in LocatedDrawContext context)
        {
            switch (context.Pass)
            {
                case RenderPass.Normal:
                    base.Draw(context);
                    break;

                case RenderPass.Colliders:
                {
                    if (Model.ColliderDebugModel is null) return;
                    if (!IsActive) return;

                    Matrix4x4 world = Body.Pose.ToPose().ToMatrix4x4();
                    BoundingBox worldBox = BoundingBox.Transform(Model.ColliderDebugModel.BoundingBox, world);
                    if (context.Frustum.Contains(worldBox) == ContainmentType.Disjoint) return;

                    InstanceData instanceData = new()
                    {
                        World = Matrix4x4.Transpose(world),
                    };
                    context.RenderQueue.Submit(context.Pass, Model.ColliderDebugModel, instanceData);
                    break;
                }
            }
        }

        public void Freeze()
        {
            if (!IsActive) return;

            FrozenPose = Pose;
            FrozenBodyVelocity = Body.Velocity;

            Body.Velocity = default;
            IsActive = false;
        }

        public void Unfreeze()
        {
            if (IsActive) return;

            IsActive = true;
            Pose = FrozenPose;
            Body.Velocity = FrozenBodyVelocity;
        }
    }
}
