using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            Camera camera = new();

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
