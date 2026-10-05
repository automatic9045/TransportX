#load "__Editor.csx"

#load "Init.csx"
#load "Templates.csx"

//Debug.ShowDialog("hello");

Camera.Locate(-1, 0, 230, 8, 10, 0, 40, 0);

Avatar.Load(@"..\LV290\Avatar_LV290N.xml");
Avatar.Locate(0, 0, -1, 0.2, 45, 0, 2, 0);

Environment.SetDefault("Environment1.xml");

DirectionalLight.SetColor("#FFFFFF");
DirectionalLight.SetDirection(-1, -4, 2);
DirectionalLight.SetIntensity(100000);

Background.Add("Background");

Chunks[-1, 0].PutProp("Grass", 0, -0.2, 0);
Chunks[0, 0].PutProp("Grass", 0, -0.2, 0);
Chunks[-1, 1].PutProp("Grass", 0, -0.2, 0);
Chunks[0, 1].PutProp("Grass", 0, -0.2, 0);

string[] centerLine = ["WhiteLine150", "WhiteLine150", "WhiteLine150", "WhiteLine150", "WhiteLine150", "", "", "", "", ""];

var fSpline = Chunks[0, 0].BeginSpline("Spline1", 10, 0, 0)
    .SpeedLimit(1, 50)
    .SpeedLimit(2, 50);
fSpline.Curves.Straight(100);
fSpline.PutProp(centerLine, 0, -0.12, 0, 0, 1, 1);
fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 90, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax50"], 5.25, 0.6, 0, 0, 180, 0, 90, 0, 0, 1);
var spline = fSpline.Build();

Chunks[0, 0].PutProp("RoadTerminal1", -1.7, 0, 30);
Chunks[0, 0].PutProp("BusStop_Ichigo", -4.5, 0, 49.3); // 一郷

var fJunction = spline.IntoJunction("4Forked1", "S");
var junction = fJunction.Build();

fSpline = junction.IntoSpline("W", "Spline1_Empty")
    .SpeedLimit(1, 60)
    .SpeedLimit(2, 60);
fSpline.Curves
    .Straight(10)
    .ByRadius(150, 50)
    .Straight(140);
fSpline.PutProp(["Road1_Straight_DikeL"], 0, 0, 0, 0, 1.2, 1.2, 75);
fSpline.PutProp(["Road1_StraightL"], 0, 0, 0, 0, 1.2, 1.2, 75);
fSpline.PutProp(["Road1_BusBay_Dike"], 0, 0, 0, 90, 36, 36, 1);
fSpline.PutProp(["Road1_BusBay"], 0, 0, 0, 90, 36, 36, 1);
fSpline.PutProp(["Road1_Straight_DikeL"], 0, 0, 0, 126, 1.2, 1.2);
fSpline.PutProp(["Road1_StraightL"], 0, 0, 0, 126, 1.2, 1.2);
fSpline.PutProp(["WhiteLine150"], -4, -0.12, 0, 0, 1, 0.9, 101);
fSpline.PutProp(["WhiteLine150"], -4, -0.12, 0, 125, 1, 0.9);
fSpline.PutProp(["WhiteLine500", "WhiteLine500", "", ""], -4.175, -0.12, 0, 93.5, 1, 1, 32);
fSpline.PutProp(["BusStop_KendoSanda"], -10.5, 0, 0, 0, -90, 0, 109, 0, 0, 1); // 県道三田 (四葉台方面)
fSpline.PutProp(["Road1_Straight_DikeR"], 0, 0, 0, 0, 1.2, 1.2, 55);
fSpline.PutProp(["Road1_StraightR"], 0, 0, 0, 0, 1.2, 1.2, 55);
fSpline.PutProp(["Road1_BusBay_Dike"], 0, 0, 36, 0, 180, 0, 66, 36, 36, 1);
fSpline.PutProp(["Road1_BusBay"], 0, 0, 36, 0, 180, 0, 66, 36, 36, 1);
fSpline.PutProp(["Road1_Straight_DikeR"], 0, 0, 0, 102, 1.2, 1.2);
fSpline.PutProp(["Road1_StraightR"], 0, 0, 0, 102, 1.2, 1.2);
fSpline.PutProp(["WhiteLine150"], 4, -0.12, 0, 0, 1, 0.9, 74);
fSpline.PutProp(["WhiteLine150"], 4, -0.12, 0, 101, 1, 0.9);
fSpline.PutProp(["WhiteLine500", "WhiteLine500", "", ""], 4.175, -0.12, 0, 69.5, 1, 1, 32);
fSpline.PutProp(["BusStop_KendoSanda"], 10.5, 0, 0, 0, 90, 0, 83, 0, 0, 1); // 県道三田 (一橋方面)
fSpline.PutProp(centerLine, 0, -0.12, 0, 0, 1, 1);
fSpline.PutProp(["StreetLight"], -5.5, 0, 0, 5, 35, 35, 3);
fSpline.PutProp(["StreetLight"], -8.5, 0, 0, 110, 35, 35, 1);
fSpline.PutProp(["StreetLight"], -5.5, 0, 0, 145, 35, 35);
fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 15, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 50, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 140, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 185, 0, 0, 1);
spline = fSpline.Build();

