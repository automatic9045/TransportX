Vehicles.Load("Bus.Sample.dll");

Models.LoadList("Models.txt");
//Debug.ShowDialog("hello");

Network.LaneKinds.Create("Pedestrians", "歩行者");
Network.LaneKinds.Create("Buses", "バス");
Network.LaneKinds.Create("OtherCars", "その他自動車");
Network.LaneKinds.Add("Cars", "Buses+OtherCars");

Network.LaneLayouts.Load("Layout1", "LaneLayout1.xml");

var tSpline = Network.Templates.CreateSpline("SplineTemplate1", "Layout1");
tSpline.PutStructure(["Road1_1", "Road1_1", "Road1_1", "Road1_1", "Road1_2", "Road1_3", "Road1_3", "Road1_3", "Road1_3", "Road1_3"], 0, 0, 0, 0, 1, 1);
tSpline.PutStructure(["Road1_Sidewalk"], 0, 0, 0, 0, 1, 1);

tSpline = Network.Templates.CreateSpline("SplineTemplate2", "Layout1");
tSpline.PutStructure(["Road1_10m"], 0, 0, 0, 0, 10, 10);

var tJunction = Network.Templates.CreateJunction("JunctionTemplate1");
tJunction.AddPort("S", "Layout1", 0, 0, 0, 0, 180, 0);
tJunction.AddPort("N", "Layout1", 0, 0, 10, 0, 0, 0);
tJunction.AddPort("E", "Layout1", 5, 0, 5, 0, 90, 0);
tJunction.Wire("S", 0, "N", 3);
tJunction.Wire("S", 1, "N", 2);
tJunction.Wire("S", 2, "N", 1);
tJunction.Wire("S", 3, "N", 0);
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

Background.Add("Background");

Plates[0, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 0].PutStructure("Signal", 20, 0, 40);

Plates[-1, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 1].PutStructure("Grass", 0, -0.01, 0);

var factory = Plates[-1, 0].BeginSpline("SplineTemplate2", 245, 0, 30);
for (int i = 0; i < 5; i++) factory.Curves.Straight(250);
factory.PutStructure(["Pedestrian"], -5.5, 0.15, 0, 20, 0, 0, 1);
var spline = factory.Build();

factory = Plates[0, 0].BeginSpline("SplineTemplate1", 10, 0, 30);
factory.Curves.Straight(70);
spline = factory.Build();

var junction = spline.IntoJunction("JunctionTemplate1", "S");

var factory2 = junction.IntoSpline("E", "SplineTemplate1");
factory2.Curves.ByRadius(-100, 35);
factory2.Curves.Straight(25);
factory2.Curves.ByRadius(100, 35);
var spline2 = factory2.Build();

var junction2 = spline2.IntoJunction("JunctionTemplate1", "E");

factory = junction.IntoSpline("N", "SplineTemplate1");
factory.Curves.Straight(20);
factory.Curves.ByRadius(50, 50);
factory.Curves.ByRadius(-50, 50);
factory.Curves.Straight(200);
spline = factory.Build();

junction = spline.IntoJunction("JunctionTemplate1", "S");

factory = junction.IntoSpline("N", "SplineTemplate1");
factory.Curves.Straight(200);
spline = factory.Build();

factory = junction.IntoSpline("E", "SplineTemplate1");
factory.Curves.Straight(10);
factory.Curves.ByRadius(50, 70);
factory.Curves.Straight(120);
factory.Cants.Constant(10);
factory.Cants.TransitionToPercent(5, 20);
factory.Cants.Constant(50);
factory.Cants.TransitionToPercent(0, 20);
factory.Gradients.TransitionByPercent(5, 5);
factory.Gradients.TransitionByPercent(-5, 5);
factory.Gradients.Constant(110);
factory.Gradients.TransitionByPercent(10, 30);
factory.Gradients.Constant(50);
factory.Gradients.TransitionByPercent(-10, 30);
factory.ConnectBezier(junction2.Junction.Ports["S"]);
spline = factory.Build();

factory = junction2.IntoSpline("N", "SplineTemplate1");
factory.Curves.Straight(20);
spline = factory.Build();

junction = spline.IntoJunction("JunctionTemplate1", "S");

factory = junction.IntoSpline("N", "SplineTemplate1");
double radius = 10;
factory.Curves.Straight(radius - 5);
factory.Curves.ByRadius(radius, radius / 2 * 3 * double.Pi);
factory.Curves.Straight(radius - 6);
spline = factory.Build();
Network.Connect(spline.Outlet, junction.Junction.Ports["E"]);

factory = Plates[0, 0].BeginSpline("SplineTemplate1", 30, 0, 35);
factory.Curves.Straight(110);
factory.Gradients.Constant(30);
factory.Gradients.TransitionByDegree(180, 50);
spline = factory.Build();
