using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordRPC;

namespace TransportX.Player
{
    internal class DiscordPresenceHost : IDisposable
    {
        private readonly DiscordRpcClient Client;
        private readonly Timestamps SessionStartTime;

        public DiscordPresenceHost()
        {
            SessionStartTime = Timestamps.Now;

            Client = new DiscordRpcClient("1546876133075714068");
            Client.OnReady += (sender, e) => UpdateStatus();

            if (!Debugger.IsAttached) // デバッガーのアタッチ時、接続を失敗する度に全スレッドが一時停止してしまうため
            {
                Client.Initialize();
            }
        }

        private void UpdateStatus()
        {
            if (!Client.IsInitialized) throw new InvalidOperationException();

            RichPresence presence = new()
            {
                Timestamps = SessionStartTime,
                Buttons = [
                    new Button()
                    {
                        Label = "公式サイトへ",
                        Url = "https://transportx.okaoka-depot.com/",
                    },
                ],
            };
            Client.SetPresence(presence);
        }

        public void Dispose()
        {
            if (Client.IsInitialized) Client.ClearPresence();
            Client.Dispose();
        }
    }
}
