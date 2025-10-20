using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using Vortice.Direct3D11;

using Bus.Common.Physics;

namespace Bus.Common.Rendering
{
    public class Model : IModel
    {
        public static readonly Model Empty = new Model([], []);


        public IEnumerable<Mesh> VisualMeshes { get; }
        public IEnumerable<ID3D11ShaderResourceView> Textures { get; }

        public Model(IEnumerable<Mesh> visualMeshes, IEnumerable<ID3D11ShaderResourceView> textures)
        {
            VisualMeshes = visualMeshes;
            Textures = textures;
        }

        public static Model Load(ID3D11Device device, ID3D11DeviceContext context, string visualModelPath)
        {
            AssimpModelFactory factory = new AssimpModelFactory(device, context);
            Model model = factory.Load(visualModelPath);
            return model;
        }

        public void Dispose()
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

        public void Draw(ID3D11DeviceContext context)
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

        public static CollidableModel Load(ID3D11Device device, ID3D11DeviceContext context, Simulation simulation,
            string visualModelPath, string collisionModelPath, bool isOpen)
        {
            AssimpModelFactory factory = new AssimpModelFactory(device, context, simulation);
            CollidableModel model = factory.LoadWithCollisionModel(visualModelPath, collisionModelPath, isOpen);
            return model;
        }

        public static CollidableModel LoadWithBoundingBox(ID3D11Device device, ID3D11DeviceContext context, Simulation simulation, string visualModelPath)
        {
            AssimpModelFactory factory = new AssimpModelFactory(device, context, simulation);
            CollidableModel model = factory.LoadWithBoundingBox(visualModelPath);
            return model;
        }

        public static CollidableModel LoadWithConvexHull(ID3D11Device device, ID3D11DeviceContext context, Simulation simulation, string visualModelPath)
        {
            AssimpModelFactory factory = new AssimpModelFactory(device, context, simulation);
            CollidableModel model = factory.LoadWithConvexHull(visualModelPath);
            return model;
        }
    }
}
