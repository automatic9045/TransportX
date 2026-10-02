#load "__Editor.csx"

using TransportX.Rendering;
using TransportX.Rendering.Pipelines;
using TransportX.Spatial;
using TransportX.Domains.Equipment.Cameras;

Structure.Parts.Add("Body", "Body_LV290N", 0, 0, 0).BuildDynamic(7500);

Structure.Parts.Add("FrontDoor1", "BifoldDoor_HingedPanel", -1.16, 0, -0.4).BuildKinematic();
Structure.Parts.Add("FrontDoor2", "BifoldDoor_GuidePanel", -1.16, 0, -1.42).BuildKinematic();
Structure.Parts.Add("RearDoor", "PocketDoor", -1.16, 0, -6.39).BuildKinematic();

Structure.Parts.Add("SideMirrorL", "SideMirrorL", 0, 0, 0).BuildKinematic();
Structure.Parts.Add("SideMirrorR", "SideMirrorR", 0, 0, 0).BuildKinematic();

Component<AvatarCameras>().AddSceneCapture("SideMirrorL")
    .Position("Body", -1.3547, 2.0748, 0.1199, 10, 162, 0)
    .TextureSize(128, 256)
    .FieldOfView(90)
    .AspectRatio(16.0 / 31.0)
    .Reflect()
    .DisableShadows()
    .ProjectOntoPart("SideMirrorL", "SideMirror")
    .Build();

Component<AvatarCameras>().AddSceneCapture("SideMirrorR")
    .Position("Body", 1.3206, 1.9698, -0.5120, 10, 190, 0)
    .TextureSize(128, 256)
    .FieldOfView(90)
    .AspectRatio(16.0 / 31.0)
    .Reflect()
    .DisableShadows()
    .ProjectOntoPart("SideMirrorR", "SideMirror")
    .Build();