fJunction = spline.IntoJunction("3Forked2", "S");
var junction2 = fJunction.Build();

fSpline = junction2.IntoSpline("N", "Spline1")
    .SpeedLimit(1, 60)
    .SpeedLimit(2, 60);
fSpline.Curves
    .Straight(50);
fSpline.PutProp(centerLine, 0, -0.12, 0, 0, 1, 1);
fSpline.PutProp(["StreetLight"], -5.5, 0, 0, 0, 35, 35);
fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 10, 0, 0, 1);
spline = fSpline.Build();

fSpline = junction2.IntoSpline("E", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.Curves
    .Straight(20)
    .ByRadius(-100, 20)
    .Straight(10)
    .ByRadius(60, 30)
    .Straight(10);
fSpline.Gradients
    .Constant(40)
    .TransitionByPercent(5, 10)
    .Constant(30)
    .TransitionByPercent(-5, 10);
fSpline.PutProp(["RoadSign_NoParking"], -4, 0, 0, 0, 0, 0, 2, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax30"], -4, 0.6, 0, 0, 0, 0, 2, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], -4, 0, 0, 0, 0, 0, 70, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 4, 0, 0, 0, 180, 0, 88, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax30"], 4, 0.6, 0, 0, 180, 0, 88, 0, 0, 1);
spline = fSpline.Build();

fJunction = spline.IntoJunction("4Forked2", "S");
fJunction.PutProp("BusStop_YotsubadaiIriguchi", -3.8, -0.12, 3, 0, -90, 0); // 四葉台入口
junction2 = fJunction.Build();

