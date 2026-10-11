#load "__Editor.csx"

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
