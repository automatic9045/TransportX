#r "..\..\..\bin\Release\TransportX.dll"
#r "..\..\..\bin\Release\TransportX.Extensions.dll"
#r "..\..\..\bin\Release\TransportX.Scripting.dll"
#r "..\..\..\bin\Release\Plugins\TransportX.Domains.Equipment.dll"
#r "..\..\..\bin\Release\Plugins\TransportX.Domains.RoadVehicles.dll"

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Numerics;
global using System.Text;
global using System.Threading.Tasks;

global using TransportX;
global using TransportX.Scripting;
global using TransportX.Scripting.Avatars;
global using TransportX.Scripting.Avatars.Commands;
global using TransportX.Scripting.Commands;
global using TransportX.Domains.Equipment.Scripting;
global using TransportX.Domains.Equipment.Scripting.Commands;
global using TransportX.Domains.RoadVehicles.Scripting.Commands;
global using TransportX.Domains.RoadVehicles.Scripting.Commands.Chassis;
global using TransportX.Domains.RoadVehicles.Scripting.Commands.Extensions;
global using TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain;
global using TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ControllerFactories;
global using TransportX.Domains.RoadVehicles.Scripting.Commands.Powertrain.ModuleFactories;

ScriptAvatar Avatar => null!;

Components Components => null!;
Signals Signals => null!;
Debug Debug => null!;
Input Input => null!;
Models Models => null!;
Sounds Sounds => null!;
Spec Spec => null!;
Structure Structure => null!;
Triggers Triggers => null!;
Viewpoints Viewpoints => null!;

public T Component<T>() where T : class, IComponentCommand => throw new InvalidOperationException();
