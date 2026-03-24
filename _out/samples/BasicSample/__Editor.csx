#r "..\..\bin\Debug\TransportX.dll"
#r "..\..\bin\Debug\TransportX.Extensions.dll"
#r "..\..\bin\Debug\TransportX.Scripting.dll"
#r "..\..\bin\Debug\Plugins\TransportX.Domains.RoadTraffic.dll"

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Numerics;
global using System.Text;
global using System.Threading.Tasks;

global using TransportX.Scripting;
global using TransportX.Scripting.Commands;
global using TransportX.Scripting.Components;
global using TransportX.Domains.RoadTraffic.Scripting;
global using TransportX.Domains.RoadTraffic.Scripting.Commands;

ScriptWorld World => null!;

Avatar Avatar => null!;
Background Background => null!;
Components Components => null!;
Debug Debug => null!;
DirectionalLight DirectionalLight => null!;
WorldEnvironment Environment => null!;
Models Models => null!;
Plates Plates => null!;
Network Network => null!;
Triggers Triggers => null!;

public T Component<T>() where T : class, IComponentCommand => throw new InvalidOperationException();
