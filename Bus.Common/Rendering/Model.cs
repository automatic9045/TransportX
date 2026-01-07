using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using Vortice.Direct3D11;

using Bus.Common.Diagnostics;
using Bus.Common.Physics;

namespace Bus.Common.Rendering
{
    public class Model : IModel
    {
        public static Model Empty() => new Model([], []);


        public IEnumerable<Mesh> VisualMeshes { get; }
        public IEnumerable<ID3D11ShaderResourceView> Textures { get; }

        public Model(IEnumerable<Mesh> visualMeshes, IEnumerable<ID3D11ShaderResourceView> textures)
        {
            VisualMeshes = visualMeshes;
            Textures = textures;
        }

        public static Model Load(ID3D11Device device, ID3D11DeviceContext context, IErrorCollector errorCollector, string visualModelPath, bool makeLH)
        {
            using AssimpModelFactory factory = new(device, context, null, errorCollector);
            Model model = factory.Load(visualModelPath, makeLH);
            return model;
        }

        public virtual void Dispose()
        {
            foreach (ID3D11ShaderResourceView texture in Textures)
            {
                texture.Dispose();
            }

            foreach (Mesh mesh in VisualMeshes)
            {
                mesh.Dispose();
            }
        }

        public void Draw(DrawContext context)
        {
            foreach (Mesh mesh in VisualMeshes)
            {
                mesh.Draw(context);
            }
        }
    }


    public class CollidableModel : Model, ICollidableModel
    {
        public ICollider Collider { get; }

        public CollidableModel(IEnumerable<Mesh> visualMeshes, IEnumerable<ID3D11ShaderResourceView> textures, ICollider collider) : base(visualMeshes, textures)
        {
            Collider = collider;
        }

        public CollidableModel(Model baseModel, ICollider collider) : this(baseModel.VisualMeshes, baseModel.Textures, collider)
        {
        }

        public CollidableModel(ICollider collider) : this([], [], collider)
        {
        }

        public static CollidableModel Load(ID3D11Device device, ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector,
            string visualModelPath, bool makeVisualLH, string collisionModelPath, bool makeCollisionLH, ColliderMaterial material, bool isOpen)
        {
            using AssimpModelFactory factory = new(device, context, simulation, errorCollector);
            CollidableModel model = factory.LoadWithCollisionModel(visualModelPath, makeVisualLH, collisionModelPath, makeCollisionLH, material, isOpen);
            return model;
        }

        public static CollidableModel LoadWithBoundingBox(ID3D11Device device, ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector,
            string visualModelPath, bool makeLH, ColliderMaterial material)
        {
            using AssimpModelFactory factory = new(device, context, simulation, errorCollector);
            CollidableModel model = factory.LoadWithBoundingBox(visualModelPath, makeLH, material);
            return model;
        }

        public static CollidableModel LoadWithConvexHull(ID3D11Device device, ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector,
            string visualModelPath, bool makeLH, ColliderMaterial material)
        {
            using AssimpModelFactory factory = new(device, context, simulation, errorCollector);
            CollidableModel model = factory.LoadWithConvexHull(visualModelPath, makeLH, material);
            return model;
        }

        public override void Dispose()
        {
            base.Dispose();
            Collider.Dispose();
        }
    }
}
