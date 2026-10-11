#load "__Editor.csx"

Structure.Parts.Add("Body", "Body_LV290N", 0, 0, 0).BuildDynamic(7500);

Structure.Parts.Add("FrontDoor1", "BifoldDoor_HingedPanel", -1.16, 0, -0.4).BuildKinematic();
Structure.Parts.Add("FrontDoor2", "BifoldDoor_GuidePanel", -1.16, 0, -1.42).BuildKinematic();
Structure.Parts.Add("RearDoor", "PocketDoor", -1.16, 0, -6.39).BuildKinematic();

Structure.Parts.Add("SideMirrorL", "SideMirrorL", 0, 0, 0).BuildKinematic();
Structure.Parts.Add("SideMirrorR", "SideMirrorR", 0, 0, 0).BuildKinematic();
