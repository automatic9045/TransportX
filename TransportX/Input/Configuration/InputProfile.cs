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
                foreach (Data.Input.ControllerButton controllerButtonData in buttonData.Controllers)
                {
                    if (GetControllerData(controllerButtonData.Key, out GameControllerData controllerData))
                    {
                        JoystickButtonBinding joystick = new(controllerData.Guid, controllerButtonData.ButtonIndex);
                        joysticks.Add(joystick);
                    }
                }

                return new ButtonBinding()
                {
                    Keys = buttonData.Keyboard.ConvertAll(keyData => keyData.Code),
                    Joysticks = joysticks,
                };
            });

            Dictionary<string, AxisBinding> axisBindings = data.Axes.ToDictionary(axisData => axisData.Key, axisData =>
            {
                List<JoystickAxisBinding> joysticks = [];
                foreach (Data.Input.ControllerAxis controllerAxisData in axisData.Controllers)
                {
                    if (GetControllerData(controllerAxisData.Key, out GameControllerData controllerData))
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
