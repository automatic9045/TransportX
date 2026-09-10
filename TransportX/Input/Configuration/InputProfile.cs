using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;

using TransportX.Diagnostics;

namespace TransportX.Input.Configuration
{
    public class InputProfile
    {
        public static InputProfile Empty(string key)
            => new(key, new Dictionary<string, ButtonBinding>(), new Dictionary<string, AxisBinding>());


        public string Key { get; }
        public IReadOnlyDictionary<string, ButtonBinding> ButtonBindings { get; }
        public IReadOnlyDictionary<string, AxisBinding> AxisBindings { get; }

        public InputProfile(string key, IReadOnlyDictionary<string, ButtonBinding> buttonBindings, IReadOnlyDictionary<string, AxisBinding> axisBindings)
        {
            Key = key;
            ButtonBindings = buttonBindings;
            AxisBindings = axisBindings;
        }

        public static InputProfile FromData(string key,
            Data.Input.InputProfile data, IReadOnlyDictionary<string, GameControllerData> controllersData, IErrorCollector errorCollector)
        {
            Dictionary<string, ButtonBinding> buttonBindings = data.Buttons.ToDictionary(buttonData => buttonData.Key, buttonData =>
            {
                List<JoystickButtonBinding> joysticks = [];
                List<JoystickPovButtonBinding> joystickPovs = [];
                foreach (Data.Input.ControllerButtonBase controllerButtonBaseData in buttonData.Controllers)
                {
                    if (!GetControllerData(controllerButtonBaseData.Key, out GameControllerData controllerData)) continue;

                    switch (controllerButtonBaseData)
                    {
                        case Data.Input.ControllerButton controllerButtonData:
                        {
                            JoystickButtonBinding joystick = new(controllerData.Guid, controllerButtonData.ButtonIndex);
                            joysticks.Add(joystick);
                            break;
                        }
                        case Data.Input.ControllerPovButton controllerPovButtonData:
                        {
                            JoystickPovButtonBinding joystick = new(controllerData.Guid, controllerPovButtonData.PovIndex, controllerPovButtonData.Direction);
                            joystickPovs.Add(joystick);
                            break;
                        }
                    }
                }

                return new ButtonBinding()
                {
                    Keys = buttonData.Keyboard.ConvertAll(keyData => keyData.Code),
                    Joysticks = joysticks,
                    JoystickPovs = joystickPovs,
                };
            });

            Dictionary<string, AxisBinding> axisBindings = data.Axes.ToDictionary(axisData => axisData.Key, axisData =>
            {
                List<JoystickAxisBinding> joysticks = [];
                List<JoystickPovAxisBinding> joystickPovs = [];
                foreach (Data.Input.ControllerAxisBase controllerAxisBaseData in axisData.Controllers)
                {
                    if (!GetControllerData(controllerAxisBaseData.Key, out GameControllerData controllerData)) continue;

                    switch (controllerAxisBaseData)
                    {
                        case Data.Input.ControllerAxis controllerAxisData:
                        {
                            Data.Input.GameControllers.Axis axisConfiguration = controllerData.Data.Axes.FirstOrDefault(x => x.Type == controllerAxisData.AxisType) ?? new();

                            JoystickAxisBinding joystick = new(controllerData.Guid, controllerAxisData.AxisType)
                            {
                                RawMin = axisConfiguration.RawMin,
                                RawNeutral = axisConfiguration.RawNeutral,
                                RawMax = axisConfiguration.RawMax,
                                IsInverted = axisConfiguration.IsInverted,
                            };
                            joysticks.Add(joystick);
                            break;
                        }
                        case Data.Input.ControllerPovAxis controllerPovAxisData:
                        {
                            JoystickPovAxisBinding joystick = new(controllerData.Guid, controllerPovAxisData.PovIndex, controllerPovAxisData.AxisType)
                            {
                                RawMin = JoystickPovObserver.AxisMin,
                                RawNeutral = JoystickPovObserver.AxisNeutral,
                                RawMax = JoystickPovObserver.AxisMax,
                                IsInverted = false,
                            };
                            joystickPovs.Add(joystick);
                            break;
                        }
                    }
                }

                return new AxisBinding()
                {
                    KeyboardPlus = CreateKeyboardBindings(axisData.KeyboardPlus),
                    KeyboardMinus = CreateKeyboardBindings(axisData.KeyboardMinus),
                    KeyboardReset = CreateKeyboardBindings(axisData.KeyboardReset),
                    Joysticks = joysticks,
                };


                Dictionary<string, KeyboardAxisBinding> CreateKeyboardBindings(IEnumerable<Data.Input.KeyboardAction> actions)
                {
                    return actions.ToDictionary(actionData => actionData.Key, actionData =>
                    {
                        List<Key> keys = actionData.Keys.ConvertAll(keyData => keyData.Code);
                        return new KeyboardAxisBinding(actionData.Key, keys);
                    });
                }
            });

            return new InputProfile(key, buttonBindings, axisBindings);


            bool GetControllerData(string controllerKey, out GameControllerData guid)
            {
                if (controllersData.TryGetValue(controllerKey, out guid)) return true;

                Error error = new(ErrorLevel.Error, $"ゲームコントローラー '{controllerKey}' は登録されていません。", null);
                errorCollector.Report(error);
                return false;
            }
        }


        public readonly record struct GameControllerData(Guid Guid, Data.Input.GameControllers.GameController Data);
    }
}
