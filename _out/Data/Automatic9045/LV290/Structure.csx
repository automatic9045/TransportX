#load "__Editor.csx"

Models.LoadList("Models.txt");

Structure.Parts.Add("Body", "Body_LV290N", 0, 0, 0).BuildDynamic(7500);

Structure.Parts.Add("FrontDoor1", "BifoldDoor_HingedPanel", -1.16, 0, -0.4).BuildKinematic();
Structure.Parts.Add("FrontDoor2", "BifoldDoor_GuidePanel", -1.16, 0, -1.42).BuildKinematic();
Structure.Parts.Add("RearDoor", "PocketDoor", -1.16, 0, -6.39).BuildKinematic();

Component<AvatarDoors>().AddBiford("Front")
    .HingedPanel("FrontDoor1", 0.511)
    .GuidePanel("FrontDoor2", 0.51)
    .PanelThickness(0.02)
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
