using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using Vortice.Direct3D11;

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
        private readonly Vector4 DebugModelColor;

        public ModelFactory(ID3D11Device device, ID3D11DeviceContext context, Simulation simulation, Vector4 debugModelColor)
        {
            Device = device;
            Context = context;
            Simulation = simulation;
            DebugModelColor = debugModelColor;
        }

        private string GetAbsolutePath(string relativePath)
        {
            string path = Path.Combine(BaseDirectory, relativePath);
            return path;
        }

        public Model NonCollision(string visualPath)
        {
            Model model = Model.Load(Device, Context, GetAbsolutePath(visualPath));
            return model;
        }

        public CollidableModel WithCollisionModel(string visualPath, string collisionPath, ColliderMaterial material, bool isOpen)
        {
            CollidableModel model = CollidableModel.Load(Device, Context, Simulation, GetAbsolutePath(visualPath), GetAbsolutePath(collisionPath), material, isOpen);
            model.Collider.CreateDebugModel(Device, DebugModelColor);
            return model;
        }

        public CollidableModel WithBoundingBox(string path, ColliderMaterial material)
        {
            CollidableModel model = CollidableModel.LoadWithBoundingBox(Device, Context, Simulation, GetAbsolutePath(path), material);
            model.Collider.CreateDebugModel(Device, DebugModelColor);
            return model;
        }

        public CollidableModel WithConvexHull(string path, ColliderMaterial material)
        {
            CollidableModel model = CollidableModel.LoadWithConvexHull(Device, Context, Simulation, GetAbsolutePath(path), material);
            model.Collider.CreateDebugModel(Device, DebugModelColor);
            return model;
        }
    }
}
