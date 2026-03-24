#load "__Editor.csx"

Avatar.Load(@"..\LV290\TransportX.Sample.LV290.dll");
Avatar.Locate(0, 0, -1, 0.2, 45, 0, 2, 0);

Models.LoadList("Models.txt");
//Debug.ShowDialog("hello");

Network.LaneTraffic.AddType("Pedestrians", "歩行者", "#FFFF00");
Network.LaneTraffic.AddType("Buses", "バス", "#00FF00");
Network.LaneTraffic.AddType("NormalCars", "その他自動車", "#0000FF");
Network.LaneTraffic.AddGroup("Cars", "Buses|NormalCars", "#00FFFF");

Network.LaneLayouts.Load("Layout1", "LaneLayout1.xml");

Component<TrafficAgents>().AddSpawner<RandomTrafficSpawnerTemplate>("Random");
Component<TrafficAgents>().AddAgent<CarTemplate>("AICar");
Component<TrafficAgents>().Generate("NormalCars", @"Traffic\NormalCars.xml");

Component<TrafficSignals>().AddController("4Forked_Normal", @"SignalControllers\4Forked_Normal.xml");

// --------------------------------------------------
// テンプレートの定義

var tSpline = Network.Templates.CreateSpline("SplineTemplate1", "Layout1")
    .SpeedLimit(1, 60)
    .SpeedLimit(2, 60);
tSpline.PutStructure(["Road1_1", "Road1_1", "Road1_1", "Road1_1", "Road1_2", "Road1_3", "Road1_3", "Road1_3", "Road1_3", "Road1_3"], 0, 0, 0, 0, 1, 1);
tSpline.PutStructure(["Road1_Sidewalk"], 0, 0, 0, 0, 1, 1);

tSpline = Network.Templates.CreateSpline("SplineTemplate2", "Layout1");
tSpline.PutStructure(["Road1_10m"], 0, 0, 0, 0, 10, 10);

var tJunction = Network.Templates.CreateJunction("3Forked1")
    .SignalController("4Forked_Normal");
tJunction.AddPort("S", "Layout1", 0, 0, 0, 0, 180, 0);
tJunction.AddPort("N", "Layout1", 0, 0, 20, 0, 0, 0);
tJunction.AddPort("E", "Layout1", 10, 0, 10, 0, 90, 0);
tJunction.Wire("SN0", "S", 0, "N", 3).StraightToEnd();
tJunction.Wire("SN1", "S", 1, "N", 2).Deflection(0).Signal("V_Car").StraightToEnd();
tJunction.Wire("SN2", "S", 2, "N", 1).Deflection(0).Signal("V_Car").StraightToEnd();
tJunction.Wire("SN3", "S", 3, "N", 0).StraightToEnd();
tJunction.Wire("SE0", "S", 0, "E", 3);
tJunction.Wire("SE1", "S", 1, "E", 2).Deflection(1).Signal("H_Car")/*.Yield("SN1")*/;
tJunction.Wire("SE2", "S", 2, "E", 1).Deflection(1).Signal("V_Car").Yield("SN1", "NE2");
tJunction.Wire("SE3", "S", 3, "E", 0);
tJunction.Wire("NE0", "N", 0, "E", 3);
tJunction.Wire("NE1","N", 1, "E", 2).Deflection(-1).Signal("H_Car")/*.Yield("SN1", "SN2", "SE2")*/;
tJunction.Wire("NE2","N", 2, "E", 1).Deflection(-1).Signal("V_Car");
tJunction.Wire("NE3","N", 3, "E", 0);
tJunction.PutStructure("Road1_3", 0, 0, 0);
tJunction.PutStructure("Road1_3", 0, 0, 19);
tJunction.PutStructure("Signal_L", -5.25, 0, 20, 0, 0, 0);
tJunction.PutSignalStructure("Signal_L_CarRed", -5.25, 0, 20, 0, 0, 0, "V_Car", 0);
tJunction.PutSignalStructure("Signal_L_CarYellow", -5.25, 0, 20, 0, 0, 0, "V_Car", 1);
tJunction.PutSignalStructure("Signal_L_CarGreen", -5.25, 0, 20, 0, 0, 0, "V_Car", 2);
tJunction.PutStructure("Signal_L", 5.25, 0, 0, 0, 180, 0);
tJunction.PutSignalStructure("Signal_L_CarRed", 5.25, 0, 0, 0, 180, 0, "V_Car", 0);
tJunction.PutSignalStructure("Signal_L_CarYellow", 5.25, 0, 0, 0, 180, 0, "V_Car", 1);
tJunction.PutSignalStructure("Signal_L_CarGreen", 5.25, 0, 0, 0, 180, 0, "V_Car", 2);
tJunction.PutStructure("Signal_L", -5.25, 0, 4.75, 0, -90, 0);
tJunction.PutSignalStructure("Signal_L_CarRed", -5.25, 0, 4.75, 0, -90, 0, "H_Car", 0);
tJunction.PutSignalStructure("Signal_L_CarYellow", -5.25, 0, 4.75, 0, -90, 0, "H_Car", 1);
tJunction.PutSignalStructure("Signal_L_CarGreen", -5.25, 0, 4.75, 0, -90, 0, "H_Car", 2);

