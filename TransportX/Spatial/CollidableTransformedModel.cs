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
    public abstract class CollidableTransformedModel : TransformedModel, IDisposable
    {
        public new ICollidableModel Model { get; }

        /// <summary>
        /// 視点が位置するプレートから、このモデルが位置するプレートまでの距離を取得します。
        /// </summary>
        public ChunkIndex FromCamera { get; private set; } = ChunkIndex.Zero;

        /// <summary>
        /// 物理モデルの姿勢を物理モデル座標系のままの形で取得・設定します。
        /// </summary>
        protected abstract Pose ColliderRawPose { get; set; }

        /// <summary>
        /// 物理モデルの姿勢を、<see cref="TransformedModel"/> 座標系に変換した形で取得・設定します。
        /// </summary>
        /// <remarks>
        /// <see cref="TransformedModel"/> 座標系はモデルが位置するプレートを基準とする一方、物理モデル座標系は視点が位置するプレートを基準とするため、
        /// <see cref="ICollider.Offset"/> と <see cref="FromCamera"/> プロパティの値を参照して変換されます。
        /// </remarks>
        protected virtual Pose ColliderPose
        {
            get => Model.Collider.OffsetInverse * ColliderRawPose * FromCamera.PoseInverse;
            set => ColliderRawPose = (Model.Collider.Offset * value * FromCamera.Pose).Validated();
        }

        public Pose BaseToCollider => BasePoseInverse * Model.Collider.OffsetInverse;
        public Pose ColliderToBase => Model.Collider.Offset * BasePose;

        protected CollidableTransformedModel(ICollidableModel model, Pose basePose)
            : base(model, basePose, false)
        {
            Model = model;
        }

        public abstract void Dispose();

        /// <summary>
        /// 視点が位置するプレートと、このモデルが位置するプレートの位置関係を設定します。
        /// </summary>
        /// <remarks>
        /// ここで設定した値は、<see cref="TransformedModel"/> 座標系と物理モデル座標系の差を計算するために使用されます。
        /// <paramref name="fromCamera"/> パラメータの値が <see cref="FromCamera"/> プロパティと異なっている場合は、同プロパティの値を更新します。
        /// 派生クラスによっては <see cref="ColliderPose"/> プロパティの値もあわせて更新されます。
        /// </remarks>
        /// <param name="fromCamera">視点が位置するプレートから、このモデルが位置するプレートまでの距離。</param>
        /// <returns><see cref="FromCamera"/> プロパティの値が更新されたかどうか。</returns>
        public virtual bool SetFromCamera(ChunkIndex fromCamera)
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

        public override void Draw(in TransformedDrawContext context)
        {
            switch (context.Layer)
            {
                case RenderLayer.Normal:
                    base.Draw(context);
                    break;

                case RenderLayer.Colliders:
                {
                    if (Model.ColliderDebugModel is null) return;

                    Matrix4x4 world = ColliderRawPose.ToMatrix4x4();
                    BoundingBox worldBox = BoundingBox.Transform(Model.ColliderDebugModel.BoundingBox, world);
                    if (context.Frustum.Contains(worldBox) == ContainmentType.Disjoint) return;

                    context.DrawModel(Model.ColliderDebugModel, world);
                    break;
                }
            }
        }
    }
}
