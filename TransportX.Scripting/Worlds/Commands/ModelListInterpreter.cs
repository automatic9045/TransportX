using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics.Constraints;

using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering;

using TransportX.Scripting.Worlds.Commands.Functions;

namespace TransportX.Scripting.Worlds.Commands
{
    internal class ModelListInterpreter
    {
        private readonly Parser Parser;
        private readonly ModelFactory Factory;
        private readonly string BaseDirectory;
        private readonly IErrorCollector ErrorCollector;

        private bool MakeLH = true;
        private ModelLoader ModelLoadFunc;

        public ModelListInterpreter(Parser parser, ModelFactory factory, string baseDirectory, IErrorCollector errorCollector)
        {
            Parser = parser;
            Factory = factory;
            BaseDirectory = baseDirectory;
            ErrorCollector = errorCollector;

            ModelLoadFunc = modelPath => Factory.Load(modelPath, MakeLH);
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
                ModelLoadFunc = modelPath => Factory.Load(modelPath, MakeLH);
            }
            else if (function.Signature == ModelListSignatures.BoundingBox1)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelLoadFunc = modelPath => Factory.LoadWithBoundingBox(modelPath, MakeLH, material);
            }
            else if (function.Signature == ModelListSignatures.BoundingBox2)
            {
                ModelLoadFunc = modelPath => Factory.LoadWithBoundingBox(modelPath, MakeLH, ColliderMaterial.Default);
            }
            else if (function.Signature == ModelListSignatures.ConvexHull1)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelLoadFunc = modelPath => Factory.LoadWithConvexHull(modelPath, MakeLH, material);
            }
            else if (function.Signature == ModelListSignatures.ConvexHull2)
            {
                ModelLoadFunc = modelPath => Factory.LoadWithConvexHull(modelPath, MakeLH, ColliderMaterial.Default);
            }
            else if (function.Signature == ModelListSignatures.ClosedModel1)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);
                ColliderMaterial material = CreateMaterial(1);

                ModelLoadFunc = modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, collisionModelPath, MakeLH, material, false);
            }
            else if (function.Signature == ModelListSignatures.ClosedModel2)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);

                ModelLoadFunc = modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, collisionModelPath, MakeLH, ColliderMaterial.Default, false);
            }
            else if (function.Signature == ModelListSignatures.ClosedModel3)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelLoadFunc = modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, modelPath, MakeLH, material, false);
            }
            else if (function.Signature == ModelListSignatures.ClosedModel4)
            {
                ModelLoadFunc = modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, modelPath, MakeLH, ColliderMaterial.Default, false);
            }
            else if (function.Signature == ModelListSignatures.OpenModel1)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);
                ColliderMaterial material = CreateMaterial(1);

                ModelLoadFunc = modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, collisionModelPath, MakeLH, material, true);
            }
            else if (function.Signature == ModelListSignatures.OpenModel2)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);

                ModelLoadFunc = modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, collisionModelPath, MakeLH, ColliderMaterial.Default, true);
            }
            else if (function.Signature == ModelListSignatures.OpenModel3)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelLoadFunc = modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, modelPath, MakeLH, material, true);
            }
            else if (function.Signature == ModelListSignatures.OpenModel4)
            {
                ModelLoadFunc = modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, modelPath, MakeLH, ColliderMaterial.Default, true);
            }
            else
            {
                Error error = new(ErrorLevel.Error, $"コマンド '${function.Signature}' は存在しません。", null);
                ErrorCollector.Report(error);
            }


            ColliderMaterial CreateMaterial(int argBeginIndex)
            {
                return new ColliderMaterial(
                    (float)function.Args[argBeginIndex], (float)function.Args[argBeginIndex + 1],
                    new SpringSettings((float)function.Args[argBeginIndex + 2], (float)function.Args[argBeginIndex + 3]));
            }
        }

        public Model Build(string modelPath)
        {
            Model model = ModelLoadFunc(modelPath);
            return model;
        }


        private delegate Model ModelLoader(string modelPath);
    }
}