tJunction = Network.Templates.CreateJunction("DeadEnd1");
tJunction.AddPort("0", "Layout1", 0, 0, 0, 0, 180, 0);
var tJunctionPath = tJunction.Wire("", "0", 2, "0", 1);
tJunctionPath
    .StraightTo(-2, 0, 10, 0, 0, 0, out var s1)
    .BezierTo(5.5, 0, 10, 0, 180, 0, out var s2)
    .BezierToEnd();
tJunctionPath.Width
    .Constant(5)
    .TransitionTo(2.25, 2, s1 - 5)
    .Constant(s2 - s1);
tJunction.PutStructure("RoadDeadEnd1", 0, 0, 0);

// テンプレートの定義 ここまで
// --------------------------------------------------

Environment.SetDefault("Environment1.xml");

DirectionalLight.SetColor("#FFFFFF");
DirectionalLight.SetDirection(-1, -4, 2);
DirectionalLight.SetIntensity(3);

Background.Add("Background");

Plates[-1, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[-1, 1].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 1].PutStructure("Grass", 0, -0.01, 0);

var factory = Plates[-1, 0].BeginSpline("SplineTemplate2", 160, 0, 30);
for (int i = 0; i < 5; i++) factory.Curves.Straight(250);
factory.PutStructure(["Pedestrian"], -5.5, 0.15, 0, 20, 0, 0, 1);
var spline = factory.Build();

factory = Plates[-1, 0].BeginSpline("SplineTemplate2", 246, 0, 141.25, 0, -90, 0);
factory.Curves.Straight(80);
spline = factory.Build();

factory = Plates[0, 0].BeginSpline("SplineTemplate1", 10, 0, 0);
factory.Curves.Straight(100);
spline = factory.Build();

Plates[0, 0].PutStructure("RoadTerminal1", -1.7, 0, 30);
Plates[0, 0].PutStructure("BusStop", -4.5, 0.15, 49.3);

var junction = spline.IntoJunction("3Forked1", "S");

var factory2 = junction.IntoSpline("E", "SplineTemplate1")
    .SpeedLimit(1, 40)
    .SpeedLimit(2, 40);
factory2.Curves
    .ByRadius(-100, 35)
    .Straight(25)
    .ByRadius(100, 35);
var spline2 = factory2.Build();

var junction2 = spline2.IntoJunction("3Forked1", "E");

factory = junction.IntoSpline("N", "SplineTemplate1");
factory.Curves
    .Straight(20)
    .ByRadius(50, 50)
    .ByRadius(-50, 50)
    .Straight(200);
factory.PutStructure(["BusStop"], -5.25, 0.15, 0, 250, 0, 0, 1);
spline = factory.Build();

junction = spline.IntoJunction("3Forked1", "S");

factory = junction.IntoSpline("N", "SplineTemplate1");
factory.Curves.Straight(200);
spline = factory.Build();

factory = junction.IntoSpline("E", "SplineTemplate1")
    .SpeedLimit(1, 40)
    .SpeedLimit(2, 40);
factory.Curves
    .Straight(10)
    .ByRadius(50, 70)
    .Straight(120);
factory.Cants
    .Constant(10)
    .TransitionToPercent(5, 20)
    .Constant(50)
    .TransitionToPercent(0, 20);
factory.Gradients
    .TransitionByPercent(5, 5)
    .TransitionByPercent(-5, 5)
    .Constant(110)
    .TransitionByPercent(10, 30)
    .Constant(50)
    .TransitionByPercent(-10, 30);
factory.ConnectBezier(junction2.Junction.Ports["S"]);
spline = factory.Build();

factory = junction2.IntoSpline("N", "SplineTemplate1");
factory.Curves.Straight(30);
spline = factory.Build();

junction = spline.IntoJunction("DeadEnd1", "0");

factory = Plates[0, 0].BeginSpline("SplineTemplate1", 30, 0, 35);
factory.Curves.Straight(110);
factory.Gradients
    .Constant(30)
    .TransitionByDegree(180, 50);
spline = factory.Build();
