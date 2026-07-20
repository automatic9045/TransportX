#load "__Editor.csx"

#load "Init.csx"
#load "Input.csx"
#load "Input_Amt.csx"
#load "Structure.csx"
#load "Chassis.csx"
#load "Powertrain.csx"
#load "Powertrain_Amt.csx"
#load "Audio.csx"

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

Triggers.OnStart(() =>
{
    var engine = (EngineFactory)Component<Powertrain>().Modules.Factories["Engine"];
    engine.Output.BuiltShaft.Rpm = 1000;
});
