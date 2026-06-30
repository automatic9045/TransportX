using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;

using TransportX.Diagnostics;
using TransportX.Physics;
using TransportX.Rendering;

using TransportX.Scripting.Commands.Functions;

namespace TransportX.Scripting.Commands
{
    internal class ModelListInterpreter
    {
        private readonly Parser Parser;

        private readonly ModelFactory Factory;
        private readonly ModelWithPrimitiveColliderFactory PrimitiveFactory;

        private readonly string BaseDirectory;
        private readonly IErrorCollector ErrorCollector;

        private bool MakeLH = true;
        private ModelLoader ModelLoadFunc;

        public ModelListInterpreter(Parser parser, Simulation simulation, ModelFactory factory, string baseDirectory, IErrorCollector errorCollector)
        {
            Parser = parser;

            Factory = factory;
            PrimitiveFactory = new ModelWithPrimitiveColliderFactory(simulation, errorCollector, factory);

            BaseDirectory = baseDirectory;
            ErrorCollector = errorCollector;

            ModelLoadFunc = modelPath => modelPath is null ? Model.Empty() : Factory.Load(modelPath, MakeLH);
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
                ModelLoadFunc = modelPath => modelPath is null ? Model.Empty() : Factory.Load(modelPath, MakeLH);
            }
            else if (function.Signature == ModelListSignatures.BoundingBox1)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithBoundingBox(modelPath, MakeLH, material));
            }
            else if (function.Signature == ModelListSignatures.BoundingBox2)
            {
                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithBoundingBox(modelPath, MakeLH, ColliderMaterial.Default));
            }
            else if (function.Signature == ModelListSignatures.ConvexHull1)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithConvexHull(modelPath, MakeLH, material));
            }
            else if (function.Signature == ModelListSignatures.ConvexHull2)
            {
                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithConvexHull(modelPath, MakeLH, ColliderMaterial.Default));
            }
            else if (function.Signature == ModelListSignatures.ClosedModel1)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);
                ColliderMaterial material = CreateMaterial(1);

                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, collisionModelPath, MakeLH, material, false));
            }
            else if (function.Signature == ModelListSignatures.ClosedModel2)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);

                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, collisionModelPath, MakeLH, ColliderMaterial.Default, false));
            }
            else if (function.Signature == ModelListSignatures.ClosedModel3)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, modelPath, MakeLH, material, false));
            }
            else if (function.Signature == ModelListSignatures.ClosedModel4)
            {
                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, modelPath, MakeLH, ColliderMaterial.Default, false));
            }
            else if (function.Signature == ModelListSignatures.OpenModel1)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);
                ColliderMaterial material = CreateMaterial(1);

                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, collisionModelPath, MakeLH, material, true));
            }
            else if (function.Signature == ModelListSignatures.OpenModel2)
            {
                string collisionModelPath = Path.Combine(BaseDirectory, (string)function.Args[0]);

                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, collisionModelPath, MakeLH, ColliderMaterial.Default, true));
            }
            else if (function.Signature == ModelListSignatures.OpenModel3)
            {
                ColliderMaterial material = CreateMaterial(0);

                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, modelPath, MakeLH, material, true));
            }
            else if (function.Signature == ModelListSignatures.OpenModel4)
            {
                ModelLoadFunc = modelPath => InvokeWithNonNullPathOrReport(modelPath,
                    modelPath => Factory.LoadWithCollisionModel(modelPath, MakeLH, modelPath, MakeLH, ColliderMaterial.Default, true));
            }
            else if (function.Signature == ModelListSignatures.Box1)
            {
                Box shape = CreateBox(0);
                Pose offset = CreatePoseWithTranslationRotation(3);
                ColliderMaterial material = CreateMaterial(9);
                ModelLoadFunc = modelPath => PrimitiveFactory.Box(modelPath, MakeLH, shape, material, offset);
            }
            else if (function.Signature == ModelListSignatures.Box2)
            {
                Box shape = CreateBox(0);
                Pose offset = CreatePoseWithTranslation(3);
                ColliderMaterial material = CreateMaterial(6);
                ModelLoadFunc = modelPath => PrimitiveFactory.Box(modelPath, MakeLH, shape, material, offset);
            }
            else if (function.Signature == ModelListSignatures.Box3)
            {
                Box shape = CreateBox(0);
                ColliderMaterial material = CreateMaterial(3);
                ModelLoadFunc = modelPath => PrimitiveFactory.Box(modelPath, MakeLH, shape, material, Pose.Identity);
            }
            else if (function.Signature == ModelListSignatures.Box4)
            {
                Box shape = CreateBox(0);
                Pose offset = CreatePoseWithTranslationRotation(3);
                ModelLoadFunc = modelPath => PrimitiveFactory.Box(modelPath, MakeLH, shape, ColliderMaterial.Default, offset);
            }
            else if (function.Signature == ModelListSignatures.Box5)
            {
                Box shape = CreateBox(0);
                Pose offset = CreatePoseWithTranslation(3);
                ModelLoadFunc = modelPath => PrimitiveFactory.Box(modelPath, MakeLH, shape, ColliderMaterial.Default, offset);
            }
            else if (function.Signature == ModelListSignatures.Box6)
            {
                Box shape = CreateBox(0);
                ModelLoadFunc = modelPath => PrimitiveFactory.Box(modelPath, MakeLH, shape, ColliderMaterial.Default, Pose.Identity);
            }
            else if (function.Signature == ModelListSignatures.Cylinder1)
            {
                Cylinder shape = CreateCylinder(0);
                Pose offset = CreatePoseWithTranslationRotation(2);
                ColliderMaterial material = CreateMaterial(8);
                ModelLoadFunc = modelPath => PrimitiveFactory.Cylinder(modelPath, MakeLH, shape, material, offset);
            }
            else if (function.Signature == ModelListSignatures.Cylinder2)
            {
                Cylinder shape = CreateCylinder(0);
                Pose offset = CreatePoseWithTranslation(2);
                ColliderMaterial material = CreateMaterial(5);
                ModelLoadFunc = modelPath => PrimitiveFactory.Cylinder(modelPath, MakeLH, shape, material, offset);
            }
            else if (function.Signature == ModelListSignatures.Cylinder3)
            {
                Cylinder shape = CreateCylinder(0);
                ColliderMaterial material = CreateMaterial(2);
                ModelLoadFunc = modelPath => PrimitiveFactory.Cylinder(modelPath, MakeLH, shape, material, Pose.Identity);
            }
            else if (function.Signature == ModelListSignatures.Cylinder4)
            {
                Cylinder shape = CreateCylinder(0);
                Pose offset = CreatePoseWithTranslationRotation(2);
                ModelLoadFunc = modelPath => PrimitiveFactory.Cylinder(modelPath, MakeLH, shape, ColliderMaterial.Default, offset);
            }
            else if (function.Signature == ModelListSignatures.Cylinder5)
            {
                Cylinder shape = CreateCylinder(0);
                Pose offset = CreatePoseWithTranslation(2);
                ModelLoadFunc = modelPath => PrimitiveFactory.Cylinder(modelPath, MakeLH, shape, ColliderMaterial.Default, offset);
            }
            else if (function.Signature == ModelListSignatures.Cylinder6)
            {
                Cylinder shape = CreateCylinder(0);
                ModelLoadFunc = modelPath => PrimitiveFactory.Cylinder(modelPath, MakeLH, shape, ColliderMaterial.Default, Pose.Identity);
            }
            else if (function.Signature == ModelListSignatures.Sphere1)
            {
                Sphere shape = CreateSphere(0);
                Pose offset = CreatePoseWithTranslationRotation(1);
                ColliderMaterial material = CreateMaterial(7);
                ModelLoadFunc = modelPath => PrimitiveFactory.Sphere(modelPath, MakeLH, shape, material, offset);
            }
            else if (function.Signature == ModelListSignatures.Sphere2)
            {
                Sphere shape = CreateSphere(0);
                Pose offset = CreatePoseWithTranslation(1);
                ColliderMaterial material = CreateMaterial(4);
                ModelLoadFunc = modelPath => PrimitiveFactory.Sphere(modelPath, MakeLH, shape, material, offset);
            }
            else if (function.Signature == ModelListSignatures.Sphere3)
            {
                Sphere shape = CreateSphere(0);
                ColliderMaterial material = CreateMaterial(1);
                ModelLoadFunc = modelPath => PrimitiveFactory.Sphere(modelPath, MakeLH, shape, material, Pose.Identity);
            }
            else if (function.Signature == ModelListSignatures.Sphere4)
            {
                Sphere shape = CreateSphere(0);
                Pose offset = CreatePoseWithTranslationRotation(1);
                ModelLoadFunc = modelPath => PrimitiveFactory.Sphere(modelPath, MakeLH, shape, ColliderMaterial.Default, offset);
            }
            else if (function.Signature == ModelListSignatures.Sphere5)
            {
                Sphere shape = CreateSphere(0);
                Pose offset = CreatePoseWithTranslation(1);
                ModelLoadFunc = modelPath => PrimitiveFactory.Sphere(modelPath, MakeLH, shape, ColliderMaterial.Default, offset);
            }
            else if (function.Signature == ModelListSignatures.Sphere6)
            {
                Sphere shape = CreateSphere(0);
                ModelLoadFunc = modelPath => PrimitiveFactory.Sphere(modelPath, MakeLH, shape, ColliderMaterial.Default, Pose.Identity);
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

            Box CreateBox(int argBeginIndex)
            {
                return new Box((float)function.Args[argBeginIndex], (float)function.Args[argBeginIndex + 1], (float)function.Args[argBeginIndex + 2]);
            }

            Cylinder CreateCylinder(int argBeginIndex)
            {
                return new Cylinder((float)function.Args[argBeginIndex], (float)function.Args[argBeginIndex + 1]);
            }

            Sphere CreateSphere(int argBeginIndex)
            {
                return new Sphere((float)function.Args[argBeginIndex]);
            }

            Pose CreatePoseWithTranslationRotation(int argBeginIndex)
            {
                SixDoF sixDoF = SixDoF.FromDegrees((float)function.Args[argBeginIndex], (float)function.Args[argBeginIndex + 1], (float)function.Args[argBeginIndex + 2],
                    (float)function.Args[argBeginIndex + 3], (float)function.Args[argBeginIndex + 4], (float)function.Args[argBeginIndex + 5]);
                return sixDoF.ToPose();
            }

            Pose CreatePoseWithTranslation(int argBeginIndex)
            {
                SixDoF sixDoF = new((float)function.Args[argBeginIndex], (float)function.Args[argBeginIndex + 1], (float)function.Args[argBeginIndex + 2]);
                return sixDoF.ToPose();
            }

            Model InvokeWithNonNullPathOrReport(string? modelPath, Func< string, Model> func)
            {
                if (modelPath is null)
                {
                    Error error = new(ErrorLevel.Error, $"コマンド '${function.Signature}' を描画用モデルの指定がないモデルに使用することはできません。", null);
                    ErrorCollector.Report(error);
                    return Model.Empty();
                }
                else
                {
                    return func(modelPath);
                }
            }
        }

        public Model Build(string? modelPath)
        {
            Model model = ModelLoadFunc(modelPath);
            return model;
        }


        private delegate Model ModelLoader(string? modelPath);
    }
}
