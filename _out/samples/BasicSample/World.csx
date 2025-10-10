Models.LoadList("Models.txt");
//Debug.ShowDialog("hello");

Background.Add("Background");

Plates[0, 0].PutStructure("Grass", 0, -0.01, 0);
Plates[0, 0].PutStructure("Signal", 120, 0, 150);

Plates[0, 1].PutStructure("Grass", 0, -0.01, 0);

var spline = Plates[0, 0].BeginSpline(110, 0, 50, 0, 0.2, 0);
spline.Straight(100);
spline.Curve(50, 50);
spline.Curve(-50, 50);
spline.Straight(100);
spline.PutStructure(["Road1_10m"], -2, 0, 10, 0, 10, 10);

spline = Plates[0, 0].BeginSpline(10, 0, 30, 0, 0.1, 0);
spline.Straight(100);
spline.Curve(50, 50);
spline.Curve(-50, 50);
spline.Straight(200);
spline.Straight(200);
//spline.PutStructure(["Road1_1", "Road1_1", "Road1_1", "Road1_1", "Road1_2", "Road1_3", "Road1_3", "Road1_3", "Road1_3", "Road1_3"], -2, 0, 1, 0, 1, 1);
//spline.PutStructure(["Road1_Sidewalk"], -2, 0, 1, 0, 1, 1);
spline.PutStructure(["Road1_10m"], -2, 0, 10, 0, 10, 10);
