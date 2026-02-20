using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Spatial
{
    public abstract class CollidableLocatedModel : LocatedModel, IDisposable
    {
        protected readonly Simulation Simulation;

        public new ICollidableModel Model { get; }

        /// <summary>
        /// 視点が位置するプレートから、このモデルが位置するプレートまでの距離を取得します。
        /// </summary>
        public PlateOffset FromCamera { get; private set; } = PlateOffset.Identity;

        public BodyHandle Handle { get; }
        public BodyReference Body => Simulation.Bodies[Handle];

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
                Simulation.Awakener.AwakenBody(Handle);
                Body.Pose = (Model.Collider.Offset * value * FromCamera.Pose).ToRigidPose();
            }
        }

        public Pose BaseToCollider => BasePoseInverse * Model.Collider.OffsetInverse;
        public Pose ColliderToBase => Model.Collider.Offset * BasePose;

        internal protected CollidableLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, Pose basePose)
            : base(model, basePose, false)
        {
            Simulation = simulation;
            Model = model;
            Handle = handle;
        }

        public virtual void Dispose()
        {
            Simulation.Bodies.Remove(Handle);
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

        public override void Draw(LocatedDrawContext context)
        {
            switch (context.Pass)
            {
                case RenderPass.Normal:
                    base.Draw(context);
                    break;

                case RenderPass.Colliders:
                {
                    if (Model.ColliderDebugModel is null) return;

                    TransformConstants transformConstants = new()
                    {
                        World = Matrix4x4.Transpose(Body.Pose.ToPose().ToMatrix4x4()),
                        View = Matrix4x4.Transpose(context.View),
                        Projection = Matrix4x4.Transpose(context.Projection),
                    };
                    context.DeviceContext.UpdateSubresource(transformConstants, context.TransformBuffer);

                    Model.ColliderDebugModel.Draw(new(context.DeviceContext, context.TransformBuffer, context.MaterialBuffer));
                    break;
                }
            }
        }
    }
}
