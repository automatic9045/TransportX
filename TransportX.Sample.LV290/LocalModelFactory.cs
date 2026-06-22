using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using Vortice.Direct3D11;

using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Sample.LV290
{
    internal class LocalModelFactory : IDisposable
    {
        private static readonly string BaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;


        private readonly ID3D11DeviceContext Context;
        private readonly Simulation Simulation;
        private readonly ModelFactory Factory;
        private readonly Vector4 DebugModelColor;

        public IReadOnlyCollection<ID3D11ShaderResourceView> Textures => Factory.Textures;

        public LocalModelFactory(ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector, Vector4 debugModelColor)
        {
            Context = context;
            Simulation = simulation;
            Factory = new ModelFactory(Context, simulation, errorCollector);
            DebugModelColor = debugModelColor;
        }

        public void Dispose()
        {
            Factory.Dispose();
        }

        private static string GetAbsolutePath(string relativePath)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            return path;
        }

        public Model NonCollision(string visualPath, bool makeLH)
        {
            Model model = Factory.Load(GetAbsolutePath(visualPath), makeLH);
            model.DebugName = Path.GetFileNameWithoutExtension(visualPath);
            return model;
        }

        public CollidableModel WithCollisionModel(
            string visualPath, bool makeVisualLH, string collisionPath, bool makeCollisionLH, ColliderMaterial material, bool isOpen)
        {
            CollidableModel model = Factory.LoadWithCollisionModel(
                GetAbsolutePath(visualPath), makeVisualLH, GetAbsolutePath(collisionPath), makeCollisionLH, material, isOpen);
            model.CreateColliderDebugModel(Context.Device);
            model.ColliderDebugModel!.Color = DebugModelColor;
            model.DebugName = Path.GetFileNameWithoutExtension(visualPath);
            return model;
        }

        public CollidableModel WithBoundingBox(string path, bool makeLH, ColliderMaterial material)
        {
            CollidableModel model = Factory.LoadWithBoundingBox(GetAbsolutePath(path), makeLH, material);
            model.CreateColliderDebugModel(Context.Device);
            model.ColliderDebugModel!.Color = DebugModelColor;
            model.DebugName = Path.GetFileNameWithoutExtension(path);
            return model;
        }

        public CollidableModel WithConvexHull(string path, bool makeLH, ColliderMaterial material)
        {
            CollidableModel model = Factory.LoadWithConvexHull(GetAbsolutePath(path), makeLH, material);
            model.CreateColliderDebugModel(Context.Device);
            model.ColliderDebugModel!.Color = DebugModelColor;
            model.DebugName = Path.GetFileNameWithoutExtension(path);
            return model;
        }

        public CollidableModel WithCylinder(Model baseModel, Cylinder shape, ColliderMaterial material, Pose offset)
        {
            ColliderBase<Cylinder> collider = ColliderFactory.Cylinder(Simulation, shape, material, offset);
            CollidableModel model = new(baseModel, collider);
            model.CreateColliderDebugModel(Context.Device);
            model.ColliderDebugModel!.Color = DebugModelColor;
            return model;
        }
    }
}
