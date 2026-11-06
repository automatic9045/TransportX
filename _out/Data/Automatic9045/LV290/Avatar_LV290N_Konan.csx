#load "__Editor.csx"
#load "Avatar.csx"
#load "Konan/Routes.csx"

{
    Structure.Parts.All["Body"]
        .OverrideBaseColorTexture("Front1", "Models/Skins/Konan/PaintFront1.png")
        .OverrideBaseColorTexture("Front2", "Models/Skins/Konan/PaintFront2.png")
        .OverrideBaseColorTexture("Front3", "Models/Skins/Konan/PaintFront3.png")
        .OverrideBaseColorTexture("Rear1", "Models/Skins/Konan/PaintRear1.png")
        .OverrideBaseColorTexture("Rear2", "Models/Skins/Konan/PaintRear2.png")
        .OverrideBaseColorTexture("L1", "Models/Skins/Konan/PaintL1.png")
        .OverrideBaseColorTexture("L2", "Models/Skins/Konan/PaintL2.png")
        .OverrideBaseColorTexture("L3", "Models/Skins/Konan/PaintL3.png")
        .OverrideBaseColorTexture("R1", "Models/Skins/Konan/PaintR1.png")
        .OverrideBaseColorTexture("R2", "Models/Skins/Konan/PaintR2.png")
        .OverrideBaseColorTexture("R3", "Models/Skins/Konan/PaintR3.png")
        .OverrideBaseColorTexture("Roof1", "Models/Skins/Konan/PaintRoof1.png")
        .OverrideEmissiveTexture("SignFront", "Models/Skins/Konan/SignFront/001.png")
        .OverrideEmissiveTexture("SignRear", "Models/Skins/Konan/SignRear/001.png");

    Structure.Parts.All["RearDoor"]
        .OverrideBaseColorTexture("L1", "Models/Skins/Konan/PaintL1.png");


    var announcementSounds = Sounds.LoadList("Announcements", "Konan/Announcements/Sounds.txt");

    Sounds.CreateSource3D("SpeakerIn", 0, 2.8, -5, 90, 0, 0, 1);
    Sounds.CreateSource3D("SpeakerOutRear", -1.25, 1.2, -3, 0, 0, 90, 1);

    var inPlayer = new TransportX.Extensions.Audio.SoundSequencePlayer(Sounds.Source3Ds["SpeakerIn"].Source);
    var outRearPlayer = new TransportX.Extensions.Audio.SoundSequencePlayer(Sounds.Source3Ds["SpeakerOutRear"].Source);

    int routeId = 1;
    int stepId = -1;

    void SetRoute(int id)
    {
        inPlayer.Stop();
        routeId = id;
        stepId = -1;
        Structure.Parts.All["Body"]
            .OverrideEmissiveTexture("SignFront", $"Models/Skins/Konan/SignFront/{routeId:d3}.png")
            .OverrideEmissiveTexture("SignRear", $"Models/Skins/Konan/SignRear/{routeId:d3}.png");
    }

    void ChangeStep(int idDelta)
    {
        inPlayer.Stop();

        Route route = routes[routeId];
        stepId = int.Min(stepId + idDelta, route.Steps.Count - 1);

        if (0 < idDelta && 0 <= stepId)
        {
            RouteStep step = route.Steps[stepId];
            foreach (string soundKey in step.SoundKeys)
            {
                inPlayer.Enqueue(announcementSounds.Sounds[soundKey]);
            }
        }
    }

    Input.AddButton("RouteUp")
        .Bind("PageUp")
        .OnPressed(button => SetRoute(routeId == 5 ? 1 : routeId + 1))
        .Build();
    Input.AddButton("RouteDown")
        .Bind("PageDown")
        .OnPressed(button => SetRoute(routeId == 1 ? 5 : routeId - 1))
        .Build();

    Input.AddButton("NextStop")
        .Bind("Number1")
        .OnPressed(button => ChangeStep(1))
        .Build();

    int entranceDoorOpenCount = 0;
    int exitDoorOpenCount = 0;
    int allDoorCloseCount = 0;
    Triggers.OnTick(elapsed =>
    {
        Route route = routes[routeId];

        if (Signals.ReadBool("RearDoor"))
        {
            if (Signals.ReadInt("RearDoorOpenCount") != entranceDoorOpenCount)
            {
                entranceDoorOpenCount = Signals.ReadInt("RearDoorOpenCount");

                if (0 <= stepId && stepId < route.Steps.Count - 1)
                {
                    RouteStep step = route.Steps[stepId];
                    for (int i = 0; i < 5; i++)
                    {
                        foreach (string soundKey in step.DoorSoundKeys)
                        {
                            outRearPlayer.Enqueue(announcementSounds.Sounds[soundKey]);
                        }
                    }
                }
            }
        }
        else
        {
            outRearPlayer.Stop();
        }

        if (Signals.ReadInt("FrontDoorOpenCount") != exitDoorOpenCount)
        {
            exitDoorOpenCount = Signals.ReadInt("FrontDoorOpenCount");

            if (stepId == route.Steps.Count - 1)
            {
                RouteStep step = route.Steps[stepId];
                if (0 < step.DoorSoundKeys.Count)
                {
                    inPlayer.Stop();
                    foreach (string soundKey in step.DoorSoundKeys)
                    {
                        inPlayer.Enqueue(announcementSounds.Sounds[soundKey]);
                    }
                }
            }
        }

        if (Signals.ReadInt("AllDoorCloseCount") != allDoorCloseCount)
        {
            allDoorCloseCount = Signals.ReadInt("AllDoorCloseCount");
            if (routes[routeId].IsInService && 0 <= stepId && stepId < routes[routeId].Steps.Count - 1)
            {
                inPlayer.PlayImmediate(announcementSounds.Sounds["発車します。おつかまりください。"]);
            }
        }

        inPlayer.Tick(elapsed);
        outRearPlayer.Tick(elapsed);
    });
}
