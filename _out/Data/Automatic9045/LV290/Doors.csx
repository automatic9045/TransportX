#load "__Editor.csx"

{
    Component<AvatarDoors>().AddBiford("Front")
        .HingedPanel("FrontDoor1", 0.511)
        .GuidePanel("FrontDoor2", 0.51)
        .PanelThickness(0.02)
        .OpenLeft()
        .OpenAnimation(10, 0, 2, 2.8, [
            (0, 0),
            (0.2, 0.1),
            (0.6, 0.7),
            (1, 1),
        ])
        .CloseAnimation(12, 0, 1, 3.5, [
            (0, 0),
            (0.1, 0.2),
            (0.4, 0.4),
            (1, 1),
        ])
        .Restitution(0.01, 0.5)
        .DoorSwitch("FrontDoor")
        .Build();

    Component<AvatarDoors>().AddSliding("Rear")
        .Panel("RearDoor", 1.005)
        .OpenLeft()
        .OpenAnimation(20, 0, 5, 2.2, [
            (0, 0),
            (0.7, 0.9),
            (0.9, 0.94),
            (1, 1),
        ])
        .CloseAnimation(20, 0, 5, 2.2, [
            (0, 0),
            (0.1, 0.06),
            (0.3, 0.1),
            (1, 1),
        ])
        .Restitution(0.01, 0.01)
        .DoorSwitch("RearDoor")
        .Build();


    Signals.ToSwitchCounter("FrontDoorOpenCount", "FrontDoor", true);
    Signals.ToSwitchCounter("FrontDoorCloseCount", "FrontDoor", false);

    Signals.ToSwitchCounter("RearDoorOpenCount", "RearDoor", true);
    Signals.ToSwitchCounter("RearDoorCloseCount", "RearDoor", false);


    Sounds.Create3D("FrontDoorOpen", "BifoldDoorOpen", -1.15, 3.7, -0.35, 0, 180, 0, 1)
        .PlayStopWhen("FrontDoorOpenCount", "FrontDoorCloseCount");

    Sounds.Create3D("FrontDoorClose", "BifoldDoorClose", -1.15, 3.7, -0.35, 0, 180, 0, 1)
        .PlayStopWhen("FrontDoorCloseCount", "FrontDoorOpenCount");

    Sounds.Create3D("RearDoorOpen", "SlidingDoorOpen", -1.15, 2.3, -5.2, 0, 90, 0, 1)
        .PlayStopWhen("RearDoorOpenCount", "RearDoorCloseCount");

    Sounds.Create3D("RearDoorClose", "SlidingDoorClose", -1.15, 2.3, -5.2, 0, 90, 0, 1)
        .PlayStopWhen("RearDoorCloseCount", "RearDoorOpenCount");
}
