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

using Bus.Common;
using Bus.Common.Diagnostics;
using Bus.Common.Physics;
using Bus.Common.Rendering;

namespace Bus.Sample
{
    internal class ModelFactory
    {
        private static readonly string BaseDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"Models");


        private readonly ID3D11Device Device;
        private readonly ID3D11DeviceContext Context;
        private readonly Simulation Simulation;
        private readonly IErrorCollector ErrorCollector;
        private readonly Vector4 DebugModelColor;

        public ModelFactory(ID3D11Device device, ID3D11DeviceContext context, Simulation simulation, IErrorCollector errorCollector, Vector4 debugModelColor)
        {
            Device = device;
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
            Model model = Model.Load(Device, Context, ErrorCollector, GetAbsolutePath(visualPath), makeLH);
            model.DebugName = Path.GetFileNameWithoutExtension(visualPath);
            return model;
        }

        public CollidableModel WithCollisionModel(
            string visualPath, bool makeVisualLH, string collisionPath, bool makeCollisionLH, ColliderMaterial material, bool isOpen)
        {
            CollidableModel model = CollidableModel.Load(Device, Context, Simulation, ErrorCollector,
                GetAbsolutePath(visualPath), makeVisualLH, GetAbsolutePath(collisionPath), makeCollisionLH, material, isOpen);
            model.Collider.CreateDebugResources(Device);
            model.Collider.DebugColor = DebugModelColor;
            model.DebugName = Path.GetFileNameWithoutExtension(visualPath);
            return model;
        }

        public CollidableModel WithBoundingBox(string path, bool makeLH, ColliderMaterial material)
        {
            CollidableModel model = CollidableModel.LoadWithBoundingBox(Device, Context, Simulation, ErrorCollector, GetAbsolutePath(path), makeLH, material);
            model.Collider.CreateDebugResources(Device);
            model.Collider.DebugColor = DebugModelColor;
            model.DebugName = Path.GetFileNameWithoutExtension(path);
            return model;
        }

        public CollidableModel WithConvexHull(string path, bool makeLH, ColliderMaterial material)
        {
            CollidableModel model = CollidableModel.LoadWithConvexHull(Device, Context, Simulation, ErrorCollector, GetAbsolutePath(path), makeLH, material);
            model.Collider.CreateDebugResources(Device);
            model.Collider.DebugColor = DebugModelColor;
            model.DebugName = Path.GetFileNameWithoutExtension(path);
            return model;
        }

        public CollidableModel WithCylinder(Model baseModel, Cylinder shape, ColliderMaterial material, Pose offset)
        {
            Collider<Cylinder> collider = ColliderFactory.Cylinder(Simulation, shape, material, offset);
            collider.CreateDebugResources(Device);
            collider.DebugColor = DebugModelColor;

            CollidableModel model = new(baseModel, collider);
            return model;
        }
    }
}
