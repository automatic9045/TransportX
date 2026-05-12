using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NativeFileDialogs.Net;

using TransportX.Worlds;

namespace TransportX.Player.Launcher
{
    internal class LauncherApp : IApp
    {
        private readonly IAppHost Host;

        public bool IsDisposed { get; private set; } = false;

        public LauncherApp(IAppHost host)
        {
            Host = host;
            Host.Platform.Window.Update += OnUpdate;
        }

        public void Dispose()
        {
            Host.Platform.Window.Update -= OnUpdate;
            IsDisposed = true;
        }

        private void OnUpdate(double deltaTime)
        {
            if (IsDisposed) return;

            Dictionary<string, string> filter = new()
            {
                { "ワールド情報ファイル", "xml" },
                { "すべてのファイル", "*" },
            };
            string defaultDirectory = Path.GetDirectoryName(typeof(LauncherApp).Assembly.Location)!;
            NfdStatus status = Nfd.OpenDialog(out string? outPath, filter, defaultDirectory);

            if (status != NfdStatus.Ok || outPath is null)
            {
                System.Environment.Exit(0);
                return;
            }

            WorldInfo worldInfo = WorldInfo.Deserialize(outPath, false);
            AppReference appReference = AppReference.FromPath(worldInfo.AppPath, null);
            WorldAppParameters parameters = new(worldInfo);

            Host.RequestLoadApp(appReference, parameters);
        }


        internal class Factory : IAppFactory
        {
            public IApp Create(IAppHost host, IAppParameters parameters)
            {
                return new LauncherApp(host);
            }
        }
    }
}
