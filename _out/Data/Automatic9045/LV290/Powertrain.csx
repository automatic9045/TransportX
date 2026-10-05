#load "__Editor.csx"

{
    var engine = Component<Powertrain>().Modules.AddEngine("Engine")
        .PerformanceCurve([
            (0, 0),
            (990, 626),
            (1200, 700),
            (1390, 734),
            (1950, 735),
            (2415, 700),
            (3775, 0),
        ])
        .Friction(240)
        .OutputInertia(3);

    /*var clutch = Component<Powertrain>().Modules.AddFrictionClutch("Clutch")
        .FrictionCoefficient(900)
        .OutputInertia(0.15);*/

    var clutch = Component<Powertrain>().Modules.AddFluidClutch("Clutch")
        .CapacityFactorCurve([
            (0, 0.05),
            (0.5, 0.04),
            (0.8, 0.02),
            (0.9, 0.01),
            (1, 0),
        ])
        .TorqueRatioCurve([
            (0, 1.8),
            (0.5, 1.4),
            (0.8, 1.1),
            (0.9, 1),
            (1, 1),
        ])
        .Coefficients(900, 0.05, 0.08)
        .LockUpSpeed(1.5, 10)
        .OutputInertia(0.15);

    var gearbox = Component<Powertrain>().Modules.AddGearbox("Gearbox")
        .GearRatios(6.615, 4.095, 2.358, 1.531, 1, 0.722)
        .ReverseGearRatios(-6.615)
        .OutputInertia(0.25);

    var differential = Component<Powertrain>().Modules.AddDifferential("Differential")
        .FinalRatio(6.5)
        .OutputInertia(20, 20);

    var wheelAdapterL = Component<Powertrain>().Modules.AddWheelAdapter("WheelL")
        .WheelPart("WheelRL")
        .Reverse();

    var wheelAdapterR = Component<Powertrain>().Modules.AddWheelAdapter("WheelR")
        .WheelPart("WheelRR");

    clutch.Input.ConnectTo(engine.Output);
    gearbox.Input.ConnectTo(clutch.Output);
    differential.Input.ConnectTo(gearbox.Output);
    wheelAdapterL.Input.ConnectTo(differential.OutputL);
    wheelAdapterR.Input.ConnectTo(differential.OutputR);


    Component<Powertrain>().Controllers.AddEcu("ECU")
        .PedalThrottle("PedalThrottle")
        .MinMaxThrottle("TcuMinThrottle", "TcuMaxThrottle")
        .Modules("Engine", "Clutch", "Gearbox")
        .AntiStall()
        .IdlingGains(0.01, 0.05, 0.0002)
        .IdlingRpm(600, 575)
        .LimitRpm(3500);


    Sounds.Create3D("Engine", "Engine", 0, 0, -10, 5)
        .Loop()
        .Pitch(elapsed => 0.0036f * engine.BuiltModule.Output.AngularVelocity);

    float engineThrottleVolume = 0;
    float engineThrottlePitch = 0;
    Sounds.Create3D("EngineIdling", "EngineIdling", 0, 0, -10, 5)
        .Loop()
        .Volume(elapsed =>
        {
            float throttle = engine.BuiltModule.Throttle;
            float delta = 2 * (float)elapsed.TotalSeconds;
            float targetVolume = throttle < 0.1f ? throttle / 0.1f : float.Clamp(float.Lerp(1, 0.25f, (throttle - 0.4f) / 0.2f), 0.25f, 1);
            float nextVolume =
                targetVolume < engineThrottleVolume ? float.Max(targetVolume, engineThrottleVolume - delta)
                : engineThrottleVolume < targetVolume ? float.Min(targetVolume, engineThrottleVolume + delta)
                : targetVolume;

            engineThrottleVolume = nextVolume;
            return nextVolume;
        })
        .Pitch(elapsed =>
        {
            float delta = 3 * (float)elapsed.TotalSeconds;
            float targetPitch = float.Clamp(float.Lerp(1, 3, (engine.BuiltModule.Throttle - 0.39f) / 0.61f), 0, 2);
            float nextPitch =
                targetPitch < engineThrottlePitch ? float.Max(targetPitch, engineThrottlePitch - delta)
                : engineThrottlePitch < targetPitch ? float.Min(targetPitch, engineThrottlePitch + delta)
                : targetPitch;

            engineThrottlePitch = nextPitch;
            return nextPitch;
        });
}
