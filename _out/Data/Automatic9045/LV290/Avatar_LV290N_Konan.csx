#load "__Editor.csx"
#load "Avatar.csx"

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
}