fSpline = junction2.IntoSpline("E", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.Curves
    .Straight(10)
    .ByRadius(-10, 10 * 3.1415 / 2)
    .Straight(30);
fSpline.PutProp(["Road1_Straight_DikeR"], -4, 0, 0, 10.2, 1.2, 1.2, 13);
fSpline.PutProp(["Road2_StraightR"], 0, 0, 0, 10.2, 1.2, 1.2, 13);
fSpline.PutProp(["WhiteLine150"], 2.8, -0.12, 0, 9.45, 1, 0.9, 18);
spline = fSpline.Build();

fJunction = spline.IntoJunction("3Forked3", "N");
fJunction.Paths["NE_L"].TrafficDensity(0);
fJunction.Paths["SE_R"].TrafficDensity(0);
fJunction.PutProp("RoadSign_C", -4, 0, 1, 0, 0, 0);
fJunction.PutProp("RoadSign_ExceptBusses", -4, 0, 1, 0, 0, 0);
fJunction.PutProp("RoadSign_C", 4, 0, 15, 0, 180, 0);
fJunction.PutProp("RoadSign_NoEntry", 6.5, 0, 12, 0, 90, 0);
fJunction.PutProp("RoadSign_ExceptBusses", 6.5, 0, 12, 0, 90, 0);
fJunction.PutProp("RoadSign_End", 7, 0, 4.5, 0, 180, 0);
fJunction.PutProp("RoadSign_OneWayR", 7, 0, 4.5, 0, 180, 0);
fJunction.PutProp("RoadSign_ExceptBusses", 7, 0, 4.5, 0, 180, 0);
var junction3 = fJunction.Build();

fSpline = junction2.IntoSpline("W", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.Curves
    .Straight(10)
    .ByRadius(10, 10 * 3.1415 / 2)
    .Straight(30);
fSpline.PutProp(["Road1_Straight_DikeL"], 4, 0, 0, 10.2, 1.2, 1.2, 13);
fSpline.PutProp(["Road2_StraightL"], 0, 0, 0, 10.2, 1.2, 1.2, 13);
fSpline.PutProp(["WhiteLine150"], -2.8, -0.12, 0, 9.45, 1, 0.9, 18);
spline = fSpline.Build();

fJunction = spline.IntoJunction("3Forked3", "S");
var junction4 = fJunction.Build();

fSpline = junction2.IntoSpline("N", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.Curves
    .Straight(32);
fSpline.PutProp(["RoadSign_NoParking"], -4, 0, 0, 0, 0, 0, 8, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax30"], -4, 0.6, 0, 0, 0, 0, 8, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 4, 0, 0, 0, 180, 0, 24, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax30"], 4, 0.6, 0, 0, 180, 0, 24, 0, 0, 1);
spline = fSpline.Build();

fJunction = spline.IntoJunction("4Forked2", "S");
fJunction.Paths["SE_R"].TrafficDensity(0);
fJunction.Paths["NW_R"].TrafficDensity(0);
fJunction.PutProp("RoadSign_CL", -4, 0, 1, 0, 0, 0);
fJunction.PutProp("RoadSign_CL", 4, 0, 15, 0, 180, 0);
fJunction.PutProp("RoadSign_OneWayR", 7, 0, 11.5, 0, 0, 0);
fJunction.PutProp("RoadSign_ExceptBusses", 7, 0, 11.5, 0, 0, 0);
junction2 = fJunction.Build();

fSpline = junction2.IntoSpline("E", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.ConnectBezier(junction3.Junction.Ports["E"]);
fSpline.PutProp(["BusStop_Yotsubadai"], 3.8, -0.01, 0, 0, 90, 0, 5, 0, 0, 1); // 四葉台
spline = fSpline.Build();

fSpline = junction2.IntoSpline("W", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.ConnectBezier(junction4.Junction.Ports["E"]);
spline = fSpline.Build();

fSpline = junction2.IntoSpline("N", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.Curves
    .Straight(32);
fSpline.PutProp(["RoadSign_NoParking"], -4, 0, 0, 0, 0, 0, 8, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax30"], -4, 0.6, 0, 0, 0, 0, 8, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 4, 0, 0, 0, 180, 0, 24, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax30"], 4, 0.6, 0, 0, 180, 0, 24, 0, 0, 1);
fSpline.PutProp(["BusStop_YotsubadaiKita"], -3.4, -0.12, 0, 0, -90, 0, 31, 0, 0, 1); // 四葉台北
spline = fSpline.Build();

fJunction = spline.IntoJunction("4Forked2", "S");
junction2 = fJunction.Build();

fSpline = junction3.IntoSpline("S", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.Curves
    .Straight(30)
    .ByRadius(-10, 10 * 3.1415 / 2)
    .Straight(5);
fSpline.ConnectBezier(junction2.Junction.Ports["E"]);
fSpline.PutProp(["Road1_Straight_DikeR"], -4, 0, 0, 29.4, 1.2, 1.2, 14);
fSpline.PutProp(["Road2_StraightR"], 0, 0, 0, 29.4, 1.2, 1.2, 14);
fSpline.PutProp(["WhiteLine150"], 2.8, -0.12, 0, 29.25, 1, 0.9, 18);
spline = fSpline.Build();

fSpline = junction4.IntoSpline("N", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.Curves
    .Straight(30)
    .ByRadius(10, 10 * 3.1415 / 2)
    .Straight(5);
fSpline.ConnectBezier(junction2.Junction.Ports["W"]);
fSpline.PutProp(["Road1_Straight_DikeL"], 4, 0, 0, 29.4, 1.2, 1.2, 14);
fSpline.PutProp(["Road2_StraightL"], 0, 0, 0, 29.4, 1.2, 1.2, 14);
fSpline.PutProp(["WhiteLine150"], -2.8, -0.12, 0, 29.25, 1, 0.9, 18);
spline = fSpline.Build();

fSpline = junction2.IntoSpline("N", "Spline2")
    .SpeedLimit(1, 30)
    .SpeedLimit(2, 30);
fSpline.Curves
    .Straight(10)
    .ByRadius(-70, 70)
    .Straight(10)
    .ByRadius(200, 50)
    .Straight(20);
fSpline.Gradients
    .Constant(5)
    .TransitionByPercent(-2.5, 10)
    .Constant(75)
    .TransitionByPercent(2.5, 10);
fSpline.PutProp(["RoadSign_NoParking"], -4, 0, 0, 0, 0, 0, 5, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax30"], -4, 0.6, 0, 0, 0, 0, 5, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 4, 0, 0, 0, 180, 0, 20, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], -4, 0, 0, 0, 0, 0, 80, 0, 0, 1);
spline = fSpline.Build();

fSpline = junction.IntoSpline("E", "Spline1")
    .SpeedLimit(1, 40)
    .SpeedLimit(2, 40);
fSpline.Curves
    .ByRadius(-100, 35)
    .Straight(25)
    .ByRadius(100, 35);
fSpline.PutProp(centerLine, 0, -0.12, 0, 0, 1, 1);
fSpline.PutProp(["RoadSign_NoParkingStopping"], -5.25, 0, 0, 0, 0, 0, 10, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax40"], -5.25, 0.6, 0, 0, 0, 0, 10, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParkingStopping"], 5.25, 0, 0, 0, 180, 0, 93, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax40"], 5.25, 0.6, 0, 0, 180, 0, 93, 0, 0, 1);
spline = fSpline.Build();

fJunction = spline.IntoJunction("3Forked1", "E");
junction2 = fJunction.Build();

fSpline = junction.IntoSpline("N", "Spline1")
    .SpeedLimit(1, 50)
    .SpeedLimit(2, 50);
fSpline.Curves
    .Straight(20)
    .ByRadius(50, 50)
    .ByRadius(-50, 50)
    .Straight(200);
fSpline.PutProp(centerLine, 0, -0.12, 0, 0, 1, 1);
fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 20, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax50"], -5.25, 0.6, 0, 0, 0, 0, 20, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 110, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax50"], 5.25, 0.6, 0, 0, 180, 0, 110, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 170, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax50"], -5.25, 0.6, 0, 0, 0, 0, 170, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 300, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax50"], 5.25, 0.6, 0, 0, 180, 0, 300, 0, 0, 1);
fSpline.PutProp(["BusStop_Futagawa"], -5.25, 0, 0, 250, 0, 0, 1); // 二川 (四葉台方面)
fSpline.PutProp(["BusStop_Futagawa"], 5.25, 0, 0, 0, 180, 0, 220, 0, 0, 1); // 二川 (一郷方面)
spline = fSpline.Build();

fJunction = spline.IntoJunction("3Forked1", "S");
junction = fJunction.Build();

fSpline = junction.IntoSpline("N", "Spline1")
    .SpeedLimit(1, 50)
    .SpeedLimit(2, 50);
fSpline.Curves.Straight(50);
fSpline.PutProp(centerLine, 0, -0.12, 0, 0, 1, 1);
fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 20, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax50"], -5.25, 0.6, 0, 0, 0, 0, 20, 0, 0, 1);
spline = fSpline.Build();

fSpline = junction.IntoSpline("E", "Spline1")
    .SpeedLimit(1, 40)
    .SpeedLimit(2, 40);
fSpline.Curves
    .Straight(10)
    .ByRadius(50, 70)
    .Straight(120);
fSpline.Cants
    .Constant(10)
    .TransitionToPercent(5, 20)
    .Constant(50)
    .TransitionToPercent(0, 20);
fSpline.Gradients
    .TransitionByPercent(5, 5)
    .TransitionByPercent(-5, 5)
    .Constant(110)
    .TransitionByPercent(10, 30)
    .Constant(50)
    .TransitionByPercent(-10, 30);
fSpline.ConnectBezier(junction2.Junction.Ports["S"]);
fSpline.PutProp(centerLine, 0, -0.12, 0, 0, 1, 1);
fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 5, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax40"], -5.25, 0.6, 0, 0, 0, 0, 5, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 160, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax40"], 5.25, 0.6, 0, 0, 180, 0, 160, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 180, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax40"], -5.25, 0.6, 0, 0, 0, 0, 180, 0, 0, 1);
fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 325, 0, 0, 1);
fSpline.PutProp(["RoadSign_SpeedMax40"], 5.25, 0.6, 0, 0, 180, 0, 325, 0, 0, 1);
spline = fSpline.Build();

fSpline = junction2.IntoSpline("N", "Spline1");
fSpline.Curves.Straight(30);
fSpline.PutProp(centerLine, 0, -0.12, 0, 0, 1, 1);
spline = fSpline.Build();

fJunction = spline.IntoJunction("DeadEnd1", "0");
junction = fJunction.Build();

fSpline = Chunks[-1, 0].BeginSpline("Spline2_Empty", 15, 0, 5);
fSpline.Curves.Straight(110);
fSpline.Gradients
    .Constant(30)
    .TransitionByDegree(180, 50);
fSpline.PutProp(["Road2_StraightL"], 0, 0, 0, 0, 1.2, 1.2);
fSpline.PutProp(["Road2_StraightR"], 0, 0, 0, 0, 1.2, 1.2);
spline = fSpline.Build();
