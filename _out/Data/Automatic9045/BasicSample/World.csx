#load "__Editor.csx"
#load "Init.csx"
#load "Templates/Load.csx"
{
    // <editor-generated version="1">
    Camera.Locate(-1, 0, 230, 8, 10, 0, 40, 0);
    Avatar.Load("../LV290/Avatar_LV290N_Konan.xml");
    Avatar.Locate(0, 0, -1, 0.2, 45, 0, 2, 0);
    Environment.SetDefault("Environment1.xml");
    DirectionalLight.SetColor("#FFFFFF");
    DirectionalLight.SetDirection(-1, -4, 2);
    DirectionalLight.SetIntensity(100000);
    Background.Add("Background");

    Chunks[-1, 0].PutProp("Grass", 0, -0.2, 0, 0, 0, 0);
    Chunks[0, 0].PutProp("Grass", 0, -0.2, 0, 0, 0, 0);
    Chunks[-1, 1].PutProp("Grass", 0, -0.2, 0, 0, 0, 0);
    Chunks[0, 1].PutProp("Grass", 0, -0.2, 0, 0, 0, 0);
    // <display name="一郷_方転場" />
    Chunks[0, 0].PutProp("RoadTerminal1", -1.7, 0, 30, 0, 0, 0);
    // <display name="一郷_標柱" />
    Chunks[0, 0].PutProp("BusStop_Ichigo", -4.5, 0, 49.3, 0, 0, 0);

    var fSpline = Chunks[0, 0].BeginSpline("Spline1", 10, 0, 0, 0, 0, 0);
    fSpline.Key = "spline_001";
    fSpline.Curves
        .Straight(100, out var s_s_end);
    fSpline.PutExtrusion("WhiteLineDashed150", 0, -0.12, 5, 0.01, 2, 5, 0, 0, s_s_end - 30);
    fSpline.PutExtrusion("WhiteLine150", 0, -0.12, 5, 0.01, 2, 1, 0, s_s_end - 30, double.MaxValue);
    fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 90, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax50"], 5.25, 0.6, 0, 0, 180, 0, 90, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 50).SpeedLimit(2, 50);
    // </user-prebuild>
    fSpline.Build();

    // <display name="一郷交差点" />
    var fJunction = Network.Splines["spline_001"].IntoJunction("4Forked1", "S");
    fJunction.Key = "Ichigo";
    fJunction.Build();

    fSpline = Network.Junctions["Ichigo"].IntoSpline("W", "Spline1");
    fSpline.Key = "spline_002";
    fSpline.Curves
        .Straight(10)
        .ByRadius(150, 50)
        .Straight(140);
    fSpline.SkipExtrusion("DikeL", 90, 126);
    fSpline.SkipExtrusion("RoadL", 90, 126);
    fSpline.SkipExtrusion("WhiteLineL", 91, 125);
    fSpline.SkipExtrusion("DikeR", 66, 102);
    fSpline.SkipExtrusion("RoadR", 66, 102);
    fSpline.SkipExtrusion("WhiteLineR", 67, 101);
    fSpline.PutExtrusion("WhiteLineDashed500", -4.175, -0.12, 5, 0.05, 1, 2, 0, 93, 125);
    fSpline.PutExtrusion("WhiteLineDashed500", 4.175, -0.12, 5, 0.05, 1, 2, 0, 69, 101);
    fSpline.PutExtrusion("WhiteLine150", 0, -0.12, 5, 0.05, 1, 1, 0, 0, 30);
    fSpline.PutExtrusion("WhiteLineDashed150", 0, -0.12, 5, 0.05, 1, 5, 0, 35, double.MaxValue);
    fSpline.PutProp(["Road1_BusBay_Dike"], 0, 0, 0, 0, 0, 0, 90, 36, 36, 1);
    fSpline.PutProp(["Road1_BusBay"], 0, 0, 0, 0, 0, 0, 90, 36, 36, 1);
    fSpline.PutProp(["BusStop_KendoSanda"], -10.5, 0, 0, 0, -90, 0, 109, 0, 0, 1);
    fSpline.PutProp(["Road1_BusBay_Dike"], 0, 0, 36, 0, 180, 0, 66, 36, 36, 1);
    fSpline.PutProp(["Road1_BusBay"], 0, 0, 36, 0, 180, 0, 66, 36, 36, 1);
    fSpline.PutProp(["BusStop_KendoSanda"], 10.5, 0, 0, 0, 90, 0, 83, 0, 0, 1);
    fSpline.PutProp(["StreetLight"], -5.5, 0, 0, 0, 0, 0, 5, 35, 35, 3);
    fSpline.PutProp(["StreetLight"], -8.5, 0, 0, 0, 0, 0, 110, 35, 35, 1);
    fSpline.PutProp(["StreetLight"], -5.5, 0, 0, 0, 0, 0, 145, 35, 35);
    fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 15, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 50, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 140, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 185, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 60).SpeedLimit(2, 60);
    // </user-prebuild>
    fSpline.Build();

    // <display name="四葉台入口交差点" />
    fJunction = Network.Splines["spline_002"].IntoJunction("3Forked2", "S");
    fJunction.Key = "YotsubadaiEnt";
    fJunction.Build();

    fSpline = Network.Junctions["YotsubadaiEnt"].IntoSpline("N", "Spline1");
    fSpline.Key = "spline_003";
    fSpline.Curves
        .Straight(50);
    fSpline.PutExtrusion("WhiteLineDashed150", 0, -0.12, 5, 0.05, 1, 5, 0, 5, double.MaxValue);
    fSpline.PutProp(["StreetLight"], -5.5, 0, 0, 0, 0, 0, 0, 35, 35);
    fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 10, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 60).SpeedLimit(2, 60);
    // </user-prebuild>
    fSpline.Build();

    fSpline = Network.Junctions["YotsubadaiEnt"].IntoSpline("E", "Spline2");
    fSpline.Key = "spline_004";
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
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    // <display name="四葉台南交差点" />
    fJunction = Network.Splines["spline_004"].IntoJunction("4Forked2", "S");
    fJunction.Key = "YotsubadaiS";
    fJunction.PutProp("BusStop_YotsubadaiIriguchi", -3.8, -0.12, 3, 0, -90, 0);
    fJunction.Build();

    fSpline = Network.Junctions["YotsubadaiS"].IntoSpline("E", "Spline2");
    fSpline.Key = "spline_005";
    fSpline.Curves
        .Straight(10)
        .ByRadius(-10, 15.7075)
        .Straight(30);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    // <display name="四葉台東交差点" />
    fJunction = Network.Splines["spline_005"].IntoJunction("3Forked3", "N");
    fJunction.Key = "YotsubadaiE";
    fJunction.PutProp("RoadSign_C", -4, 0, 1, 0, 0, 0);
    fJunction.PutProp("RoadSign_ExceptBusses", -4, 0, 1, 0, 0, 0);
    fJunction.PutProp("RoadSign_C", 4, 0, 15, 0, 180, 0);
    fJunction.PutProp("RoadSign_NoEntry", 6.5, 0, 12, 0, 90, 0);
    fJunction.PutProp("RoadSign_ExceptBusses", 6.5, 0, 12, 0, 90, 0);
    fJunction.PutProp("RoadSign_End", 7, 0, 4.5, 0, 180, 0);
    fJunction.PutProp("RoadSign_OneWayR", 7, 0, 4.5, 0, 180, 0);
    fJunction.PutProp("RoadSign_ExceptBusses", 7, 0, 4.5, 0, 180, 0);
    fJunction.Paths["NE_L"].TrafficDensity(0);
    fJunction.Paths["SE_R"].TrafficDensity(0);
    fJunction.Build();

    fSpline = Network.Junctions["YotsubadaiS"].IntoSpline("W", "Spline2");
    fSpline.Key = "spline_006";
    fSpline.Curves
        .Straight(10)
        .ByRadius(10, 15.7075)
        .Straight(30);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    // <display name="四葉台西交差点" />
    fJunction = Network.Splines["spline_006"].IntoJunction("3Forked3", "S");
    fJunction.Key = "YotsubadaiW";
    fJunction.Build();

    fSpline = Network.Junctions["YotsubadaiS"].IntoSpline("N", "Spline2");
    fSpline.Key = "spline_007";
    fSpline.Curves
        .Straight(32);
    fSpline.PutProp(["RoadSign_NoParking"], -4, 0, 0, 0, 0, 0, 8, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax30"], -4, 0.6, 0, 0, 0, 0, 8, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], 4, 0, 0, 0, 180, 0, 24, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax30"], 4, 0.6, 0, 0, 180, 0, 24, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    // <display name="四葉台中央交差点" />
    fJunction = Network.Splines["spline_007"].IntoJunction("4Forked2", "S");
    fJunction.Key = "YotsubadaiC";
    fJunction.PutProp("RoadSign_CL", -4, 0, 1, 0, 0, 0);
    fJunction.PutProp("RoadSign_CL", 4, 0, 15, 0, 180, 0);
    fJunction.PutProp("RoadSign_OneWayR", 7, 0, 11.5, 0, 0, 0);
    fJunction.PutProp("RoadSign_ExceptBusses", 7, 0, 11.5, 0, 0, 0);
    fJunction.Paths["SE_R"].TrafficDensity(0);
    fJunction.Paths["NW_R"].TrafficDensity(0);
    fJunction.Build();

    // <display name="四葉台バス停部" />
    fSpline = Network.Junctions["YotsubadaiC"].IntoSpline("E", "Spline2");
    fSpline.Key = "spline_008";
    fSpline.ConnectBezier(Network.Junctions["YotsubadaiE"].Junction.Ports["E"]);
    fSpline.PutProp(["BusStop_Yotsubadai"], 3.8, -0.01, 0, 0, 90, 0, 5, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    fSpline = Network.Junctions["YotsubadaiC"].IntoSpline("W", "Spline2");
    fSpline.Key = "spline_009";
    fSpline.ConnectBezier(Network.Junctions["YotsubadaiW"].Junction.Ports["E"]);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    fSpline = Network.Junctions["YotsubadaiC"].IntoSpline("N", "Spline2");
    fSpline.Key = "spline_010";
    fSpline.Curves
        .Straight(32);
    fSpline.PutProp(["RoadSign_NoParking"], -4, 0, 0, 0, 0, 0, 8, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax30"], -4, 0.6, 0, 0, 0, 0, 8, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], 4, 0, 0, 0, 180, 0, 24, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax30"], 4, 0.6, 0, 0, 180, 0, 24, 0, 0, 1);
    fSpline.PutProp(["BusStop_YotsubadaiKita"], -3.4, -0.12, 0, 0, -90, 0, 31, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    // <display name="四葉台北交差点" />
    fJunction = Network.Splines["spline_010"].IntoJunction("4Forked2", "S");
    fJunction.Key = "YotsubadaiN";
    fJunction.Build();

    fSpline = Network.Junctions["YotsubadaiE"].IntoSpline("S", "Spline2");
    fSpline.Key = "spline_011";
    fSpline.Curves
        .Straight(30)
        .ByRadius(-10, 15.7075)
        .Straight(5);
    fSpline.ConnectBezier(Network.Junctions["YotsubadaiN"].Junction.Ports["E"]);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    fSpline = Network.Junctions["YotsubadaiW"].IntoSpline("N", "Spline2");
    fSpline.Key = "spline_012";
    fSpline.Curves
        .Straight(30)
        .ByRadius(10, 15.7075)
        .Straight(5);
    fSpline.ConnectBezier(Network.Junctions["YotsubadaiN"].Junction.Ports["W"]);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    fSpline = Network.Junctions["YotsubadaiN"].IntoSpline("N", "Spline2");
    fSpline.Key = "spline_013";
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
    // <user-prebuild>
    fSpline.SpeedLimit(1, 30).SpeedLimit(2, 30);
    // </user-prebuild>
    fSpline.Build();

    fSpline = Network.Junctions["Ichigo"].IntoSpline("E", "Spline1");
    fSpline.Key = "spline_014";
    fSpline.Curves
        .ByRadius(-100, 35)
        .Straight(25)
        .ByRadius(100, 35);
    fSpline.PutExtrusion("YellowLine150", 0, -0.12, 5, 0.05, 1, 1, 0, 0, double.MaxValue);
    fSpline.PutProp(["RoadSign_NoParkingStopping"], -5.25, 0, 0, 0, 0, 0, 10, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax40"], -5.25, 0.6, 0, 0, 0, 0, 10, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParkingStopping"], 5.25, 0, 0, 0, 180, 0, 93, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax40"], 5.25, 0.6, 0, 0, 180, 0, 93, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 40).SpeedLimit(2, 40);
    // </user-prebuild>
    fSpline.Build();

    // <display name="本一郷交差点" />
    fJunction = Network.Splines["spline_014"].IntoJunction("3Forked1", "E");
    fJunction.Key = "Motoichigo";
    fJunction.Build();

    fSpline = Network.Junctions["Motoichigo"].IntoSpline("N", "Spline1");
    fSpline.Key = "spline_018";
    fSpline.Curves
        .Straight(30);
    fSpline.PutExtrusion("WhiteLine150", 0, -0.12, 5, 0.01, 2, 5, 0, 0, double.MaxValue);
    fSpline.Build();

    fJunction = Network.Splines["spline_018"].IntoJunction("DeadEnd1", "0");
    fJunction.Key = "junction_010";
    fJunction.Build();

    fSpline = Network.Junctions["Ichigo"].IntoSpline("N", "Spline1");
    fSpline.Key = "spline_015";
    fSpline.Curves
        .Straight(20)
        .ByRadius(50, 50)
        .ByRadius(-50, 50)
        .Straight(50)
        .Straight(150, out var s_s_2);
    fSpline.PutExtrusion("WhiteLine150", 0, -0.12, 5, 0.05, 1, 1, 0, 0, 30);
    fSpline.PutExtrusion("WhiteLineDashed150", 0, -0.12, 5, 0.05, 1, 5, 0, 35, s_s_2 - 30);
    fSpline.PutExtrusion("WhiteLine150", 0, -0.12, 5, 0.05, 1, 1, 0, s_s_2 - 30, double.MaxValue);
    fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 20, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax50"], -5.25, 0.6, 0, 0, 0, 0, 20, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 110, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax50"], 5.25, 0.6, 0, 0, 180, 0, 110, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 170, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax50"], -5.25, 0.6, 0, 0, 0, 0, 170, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 300, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax50"], 5.25, 0.6, 0, 0, 180, 0, 300, 0, 0, 1);
    fSpline.PutProp(["BusStop_Futagawa"], -5.25, 0, 0, 0, 0, 0, 250, 0, 0, 1);
    fSpline.PutProp(["BusStop_Futagawa"], 5.25, 0, 0, 0, 180, 0, 220, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 50).SpeedLimit(2, 50);
    // </user-prebuild>
    fSpline.Build();

    // <display name="二川交差点" />
    fJunction = Network.Splines["spline_015"].IntoJunction("3Forked1", "S");
    fJunction.Key = "Futagawa";
    fJunction.Build();

    fSpline = Network.Junctions["Futagawa"].IntoSpline("N", "Spline1");
    fSpline.Key = "spline_016";
    fSpline.Curves
        .Straight(50);
    fSpline.PutExtrusion("WhiteLine150", 0, -0.12, 5, 0.05, 1, 1, 0, 0, 30);
    fSpline.PutExtrusion("WhiteLineDashed150", 0, -0.12, 5, 0.05, 1, 5, 0, 35, double.MaxValue);
    fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 20, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax50"], -5.25, 0.6, 0, 0, 0, 0, 20, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 50).SpeedLimit(2, 50);
    // </user-prebuild>
    fSpline.Build();

    fSpline = Network.Junctions["Futagawa"].IntoSpline("E", "Spline1");
    fSpline.Key = "spline_017";
    fSpline.Curves
        .Straight(10)
        .ByRadius(50, 70)
        .Straight(120);
    fSpline.Gradients
        .TransitionByPercent(5, 5)
        .TransitionByPercent(-5, 5)
        .Constant(110)
        .TransitionByPercent(10, 30)
        .Constant(50)
        .TransitionByPercent(-10, 30);
    fSpline.Cants
        .Constant(10)
        .TransitionToPercent(5, 20)
        .Constant(50)
        .TransitionToPercent(0, 20);
    fSpline.ConnectBezier(Network.Junctions["Motoichigo"].Junction.Ports["S"], out var s_s_3);
    fSpline.PutExtrusion("WhiteLine150", 0, -0.12, 5, 0.05, 1, 1, 0, 0, 30);
    fSpline.PutExtrusion("WhiteLineDashed150", 0, -0.12, 5, 0.05, 1, 5, 0, 35, s_s_3 - 31);
    fSpline.PutExtrusion("WhiteLine150", 0, -0.12, 5, 0.05, 1, 1, 0, s_s_3 - 31, double.MaxValue);
    fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 5, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax40"], -5.25, 0.6, 0, 0, 0, 0, 5, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 160, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax40"], 5.25, 0.6, 0, 0, 180, 0, 160, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], -5.25, 0, 0, 0, 0, 0, 180, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax40"], -5.25, 0.6, 0, 0, 0, 0, 180, 0, 0, 1);
    fSpline.PutProp(["RoadSign_NoParking"], 5.25, 0, 0, 0, 180, 0, 325, 0, 0, 1);
    fSpline.PutProp(["RoadSign_SpeedMax40"], 5.25, 0.6, 0, 0, 180, 0, 325, 0, 0, 1);
    // <user-prebuild>
    fSpline.SpeedLimit(1, 40).SpeedLimit(2, 40);
    // </user-prebuild>
    fSpline.Build();

    fSpline = Chunks[-1, 0].BeginSpline("Spline2", 15, 0, 5, 0, 0, 0);
    fSpline.Key = "spline_019";
    fSpline.Curves
        .Straight(110);
    fSpline.Gradients
        .Constant(30)
        .TransitionByDegree(180, 50);
    fSpline.SkipExtrusion("DikeL");
    fSpline.SkipExtrusion("DikeR");
    fSpline.Build();

    // <display name="断面押し出しによる道路モデル設置" />
    fSpline = Chunks[-1, 0].BeginSpline("Spline1", 140, 0, 20, 0, 0, 0);
    fSpline.Key = "ModelTest_Extrusion";
    fSpline.Curves
        .Straight(10)
        .ByRadius(20, 25);
    fSpline.SkipExtrusion("DikeL");
    fSpline.SkipExtrusion("DikeR");
    fSpline.PutExtrusion("Ext1", "WhiteLineDashed150", 0, -0.12, 5, 0.01, 2, 5, 0, 0, double.MaxValue);
    fSpline.Build();

    // <display name="プロップによる道路モデル設置" />
    fSpline = Chunks[-1, 0].BeginSpline(null, 110, 0, 20, 0, 0, 0);
    fSpline.Key = "ModelTest_Prop";
    fSpline.Curves
        .Straight(10)
        .ByRadius(20, 25);
    fSpline.PutProp("Prop1", ["Road1_StraightL"], 0, 0, 0, 0, 0, 0, 0, 1.2, 1.2, 100);
    fSpline.PutProp("Prop2", ["Road1_StraightR"], 0, 0, 0, 0, 0, 0, 0, 1.2, 1.2, 100);
    fSpline.PutProp("Prop5", ["WhiteLine150"], -4, -0.12, 0, 0, 0, 0, 0, 1, 1, 100);
    fSpline.PutProp("Prop6", ["WhiteLine150"], 4, -0.12, 0, 0, 0, 0, 0, 1, 1, 100);
    fSpline.PutProp("Prop7", ["WhiteLine150", "WhiteLine150", "WhiteLine150", "WhiteLine150", "WhiteLine150", "", "", "", "", ""], 0, -0.12, 0, 0, 0, 0, 0, 1, 1, 100);
    fSpline.Build();
    // </editor-generated>
}
