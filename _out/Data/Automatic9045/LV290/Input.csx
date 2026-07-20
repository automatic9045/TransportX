#load "__Editor.csx"

Input.AddAxis("Steering", -1, 0, 1)
    .BindPlusMinusForSteering("Right", "Left", 9, 72, 0.25, 0.03)
    .BindResetForSteering("Slash", 9, 72, 0.3, 0.12)
    .ForwardToSignal("Steering")
    .Build();

Input.AddAxis("Throttle", 0, 0, 1)
    .BindSecondaryPlus("Up", 1, 0.6)
    .BindPlus("BackSlash", 1)
    .AutoRelease(1.2)
    .ForwardToSignal("PedalThrottle")
    .Build();

Input.AddAxis("Brake", 0, 0, 1)
    .BindPlus("Down", 0.75)
    .BindReset("Up", 1)
    .ForwardToSignal("Brake")
    .Build();

Input.AddButton("FrontDoor")
    .Bind("KeypadDivide")
    .ForwardToSignal("FrontDoorBase")
    .Build();

Input.AddButton("RearDoor")
    .Bind("KeypadMultiply")
    .ForwardToSignal("RearDoorBase")
    .Build();

Signals.ToToggle("FrontDoor", "FrontDoorBase", false);
Signals.ToToggle("RearDoor", "RearDoorBase", false);
