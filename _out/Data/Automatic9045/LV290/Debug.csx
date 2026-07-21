#load "__Editor.csx"

{
    Input.AddButton("Debug")
        .Bind("Q")
        .OnPressed(instance =>
        {
            var engine = (EngineFactory)Component<Powertrain>().Modules.Factories["Engine"];
            var clutch = (FluidClutchFactory)Component<Powertrain>().Modules.Factories["Clutch"];
            var gearbox = (GearboxFactory)Component<Powertrain>().Modules.Factories["Gearbox"];

            Debug.ShowDialog($"{engine.BuiltModule.Rpm:f1} rpm, {gearbox.BuiltModule.Gear}, " +
                $"C{clutch.BuiltModule.Engagement:f2}, T{engine.BuiltModule.Throttle:f2}");
        })
        .Build();

    TransportX.Spatial.WorldPose initialPose = default;
    Input.AddButton("Reset")
        .Bind("R")
        .Build();

    Triggers.OnStart(() =>
    {
        var engine = (EngineFactory)Component<Powertrain>().Modules.Factories["Engine"];
        engine.Output.BuiltShaft.Rpm = 600;
    });

    Triggers.OnTick(elapsed =>
    {
        if (initialPose == default)
        {
            initialPose = Avatar.WorldPose;
        }

        if (Input.Buttons["Reset"].IsPressed)
        {
            Avatar.Locate(initialPose);
            foreach (var model in Avatar.Structure)
            {
                if (model is TransportX.Spatial.DynamicTransformedModel dynamicModel) dynamicModel.Body.Velocity = default;
                model.Pose = model.BasePose * Avatar.WorldPose.Pose;
            }
        }
    });
}
