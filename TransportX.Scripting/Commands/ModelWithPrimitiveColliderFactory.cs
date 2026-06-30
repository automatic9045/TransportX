using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;

using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering;

namespace TransportX.Scripting.Commands
{
    public class ModelWithPrimitiveColliderFactory
    {
        private readonly Simulation Simulation;
        private readonly IErrorCollector ErrorCollector;

        private readonly ModelFactory ModelFactory;

        public ModelWithPrimitiveColliderFactory(Simulation simulation, IErrorCollector errorCollector, ModelFactory modelFactory)
        {
            Simulation = simulation;
            ErrorCollector = errorCollector;
            ModelFactory = modelFactory;
        }

        public Model Box(string? modelPath, bool makeLH, Box shape, ColliderMaterial material, Pose offset)
        {
            ColliderBase<Box> collider = ColliderFactory.Box(Simulation, shape, material, offset);

            if (modelPath is null)
            {
                return new CollidableModel(collider);
            }
            else
            {
                Model baseModel = ModelFactory.Load(modelPath, makeLH);
                return new CollidableModel(baseModel, collider);
            }
        }

        public Model Cylinder(string? modelPath, bool makeLH, Cylinder shape, ColliderMaterial material, Pose offset)
        {
            ColliderBase<Cylinder> collider = ColliderFactory.Cylinder(Simulation, shape, material, offset);

            if (modelPath is null)
            {
                return new CollidableModel(collider);
            }
            else
            {
                Model baseModel = ModelFactory.Load(modelPath, makeLH);
                return new CollidableModel(baseModel, collider);
            }
        }

        public Model Sphere(string? modelPath, bool makeLH, Sphere shape, ColliderMaterial material, Pose offset)
        {
            ColliderBase<Sphere> collider = ColliderFactory.Sphere(Simulation, shape, material, offset);

            if (modelPath is null)
            {
                return new CollidableModel(collider);
            }
            else
            {
                Model baseModel = ModelFactory.Load(modelPath, makeLH);
                return new CollidableModel(baseModel, collider);
            }
        }
    }
}
