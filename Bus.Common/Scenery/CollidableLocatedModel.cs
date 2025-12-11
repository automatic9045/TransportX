using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;

using Bus.Common.Physics;
using Bus.Common.Rendering;
using Vortice.Direct3D11;

namespace Bus.Common.Scenery
{
    public abstract class CollidableLocatedModel : LocatedModel, IDisposable
    {
        protected readonly Simulation Simulation;

        protected ID3D11DepthStencilState? NoDepthState = null;
        protected ID3D11RasterizerState? DebugRasterizerState = null;

        public new ICollidableModel Model { get; }

        /// <summary>
        /// 視点が位置するプレートから、このモデルが位置するプレートまでの距離を取得します。
        /// </summary>
        public PlateOffset FromCamera { get; private set; } = PlateOffset.Identity;

        public BodyHandle Handle { get; }
        public BodyReference Body => Simulation.Bodies[Handle];

        /// <summary>
        /// 物理モデルの姿勢を、LocatedModel 座標系に変換した形で取得・設定します。
        /// </summary>
        /// <remarks>
        /// LocatedModel 座標系はモデルが位置するプレートを基準とする一方、物理モデル座標系は視点が位置するプレートを基準とするため、
        /// <see cref="ICollider.Offset"/> と <see cref="FromCamera"/> プロパティの値を参照して変換されます。
        /// </remarks>
        protected Matrix4x4 ColliderTransform
        {
            get => Model.Collider.OffsetInverse * Body.Pose.ToMatrix4x4() * FromCamera.TransformInverse;
            set
            {
                Simulation.Awakener.AwakenBody(Handle);
                Body.Pose = (Model.Collider.Offset * value * FromCamera.Transform).ToRigidPose();
            }
        }

        public Matrix4x4 BaseToCollider => BaseTransformInverse * Model.Collider.OffsetInverse;
        public Matrix4x4 ColliderToBase => Model.Collider.Offset * BaseTransform;

        internal protected CollidableLocatedModel(Simulation simulation, ICollidableModel model, BodyHandle handle, Matrix4x4 transform)
            : base(model, transform, false)
        {
            Simulation = simulation;
            Model = model;
            Handle = handle;
        }

        public virtual void Dispose()
        {
            Simulation.Bodies.Remove(Handle);
            NoDepthState?.Dispose();
            DebugRasterizerState?.Dispose();
        }

        /// <summary>
        /// 視点が位置するプレートと、このモデルが位置するプレートの位置関係を設定します。
        /// </summary>
        /// <remarks>
        /// ここで設定した値は、LocatedModel 座標系と物理モデル座標系の差を計算するために使用されます。
        /// <paramref name="fromCamera"/> パラメータの値が <see cref="FromCamera"/> プロパティと異なっている場合は、同プロパティの値を更新します。
        /// 派生クラスによっては <see cref="ColliderTransform"/> プロパティの値もあわせて更新されます。
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
            base.Draw(context);
            if (!context.DrawColliderDebugModel || Model.Collider.DebugModel is null) return;

            if (NoDepthState is null)
            {
                DepthStencilDescription desc = new()
                {
                    DepthEnable = false,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthFunc = ComparisonFunction.Always,
                    StencilEnable = false,
                };
                NoDepthState = context.DeviceContext.Device.CreateDepthStencilState(desc);
            }

            if (DebugRasterizerState is null)
            {
                RasterizerDescription desc = new RasterizerDescription()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Wireframe,
                    DepthClipEnable = true,
                };
                DebugRasterizerState = context.DeviceContext.Device.CreateRasterizerState(desc);
            }

            context.DeviceContext.OMGetDepthStencilState(out ID3D11DepthStencilState? oldDState, out uint oldRef);
            context.DeviceContext.OMSetDepthStencilState(NoDepthState, 0);

            ID3D11RasterizerState? oldRSState = context.DeviceContext.RSGetState();
            context.DeviceContext.RSSetState(DebugRasterizerState);

            VertexConstantBuffer vertexBuffer = new()
            {
                World = Matrix4x4.Transpose(Body.Pose.ToMatrix4x4()),
                View = Matrix4x4.Transpose(context.View),
                Projection = Matrix4x4.Transpose(context.Projection),
            };
            context.DeviceContext.UpdateSubresource(vertexBuffer, context.VertexConstantBuffer);

            Model.Collider.DebugModel.Draw(new(context.DeviceContext, context.VertexConstantBuffer, context.PixelConstantBuffer));

            context.DeviceContext.OMSetDepthStencilState(oldDState, oldRef);
            oldDState?.Dispose();

            context.DeviceContext.RSSetState(oldRSState);
            oldRSState?.Dispose();
        }
    }
}
