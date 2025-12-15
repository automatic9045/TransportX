Vehicles.Load("Bus.Sample.dll");

Models.LoadList("Models.txt");
//Debug.ShowDialog("hello");

Background.Add("Background");

Plates[0, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 0].PutStructure("Signal", 20, 0, 40);

Plates[0, 0].PutStructure("Car", 0, 0, 40);

Plates[-1, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 1].PutStructure("Grass", 0, -0.01, 0);

var spline = Plates[-1, 0].BeginSpline(245, 0, 30, 0, 0, 0);
for (int i = 0; i < 5; i++) spline.Straight(250);
spline.PutStructure(["Road1_10m"], 0, 0, 0, 0, 10, 10);

spline = Plates[0, 0].BeginSpline(10, 0, 30, 0, 0, 0);
spline.Straight(100);
spline.Curve(50, 50);
spline.Curve(-50, 50);
spline.Straight(200);
spline.Straight(200);
spline.PutStructure(["Road1_1", "Road1_1", "Road1_1", "Road1_1", "Road1_2", "Road1_3", "Road1_3", "Road1_3", "Road1_3", "Road1_3"], -2, 0, 0, 0, 1, 1);
spline.PutStructure(["Road1_Sidewalk"], -2, 0, 0, 0, 1, 1);

spline = Plates[0, 0].BeginSpline(60, 0, 50, 0, 0.2, 0);
spline.Straight(100);
spline.Curve(50, 50);
spline.Curve(-50, 50);
spline.Straight(100);
spline.PutStructure(["Road1_10m"], 0, 0, 0, 0, 10, 10);
