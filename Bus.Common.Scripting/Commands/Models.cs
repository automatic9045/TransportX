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
    public sealed class Models
    {
        private readonly ScriptWorld World;

        internal Models(ScriptWorld world)
        {
            World = world;
        }

        public void LoadList(string path)
        {
            string listPath = Path.Combine(World.BaseDirectory, path);
            using (StreamReader sr = new StreamReader(Path.Combine(World.BaseDirectory, path)))
            {
                string listDirectory = Path.GetDirectoryName(listPath)!;
                Parser parser = new Parser(ModelListSignatures.All);
                for (int i = 0; !sr.EndOfStream; i++)
                {
                    string[] line = sr.ReadLine()!.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (line.Length == 0) continue;
                    if (line[0].StartsWith("#")) continue;

                    try
                    {
                        if (line.Length < 2) throw new FormatException($"レコード '{line[0]}' は無効です。");

                        string key = line[0];
                        string modelPath = Path.Combine(listDirectory, line[1]);

                        Model model;
                        string collisionCommand = line.Length < 3 ? string.Empty : line[2].ToLowerInvariant();
                        if (collisionCommand.StartsWith('$'))
                        {
                            Function function = parser.Parse(collisionCommand.Substring(1));
                            if (function.Signature == ModelListSignatures.BoundingBox1)
                            {
                                Material material = CreateMaterial(0);

                                model = CollidableModel.LoadWithBoundingBox(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, material);
                            }
                            else if (function.Signature == ModelListSignatures.BoundingBox2)
                            {
                                model = CollidableModel.LoadWithBoundingBox(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, Material.Default);
                            }
                            else if (function.Signature == ModelListSignatures.ClosedModel1)
                            {
                                string collisionModelPath = Path.Combine(listDirectory, (string)function.Args[0]);
                                Material material = CreateMaterial(1);

                                model = CollidableModel.Load(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, collisionModelPath, material, false);
                            }
                            else if (function.Signature == ModelListSignatures.ClosedModel2)
                            {
                                string collisionModelPath = Path.Combine(listDirectory, (string)function.Args[0]);

                                model = CollidableModel.Load(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, collisionModelPath, Material.Default, false);
                            }
                            else if (function.Signature == ModelListSignatures.ClosedModel3)
                            {
                                Material material = CreateMaterial(0);

                                model = CollidableModel.Load(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, modelPath, material, false);
                            }
                            else if (function.Signature == ModelListSignatures.ClosedModel4)
                            {
                                model = CollidableModel.Load(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, modelPath, Material.Default, false);
                            }
                            else if (function.Signature == ModelListSignatures.OpenModel1)
                            {
                                string collisionModelPath = Path.Combine(listDirectory, (string)function.Args[0]);
                                Material material = CreateMaterial(1);

                                model = CollidableModel.Load(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, collisionModelPath, material, true);
                            }
                            else if (function.Signature == ModelListSignatures.OpenModel2)
                            {
                                string collisionModelPath = Path.Combine(listDirectory, (string)function.Args[0]);

                                model = CollidableModel.Load(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, collisionModelPath, Material.Default, true);
                            }
                            else if (function.Signature == ModelListSignatures.OpenModel3)
                            {
                                Material material = CreateMaterial(0);

                                model = CollidableModel.Load(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, modelPath, material, true);
                            }
                            else if (function.Signature == ModelListSignatures.OpenModel4)
                            {
                                model = CollidableModel.Load(
                                    World.DXHost.Device, World.DXHost.Context, World.PhysicsHost.Simulation,
                                    modelPath, modelPath, Material.Default, true);
                            }
                            else if (function.Signature == ModelListSignatures.NonCollision)
                            {
                                model = Model.Load(World.DXHost.Device, World.DXHost.Context, modelPath);
                            }
                            else
                            {
                                throw new FormatException($"コマンド '{line[2]}' は無効です。");
                            }


                            Material CreateMaterial(int argBeginIndex)
                            {
                                return new Material(
                                    (float)function.Args[argBeginIndex], (float)function.Args[argBeginIndex + 1],
                                    new SpringSettings((float)function.Args[argBeginIndex + 2], (float)function.Args[argBeginIndex + 3]));
                            }
                        }
                        else
                        {
                            model = Model.Load(World.DXHost.Device, World.DXHost.Context, modelPath);
                        }
                        World.Models[key] = model;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"モデルを読み込めませんでした ({i + 1} 行目)。", ex);
                    }
                }
            }
        }
    }
}
