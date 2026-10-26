#load "__Editor.csx"
#load "Avatar.csx"

{
    Structure.Parts.All["Body"]
        .OverrideBaseColorTexture("Front1", "Models/Skins/Shimoden/PaintFront1.png")
        .OverrideBaseColorTexture("Front2", "Models/Skins/Shimoden/PaintFront2.png")
        .OverrideBaseColorTexture("Front3", "Models/Skins/Shimoden/PaintFront3.png")
        .OverrideBaseColorTexture("Rear1", "Models/Skins/Shimoden/PaintRear1.png")
        .OverrideBaseColorTexture("Rear2", "Models/Skins/Shimoden/PaintRear2.png")
        .OverrideBaseColorTexture("L1", "Models/Skins/Shimoden/PaintL1.png")
        .OverrideBaseColorTexture("L2", "Models/Skins/Shimoden/PaintL2.png")
        .OverrideBaseColorTexture("L3", "Models/Skins/Shimoden/PaintL3.png")
        .OverrideBaseColorTexture("R1", "Models/Skins/Shimoden/PaintR1.png")
        .OverrideBaseColorTexture("R2", "Models/Skins/Shimoden/PaintR2.png")
        .OverrideBaseColorTexture("R3", "Models/Skins/Shimoden/PaintR3.png")
        .OverrideBaseColorTexture("Roof1", "Models/Skins/Shimoden/PaintRoof1.png")
        .OverrideEmissiveTexture("SignFront", "Models/Skins/Shimoden/PaintSignFront.png")
        .OverrideEmissiveTexture("SignRear", "Models/Skins/Shimoden/PaintSignRear.png");

    Structure.Parts.All["RearDoor"]
        .OverrideBaseColorTexture("L1", "Models/Skins/Shimoden/PaintL1.png");
}
