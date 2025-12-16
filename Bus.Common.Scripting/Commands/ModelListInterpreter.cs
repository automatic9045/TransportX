using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using Bus.Common.Physics;
using Bus.Common.Rendering;

using Bus.Common.Scripting.Commands.Functions;

namespace Bus.Common.Scripting.Commands
{
    internal class ModelListInterpreter
    {
        private readonly ScriptWorld World;
        private readonly Parser Parser;
        private readonly string BaseDirectory;
        private readonly string Key;
        private readonly string ModelPath;

        private bool MakeLH = true;
        private Func<Model> ModelFactory;

        public ModelListInterpreter(ScriptWorld world, Parser parser, string baseDirectory, string key, string modelPath)
        {
            World = world;
            Parser = parser;
            BaseDirectory = baseDirectory;
            Key = key;
            ModelPath = modelPath;

            ModelFactory = () => Model.Load(World.DXHost.Device, World.DXHost.Context, ModelPath, MakeLH);
        }

        public void ReadCommand(string commandText)
        {
            Function function = Parser.Parse(commandText);
            if (function.Signature == ModelListSignatures.LeftHanded)
            {
                MakeLH = false;
            }
            else if (function.Signature == ModelListSignatures.NonCollision)
            {
                ModelFactory = () => Model.Load(World.DXHost.Device, World.DXHost.Context, ModelPath, MakeLH);
            }
            else if (function.Signature == ModelListSignatures.BoundingBox1)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelFactory = () => CollidableModel.LoadWithBoundingBox(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation, ModelPath, MakeLH, material);
            }
            else if (function.Signature == ModelListSignatures.BoundingBox2)
            {
                ModelFactory = () => CollidableModel.LoadWithBoundingBox(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation, ModelPath, MakeLH, ColliderMaterial.Default);
            }
            else if (function.Signature == ModelListSignatures.ClosedModel1)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);
                ColliderMaterial material = CreateMaterial(1);

                ModelFactory = () => CollidableModel.Load(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                    ModelPath, MakeLH, collisionModelPath, MakeLH, material, false);
            }
            else if (function.Signature == ModelListSignatures.ClosedModel2)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);

                ModelFactory = () => CollidableModel.Load(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                    ModelPath, MakeLH, collisionModelPath, MakeLH, ColliderMaterial.Default, false);
            }
            else if (function.Signature == ModelListSignatures.ClosedModel3)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelFactory = () => CollidableModel.Load(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                    ModelPath, MakeLH, ModelPath, MakeLH, material, false);
            }
            else if (function.Signature == ModelListSignatures.ClosedModel4)
            {
                ModelFactory = () => CollidableModel.Load(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                    ModelPath, MakeLH, ModelPath, MakeLH, ColliderMaterial.Default, false);
            }
            else if (function.Signature == ModelListSignatures.OpenModel1)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);
                ColliderMaterial material = CreateMaterial(1);

                ModelFactory = () => CollidableModel.Load(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                    ModelPath, MakeLH, collisionModelPath, MakeLH, material, true);
            }
            else if (function.Signature == ModelListSignatures.OpenModel2)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);

                ModelFactory = () => CollidableModel.Load(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                    ModelPath, MakeLH, collisionModelPath, MakeLH, ColliderMaterial.Default, true);
            }
            else if (function.Signature == ModelListSignatures.OpenModel3)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelFactory = () => CollidableModel.Load(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                    ModelPath, MakeLH, ModelPath, MakeLH, material, true);
            }
            else if (function.Signature == ModelListSignatures.OpenModel4)
            {
                ModelFactory = () => CollidableModel.Load(
                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                    ModelPath, MakeLH, ModelPath, MakeLH, ColliderMaterial.Default, true);
            }
            else
            {
                throw new FormatException($"コマンド '${commandText}' は無効です。");
            }


            ColliderMaterial CreateMaterial(int argBeginIndex)
            {
                return new ColliderMaterial(
                    (float)function.Args[argBeginIndex], (float)function.Args[argBeginIndex + 1],
                    new SpringSettings((float)function.Args[argBeginIndex + 2], (float)function.Args[argBeginIndex + 3]));
            }
        }

        public Model Build()
        {
            Model model = ModelFactory();
            return model;
        }
    }
}
