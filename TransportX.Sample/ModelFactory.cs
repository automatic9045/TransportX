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

using TransportX;
using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Sample
{
    internal class ModelFactory
    {
        private static readonly string BaseDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"Models");


        private readonly ID3D11DeviceContext Context;
        private readonly Simulation Simulation;
        private readonly IErrorCollector ErrorCollector;
        private readonly Vector4 DebugModelColor;

        public ModelFactory(ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector, Vector4 debugModelColor)
        {
            Context = context;
            Simulation = simulation;
            ErrorCollector = errorCollector;
            DebugModelColor = debugModelColor;
        }

        private string GetAbsolutePath(string relativePath)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            return path;
        }

        public Model NonCollision(string visualPath, bool makeLH)
        {
            Model model = Model.Load(Context, ErrorCollector, GetAbsolutePath(visualPath), makeLH);
            model.DebugName = Path.GetFileNameWithoutExtension(visualPath);
            return model;
        }

        public CollidableModel WithCollisionModel(
            string visualPath, bool makeVisualLH, string collisionPath, bool makeCollisionLH, ColliderMaterial material, bool isOpen)
        {
            CollidableModel model = CollidableModel.Load(Context, Simulation, ErrorCollector,
                GetAbsolutePath(visualPath), makeVisualLH, GetAbsolutePath(collisionPath), makeCollisionLH, material, isOpen);
            model.CreateColliderDebugModel(Context.Device);
            model.ColliderDebugModel!.Color = DebugModelColor;
            model.DebugName = Path.GetFileNameWithoutExtension(visualPath);
            return model;
        }

        public CollidableModel WithBoundingBox(string path, bool makeLH, ColliderMaterial material)
        {
            CollidableModel model = CollidableModel.LoadWithBoundingBox(Context, Simulation, ErrorCollector, GetAbsolutePath(path), makeLH, material);
            model.CreateColliderDebugModel(Context.Device);
            model.ColliderDebugModel!.Color = DebugModelColor;
            model.DebugName = Path.GetFileNameWithoutExtension(path);
            return model;
        }

        public CollidableModel WithConvexHull(string path, bool makeLH, ColliderMaterial material)
        {
            CollidableModel model = CollidableModel.LoadWithConvexHull(Context, Simulation, ErrorCollector, GetAbsolutePath(path), makeLH, material);
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
