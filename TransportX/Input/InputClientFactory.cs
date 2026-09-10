using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.SDL;
using Vortice.DirectInput;

using TransportX.Diagnostics;
using TransportX.Input.Configuration;

namespace TransportX.Input
{
    internal static class InputClientFactory
    {
        public static InputClient Create(InputHost host, Data.InputConfig config, IErrorCollector errorCollector)
        {
            HashSet<Guid> controllerGuids = [];
            Dictionary<string, InputProfile.GameControllerData> controllersData = [];
            foreach (Data.GameControllerReference controllerRef in config.GameControllers)
            {
                string path = Path.Combine(Data.Config.BaseDirectory, controllerRef.Path);
                Data.Input.GameControllers.GameController data = Data.Input.GameControllers.GameController.Import(path, errorCollector);

                controllerGuids.Add(controllerRef.DeviceGuid);
                controllersData.Add(controllerRef.Key, new InputProfile.GameControllerData(controllerRef.DeviceGuid, data));
            }

            Dictionary<string, InputProfile> profiles = [];
            foreach (Data.InputProfileReference profileRef in config.Profiles)
            {
                string path = Path.Combine(Data.Config.BaseDirectory, profileRef.Path);
                Data.Input.InputProfile data = Data.Input.InputProfile.Import(path, errorCollector);

                IErrorCollector profileErrorCollector = IErrorCollector.Default();
                profileErrorCollector.Reported += (sender, e) =>
                {
                    Error error = e.Error.ChangeSource(path);
                    errorCollector.Report(error);
                };

                InputProfile profile = InputProfile.FromData(profileRef.Key, data, controllersData, profileErrorCollector);
                profiles.Add(profile.Key, profile);
            }

            MessageBox.Button okButton = new("OK", MessageBoxButtonFlags.ReturnkeyDefault);
            MessageBox.Button guidButton = new("GUID をコピー", default);
            foreach ((Guid guid, IDirectInputDevice8 device) in host.JoystickDevices)
            {
                if (controllerGuids.Contains(guid)) continue;

                MessageBox.Show($"未登録のゲームコントローラーを認識しました:\n\n{device.DeviceInfo.ProductName}",
                    [okButton, guidButton], nameof(TransportX), MessageBoxFlags.Information, null, out MessageBox.Button? result);
                if (result != guidButton) continue;

                ProcessStartInfo psi = new()
                {
                    FileName = "clip",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardInputEncoding = Encoding.Unicode,
                };

                using Process process = new()
                {
                    StartInfo = psi,
                };
                process.Start();
                process.StandardInput.Write(device.DeviceInfo.InstanceGuid.ToString("D"));
                process.StandardInput.Close();
                process.WaitForExit();
            }

            return new InputClient(host, profiles);
        }
    }
}
