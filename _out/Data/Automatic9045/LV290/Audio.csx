#load "__Editor.csx"

{
    Sounds.LoadList("Sounds.txt");


    Sounds.Create3D("Engine", "Engine", 0, 0, -10, 5)
        .Loop(() => (1, 0.0036f * Component<Powertrain>().Modules.All["Engine"].Module.OutputShafts[0].AngularVelocity));

    Sounds.Create3D("EngineIdling", "EngineIdling", 0, 0, -10, 5)
        .Loop(() =>
        {
            var engineOutput = Component<Powertrain>().Modules.All["Engine"].Module.OutputShafts[0];
            return (float.Clamp(-(engineOutput.Rpm - 600) / 100 + 1, 0, 1), 0.0166f * engineOutput.AngularVelocity);
        });


    Sounds.Create3D("FrontDoorOpen", "BifoldDoorOpen", -1.15, 3.7, -0.35, 0, 180, 0, 1)
        .PlayStopWhen("FrontDoorOpenCount", "FrontDoorCloseCount");

    Sounds.Create3D("FrontDoorClose", "BifoldDoorClose", -1.15, 3.7, -0.35, 0, 180, 0, 1)
        .PlayStopWhen("FrontDoorCloseCount", "FrontDoorOpenCount");

    Sounds.Create3D("RearDoorOpen", "SlidingDoorOpen", -1.15, 2.3, -5.2, 0, 90, 0, 1)
        .PlayStopWhen("RearDoorOpenCount", "RearDoorCloseCount");

    Sounds.Create3D("RearDoorClose", "SlidingDoorClose", -1.15, 2.3, -5.2, 0, 90, 0, 1)
        .PlayStopWhen("RearDoorCloseCount", "RearDoorOpenCount");
}
