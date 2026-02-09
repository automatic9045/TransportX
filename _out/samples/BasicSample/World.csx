Avatar.Load("TransportX.Sample.dll");
Avatar.Locate(0, 0, -1, 0.2, 45, 0, 2, 0);

Models.LoadList("Models.txt");
//Debug.ShowDialog("hello");

Network.LaneTraffic.AddType("Pedestrians", "歩行者", "#FF00FF");
Network.LaneTraffic.AddType("Buses", "バス", "#00FF00");
Network.LaneTraffic.AddType("OtherCars", "その他自動車", "#0000FF");
Network.LaneTraffic.AddGroup("Cars", "Buses|OtherCars", "#00FFFF");

Network.LaneLayouts.Load("Layout1", "LaneLayout1.xml");

var tSpline = Network.Templates.CreateSpline("SplineTemplate1", "Layout1");
tSpline.PutStructure(["Road1_1", "Road1_1", "Road1_1", "Road1_1", "Road1_2", "Road1_3", "Road1_3", "Road1_3", "Road1_3", "Road1_3"], 0, 0, 0, 0, 1, 1);
tSpline.PutStructure(["Road1_Sidewalk"], 0, 0, 0, 0, 1, 1);

tSpline = Network.Templates.CreateSpline("SplineTemplate2", "Layout1");
tSpline.PutStructure(["Road1_10m"], 0, 0, 0, 0, 10, 10);

var tJunction = Network.Templates.CreateJunction("3Forked1");
tJunction.AddPort("S", "Layout1", 0, 0, 0, 0, 180, 0);
tJunction.AddPort("N", "Layout1", 0, 0, 10, 0, 0, 0);
tJunction.AddPort("E", "Layout1", 5, 0, 5, 0, 90, 0);
tJunction.Wire("S", 0, "N", 3).StraightToEnd();
tJunction.Wire("S", 1, "N", 2).StraightToEnd();
tJunction.Wire("S", 2, "N", 1).StraightToEnd();
tJunction.Wire("S", 3, "N", 0).StraightToEnd();
tJunction.Wire("S", 0, "E", 3);
tJunction.Wire("S", 1, "E", 2);
tJunction.Wire("S", 2, "E", 1);
tJunction.Wire("S", 3, "E", 0);
tJunction.Wire("N", 0, "E", 3);
tJunction.Wire("N", 1, "E", 2);
tJunction.Wire("N", 2, "E", 1);
tJunction.Wire("N", 3, "E", 0);
tJunction.PutStructure("Road1_3", 0, 0, 0);
tJunction.PutStructure("Road1_3", 0, 0, 1);
tJunction.PutStructure("Road1_3", 0, 0, 2);
tJunction.PutStructure("Road1_3", 0, 0, 3);
tJunction.PutStructure("Road1_3", 0, 0, 4);
tJunction.PutStructure("Road1_3", 0, 0, 5);
tJunction.PutStructure("Road1_3", 0, 0, 6);
tJunction.PutStructure("Road1_3", 0, 0, 7);
tJunction.PutStructure("Road1_3", 0, 0, 8);
tJunction.PutStructure("Road1_3", 0, 0, 9);

tJunction = Network.Templates.CreateJunction("DeadEnd1");
tJunction.AddPort("0", "Layout1", 0, 0, 0, 0, 180, 0);
var tJunctionPath = tJunction.Wire("0", 2, "0", 1);
tJunctionPath
    .StraightTo(-2, 0, 10, 0, 0, 0, out var s1)
    .BezierTo(5.5, 0, 10, 0, 180, 0, out var s2)
    .BezierToEnd();
tJunctionPath.Width
    .Constant(5)
    .TransitionTo(2.25, 2, s1 - 5)
    .Constant(s2 - s1);
tJunction.PutStructure("RoadDeadEnd1", 0, 0, 0);

Background.Add("Background");

Plates[-1, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[-1, 1].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 1].PutStructure("Grass", 0, -0.01, 0);

Plates[0, 0].PutStructure("Signal", 20, 0, 40);

var factory = Plates[-1, 0].BeginSpline("SplineTemplate2", 200, 0, 30);
for (int i = 0; i < 5; i++) factory.Curves.Straight(250);
factory.PutStructure(["Pedestrian"], -5.5, 0.15, 0, 20, 0, 0, 1);
var spline = factory.Build();

factory = Plates[0, 0].BeginSpline("SplineTemplate1", 10, 0, 0);
factory.Curves.Straight(100);
spline = factory.Build();

Plates[0, 0].PutStructure("RoadTerminal1", -1.7, 0, 30);
Plates[0, 0].PutStructure("BusStop", -4.5, 0.15, 49.3);

var junction = spline.IntoJunction("3Forked1", "S");

var factory2 = junction.IntoSpline("E", "SplineTemplate1");
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

factory = junction.IntoSpline("E", "SplineTemplate1");
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
