#r "../../../bin/Release/TransportX.dll"
#r "../../../bin/Release/TransportX.Extensions.dll"
#r "../../../bin/Release/TransportX.Scripting.dll"
#r "../../../bin/Release/Plugins/TransportX.Domains.RoadTraffic.dll"

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Numerics;
global using System.Text;
global using System.Threading.Tasks;

global using TransportX;
global using TransportX.Scripting;
global using TransportX.Scripting.Commands;
global using TransportX.Scripting.Worlds;
global using TransportX.Scripting.Worlds.Commands;
global using TransportX.Scripting.Worlds.Commands.Extensions;
global using TransportX.Scripting.Worlds.Components;
global using TransportX.Domains.RoadTraffic.Scripting;
global using TransportX.Domains.RoadTraffic.Scripting.Commands;

ScriptWorld World => null!;

Avatar Avatar => null!;
Background Background => null!;
Camera Camera => null!;
Chunks Chunks => null!;
Components Components => null!;
CrossSections CrossSections => null!;
Debug Debug => null!;
DirectionalLight DirectionalLight => null!;
WorldEnvironment Environment => null!;
Input Input => null!;
Models Models => null!;
Network Network => null!;
Signals Signals => null!;
Sounds Sounds => null!;
Triggers Triggers => null!;

public T Component<T>() where T : class, IWorldInstantiable<T>, IComponentCommand => throw new InvalidOperationException();
