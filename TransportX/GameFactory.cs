using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TransportX.Dependency;
using TransportX.Diagnostics;
using TransportX.Input;
using TransportX.Physics;
using TransportX.Rendering;
using TransportX.Worlds;

namespace TransportX
{
    public class GameFactory : IGameFactory
    {
        public IGame Create(PluginLoadContext context, IDXHost dxHost, IDXClient dxClient, IWorldInfo worldInfo)
        {
            Renderer renderer = new Renderer(dxHost, dxClient);
            PhysicsHost physicsHost = PhysicsHost.Create();

            TimeManager timeManager = new();
            InputManager inputManager = new();

            (int PlateX, int PlateZ, Vector3 Position, Vector2 Angle) cameraLocation = (0, 0, new Vector3(0, 10, 0), Vector2.Zero);
            try
            {
                Process process = Process.GetCurrentProcess();
                string savePath = Path.Combine(Path.GetDirectoryName(process.MainModule!.FileName)!, "Save.dat");

                string[] saveContent = File.ReadAllLines(savePath);

                if (int.Parse(saveContent[0]) == process.Id)
                {
                    string[] plateText = saveContent[1].Split(',');
                    int plateX = int.Parse(plateText[0]);
                    int plateZ = int.Parse(plateText[1]);

                    string[] positionText = saveContent[2].Split(',');
                    Vector3 position = new(float.Parse(positionText[0]), float.Parse(positionText[1]), float.Parse(positionText[2]));

                    string[] angleText = saveContent[3].Split(',');
                    Vector2 angle = new(float.Parse(angleText[0]), float.Parse(angleText[1]));

                    cameraLocation = new(plateX, plateZ, position, angle);
                }
            }
            catch { }

            Camera camera = new(cameraLocation.PlateX, cameraLocation.PlateZ, cameraLocation.Position, cameraLocation.Angle);

            ErrorCollector errorCollector = new();
            WorldBuilder worldBuilder = new WorldBuilder(worldInfo)
            {
                DXHost = dxHost,
                DXClient = dxClient,
                PhysicsHost = physicsHost,
                ErrorCollector = errorCollector,
                GameContext = context,
                TimeManager = timeManager,
                InputManager = inputManager,
                Camera = camera,
            };
            WorldBase world = worldBuilder.Build();

            if (errorCollector.HasFatalError)
            {
                PluginLoadContext worldContext = world.WorldContext;
                world.Dispose();
                context.Children.Remove(worldContext);
                worldContext.Unload();

                world = new EmptyWorld(worldBuilder);
            }

            GameCreationInfo info = new()
            {
                Context = context,
                DXHost = dxHost,
                DXClient = dxClient,
                PhysicsHost = physicsHost,
                Renderer = renderer,
                TimeManager = timeManager,
                InputManager = inputManager,
                Camera = camera,
                World = world,
            };
            return new Game(info);
        }
    }
}
