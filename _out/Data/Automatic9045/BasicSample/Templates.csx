#load "__Editor.csx"

// --------------------------------------------------
// 直線・曲線道路 1
{
    var tSpline = Network.Templates.CreateSpline("Spline1", "Layout1")
        .SpeedLimit(1, 60)
        .SpeedLimit(2, 60);
    tSpline.PutProp(["Road1_Straight_DikeL"], 0, 0, 0, 0, 1.2, 1.2);
    tSpline.PutProp(["Road1_Straight_DikeR"], 0, 0, 0, 0, 1.2, 1.2);
    tSpline.PutProp(["Road1_StraightL"], 0, 0, 0, 0, 1.2, 1.2);
    tSpline.PutProp(["Road1_StraightR"], 0, 0, 0, 0, 1.2, 1.2);
    tSpline.PutProp(["WhiteLine150"], -4, -0.12, 0, 0, 1, 0.9);
    tSpline.PutProp(["WhiteLine150"], 4, -0.12, 0, 0, 1, 0.9);
}

// --------------------------------------------------
// 直線・曲線道路 1 (プロップ無し)
{
    var tSpline = Network.Templates.CreateSpline("Spline1_Empty", "Layout1")
        .SpeedLimit(1, 60)
        .SpeedLimit(2, 60);
}

// --------------------------------------------------
// 直線・曲線道路 2
{
    var tSpline = Network.Templates.CreateSpline("Spline2", "Layout2")
        .SpeedLimit(1, 60)
        .SpeedLimit(2, 60);
    tSpline.PutProp(["Road1_Straight_DikeL"], 4, 0, 0, 0, 1.2, 1.2);
    tSpline.PutProp(["Road1_Straight_DikeR"], -4, 0, 0, 0, 1.2, 1.2);
    tSpline.PutProp(["Road2_StraightL"], 0, 0, 0, 0, 1.2, 1.2);
    tSpline.PutProp(["Road2_StraightR"], 0, 0, 0, 0, 1.2, 1.2);
    tSpline.PutProp(["WhiteLine150"], -2.8, -0.12, 0, 0, 1, 0.9);
    tSpline.PutProp(["WhiteLine150"], 2.8, -0.12, 0, 0, 1, 0.9);
}

// --------------------------------------------------
// 直線・曲線道路 2 (プロップ無し)
{
    var tSpline = Network.Templates.CreateSpline("Spline2_Empty", "Layout2")
        .SpeedLimit(1, 60)
        .SpeedLimit(2, 60);
}

// --------------------------------------------------
// 三叉路 1
{
    var tJunction = Network.Templates.CreateJunction("3Forked1")
        .SignalController("4Forked1");
    tJunction.AddPort("S", "Layout1", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout1", 0, 0, 24, 0, 0, 0);
    tJunction.AddPort("E", "Layout1", 12, 0, 12, 0, 90, 0);

    var tJunctionPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("NS_S", "S", 1, "N", 2)
        .Deflection(0)
        .Signal("V_Car");
    tJunctionPath.StraightToEnd(out var s1);
    tJunctionPath.PutProp(["Road1_Straight_DikeL"], -2, 0.12, 0, 0, 1.2, 1.2);
    tJunctionPath.PutProp(["WhiteLine500"], -1.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -0.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 1.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], -2, 0, 0, s1 - 0.75, 1, 1, 1);

    for (double x = -5.5; x <= 1.5; x += 1) tJunctionPath.PutProp(["WhiteLine500"], x, 0, 0, 0, 0, 0, 3, 1, 1, 4);
    for (double x = -5.5; x <= 1.5; x += 1) tJunctionPath.PutProp(["WhiteLine500"], x, 0, 0, 0, 0, 0, s1 - 6, 1, 1, 4);

    for (double x = 4; x <= 7; x += 1) tJunctionPath.PutProp(["WhiteLine500"], x, 0, 0, 0, 90, 0, 8.5, 1, 1, 8);

    tJunctionPath = tJunction.Wire("SN_S", "S", 2, "N", 1)
        .Deflection(0)
        .Signal("V_Car");
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["WhiteLine150"], -2, 0, 0, 0, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -1.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -0.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 1.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], 2, -0.12, 0, -0.25, 1, 1, 1);

    tJunctionPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("ES_L", "S", 1, "E", 2)
        .Deflection(1)
        .Signal("H_Car")
        .SpeedLimit(30);
    tJunctionPath.BezierToEnd(out s1);
    tJunctionPath.PutProp(["WhiteLine500"], -1.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -0.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 1.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], -2, 0, 0, s1 - 0.75, 1, 1, 1);

    tJunctionPath = tJunction.Wire("SE_R", "S", 2, "E", 1)
        .Deflection(1)
        .Signal("V_Car")
        .Yield("NS_S", "NE_L")
        .SpeedLimit(30);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("SE3", "S", 3, "E", 0);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NE0", "N", 0, "E", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("EN_R","N", 1, "E", 2)
        .Deflection(-1)
        .Signal("H_Car")
        .SpeedLimit(30);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NE_L","N", 2, "E", 1)
        .Deflection(-1)
        .Signal("V_Car")
        .SpeedLimit(30);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NE3","N", 3, "E", 0);
    tJunctionPath.BezierToEnd();

    tJunction.PutProp("Road1_Junction1_RoadL", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_RoadR", 0, 0, 12);
    tJunction.PutProp("Road1_Junction1_WalkL", 0, 0, 12);
    tJunction.PutProp("Road1_Junction1_WalkR", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 270, 0);
    tJunction.PutProp("Signal_L", -5.25, 0, 23, 0, 0, 0);
    tJunction.PutSignalProp("Signal_L_CarRed", -5.25, 0, 23, 0, 0, 0, "V_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", -5.25, 0, 23, 0, 0, 0, "V_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", -5.25, 0, 23, 0, 0, 0, "V_Car", 2);
    tJunction.PutProp("Signal_L", 5.25, 0, 1, 0, 180, 0);
    tJunction.PutSignalProp("Signal_L_CarRed", 5.25, 0, 1, 0, 180, 0, "V_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", 5.25, 0, 1, 0, 180, 0, "V_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", 5.25, 0, 1, 0, 180, 0, "V_Car", 2);
    tJunction.PutProp("Signal_L", -5.25, 0, 7.5, 0, -90, 0);
    tJunction.PutSignalProp("Signal_L_CarRed", -5.25, 0, 7.5, 0, -90, 0, "H_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", -5.25, 0, 7.5, 0, -90, 0, "H_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", -5.25, 0, 7.5, 0, -90, 0, "H_Car", 2);
}

// --------------------------------------------------
// 三叉路 本線 1、分岐 2
{
    var tJunction = Network.Templates.CreateJunction("3Forked2");
    tJunction.AddPort("S", "Layout1", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout1", 0, 0, 20, 0, 0, 0);
    tJunction.AddPort("E", "Layout2", 12, 0, 10, 0, 90, 0);

    var tJunctionPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0);
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["Road1_Straight_DikeL"], -2, 0.12, 0, 0, 1.2, 1.2);
    tJunctionPath.PutProp(["WhiteLine150"], 2, 0, 0, 0, 1, 1, 3);
    tJunctionPath.PutProp(["WhiteLine150"], 2, 0, 0, 4, 1, 2, 8);
    tJunctionPath.PutProp(["WhiteLine150"], 2, 0, 0, 17, 1, 1);

    tJunctionPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0);
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["WhiteLine150"], -2, 0, 0, 0, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], 2, 0, 0, 0, 1, 1, 5);
    tJunctionPath.PutProp(["WhiteLine150"], 2, 0, 0, 10, 1, 1, 5);

    tJunctionPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Yield("NS_S", "SE_R", "NE_L")
        .SpeedLimit(30);
    tJunctionPath
        .BezierTo(7, -0.12, 8.5, 0, 90, 0)
        .StraightToEnd(out var s1);
    tJunctionPath.PutProp(["WhiteLine150"], 1.3, 0, 0, s1 - 4, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -1, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.8, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);

    tJunctionPath = tJunction.Wire("SE_R", "S", 2, "E", 1) // S→E 右折
        .Deflection(1)
        .Yield("NS_S", "NE_L")
        .SpeedLimit(30);
    tJunctionPath
        .BezierTo(7, -0.12, 11.5, 0, 90, 0)
        .StraightToEnd();

    tJunctionPath = tJunction.Wire("SE3", "S", 3, "E", 0);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NE0", "N", 0, "E", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("EN_R","N", 1, "E", 2) // E→N 右折
        .Deflection(-1)
        .Yield("NS_S", "SN_S", "SE_R", "NE_L");
    tJunctionPath
        .BezierTo(7, -0.12, 8.5, 0, 90, 0)
        .StraightToEnd();

    tJunctionPath = tJunction.Wire("NE_L","N", 2, "E", 1) // N→E 左折
        .Deflection(-1);
    tJunctionPath
        .BezierTo(7, -0.12, 11.5, 0, 90, 0)
        .StraightToEnd(out s1);
    tJunctionPath.PutProp(["WhiteLine150"], -1.3, 0, 0, s1 - 4, 1, 1);

    tJunctionPath = tJunction.Wire("NE3","N", 3, "E", 0);
    tJunctionPath.BezierToEnd();

    tJunction.PutProp("Road1_Junction2_DikeL", 0, 0, 10, 0, 180, 0);
    tJunction.PutProp("Road1_Junction2_DikeR", 0, 0, 10);
    tJunction.PutProp("Road1_Junction2_RoadL", 0, 0, 10, 0, 180, 0);
    tJunction.PutProp("Road1_Junction2_RoadR", 0, 0, 10);
    tJunction.PutProp("Road1_Junction2_WalkL", 0, 0, 10);
    tJunction.PutProp("Road1_Junction2_WalkR", 0, 0, 10, 0, 180, 0);
}

// --------------------------------------------------
// 三叉路 2
{
    var tJunction = Network.Templates.CreateJunction("3Forked3");
    tJunction.AddPort("S", "Layout2", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout2", 0, 0, 16, 0, 0, 0);
    tJunction.AddPort("E", "Layout2", 8, 0, 8, 0, 90, 0);

    var tJunctionPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0);
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["Road1_Straight_DikeL"], 2.5, 0.12, 0, 0, 1.2, 1.2);
    tJunctionPath.PutProp(["Road2_StraightL"], -1.5, 0.12, 0, 0, 1.2, 1.2);
    tJunctionPath.PutProp(["WhiteLine150"], 1.3, 0, 0, 1, 1, 2);

    tJunctionPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0);
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["WhiteLine150"], -1.3, 0, 0, 0, 1, 1);

    tJunctionPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Yield("NS_S", "SE_R", "NE_L")
        .SpeedLimit(30);
    tJunctionPath
        .BezierToEnd(out var s1);
    tJunctionPath.PutProp(["WhiteLine150"], 1.3, 0, 0, s1 - 0.75, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -1, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.8, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);

    tJunctionPath = tJunction.Wire("SE_R", "S", 2, "E", 1) // S→E 右折
        .Deflection(1)
        .Yield("NS_S", "NE_L")
        .TrafficDensity(0.25)
        .SpeedLimit(30);
    tJunctionPath
        .BezierToEnd();

    tJunctionPath = tJunction.Wire("SE3", "S", 3, "E", 0);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NE0", "N", 0, "E", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("EN_R","N", 1, "E", 2) // E→N 右折
        .Deflection(-1)
        .Yield("NS_S", "SN_S", "SE_R", "NE_L");
    tJunctionPath
        .BezierToEnd();

    tJunctionPath = tJunction.Wire("NE_L","N", 2, "E", 1) // N→E 左折
        .Deflection(-1)
        .TrafficDensity(0.25);
    tJunctionPath
        .BezierToEnd(out s1);
    tJunctionPath.PutProp(["WhiteLine150"], -1.3, 0, 0, s1 - 0.75, 1, 1);

    tJunctionPath = tJunction.Wire("NE3","N", 3, "E", 0);
    tJunctionPath.BezierToEnd();

    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 180, 0);
    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 270, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 180, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 270, 0);
}

// --------------------------------------------------
// 四叉路 1
{
    var tJunction = Network.Templates.CreateJunction("4Forked1")
        .SignalController("4Forked2");
    tJunction.AddPort("S", "Layout1", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout1", 0, 0, 24, 0, 0, 0);
    tJunction.AddPort("E", "Layout1", 12, 0, 12, 0, 90, 0);
    tJunction.AddPort("W", "Layout1", -12, 0, 12, 0, -90, 0);

    var tJunctionPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0)
        .Signal("V_Car");
    tJunctionPath.StraightToEnd(out var s1);
    tJunctionPath.PutProp(["WhiteLine500"], -1.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -0.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 1.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], -2, 0, 0, s1 - 0.75, 1, 1, 1);

    for (double x = -5.5; x <= 1.5; x += 1) tJunctionPath.PutProp(["WhiteLine500"], x, 0, 0, 0, 0, 0, 3, 1, 1, 4);
    for (double x = -5.5; x <= 1.5; x += 1) tJunctionPath.PutProp(["WhiteLine500"], x, 0, 0, 0, 0, 0, s1 - 6, 1, 1, 4);

    tJunctionPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0)
        .Signal("V_Car");
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["WhiteLine500"], -1.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -0.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 1.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], 2, -0.12, 0, -0.25, 1, 1, 1);

    tJunctionPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Signal("H_Car");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("SE_R", "S", 2, "E", 1) // S→E 右折
        .Deflection(1)
        .Signal("V_Car")
        .Yield("NS_S", "NE_L");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("WS_R", "S", 1, "W", 2) // W→S 右折
        .Deflection(-1)
        .Signal("H_Car")
        .Yield("EW_S", "ES_L");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("SW_L", "S", 2, "W", 1) // S→W 左折
        .Deflection(-1)
        .Signal("V_Car");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("SW3", "S", 3, "W", 0);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("EN_R", "N", 1, "E", 2) // E→N 右折
        .Deflection(-1)
        .Signal("H_Car")
        .Yield("WE_S", "WN_L");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NE_L", "N", 2, "E", 1) // N→E 左折
        .Deflection(-1)
        .Signal("V_Car");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NE3", "N", 3, "E", 0);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NW0", "N", 0, "W", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("WN_L", "N", 1, "W", 2) // W→N 左折
        .Deflection(1)
        .Signal("H_Car");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NW_R", "N", 2, "W", 1) // N→W 右折
        .Deflection(1)
        .Signal("V_Car")
        .Yield("SN_S", "SW_L");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("EW0", "E", 0, "W", 3);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("WE_S", "E", 1, "W", 2) // W→E 直進
        .Deflection(0)
        .Signal("H_Car");
    tJunctionPath.StraightToEnd(out s1);
    tJunctionPath.PutProp(["WhiteLine500"], -1.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -0.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 1.5, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], -2, 0, 0, s1 - 0.75, 1, 1, 1);

    for (double x = -5.5; x <= 1.5; x += 1) tJunctionPath.PutProp(["WhiteLine500"], x, 0, 0, 0, 0, 0, 3, 1, 1, 4);
    for (double x = -5.5; x <= 1.5; x += 1) tJunctionPath.PutProp(["WhiteLine500"], x, 0, 0, 0, 0, 0, s1 - 6, 1, 1, 4);

    tJunctionPath = tJunction.Wire("EW_S", "E", 2, "W", 1) // E→W 直進
        .Deflection(0)
        .Signal("H_Car");
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["WhiteLine500"], -1.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], -0.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 1.5, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], 2, -0.12, 0, -0.25, 1, 1, 1);

    tJunctionPath = tJunction.Wire("EW3", "E", 3, "W", 0);
    tJunctionPath.StraightToEnd();

    tJunction.PutProp("Road1_Junction1_RoadL", 0, 0, 12);
    tJunction.PutProp("Road1_Junction1_RoadL", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_RoadR", 0, 0, 12);
    tJunction.PutProp("Road1_Junction1_RoadR", 0, 0, 12, 0, 180, 0);

    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 90, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 270, 0);

    tJunction.PutProp("Signal_L", -5.25, 0, 23, 0, 0, 0);
    tJunction.PutSignalProp("Signal_L_CarRed", -5.25, 0, 23, 0, 0, 0, "V_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", -5.25, 0, 23, 0, 0, 0, "V_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", -5.25, 0, 23, 0, 0, 0, "V_Car", 2);
    tJunction.PutProp("Signal_L", 5.25, 0, 1, 0, 180, 0);
    tJunction.PutSignalProp("Signal_L_CarRed", 5.25, 0, 1, 0, 180, 0, "V_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", 5.25, 0, 1, 0, 180, 0, "V_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", 5.25, 0, 1, 0, 180, 0, "V_Car", 2);
    tJunction.PutProp("Signal_L", 11, 0, 17.25, 0, 90, 0);
    tJunction.PutSignalProp("Signal_L_CarRed", 11, 0, 17.25, 0, 90, 0, "H_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", 11, 0, 17.25, 0, 90, 0, "H_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", 11, 0, 17.25, 0, 90, 0, "H_Car", 2);
    tJunction.PutProp("Signal_L", -11, 0, 6.75, 0, -90, 0);
    tJunction.PutSignalProp("Signal_L_CarRed", -11, 0, 6.75, 0, -90, 0, "H_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", -11, 0, 6.75, 0, -90, 0, "H_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", -11, 0, 6.75, 0, -90, 0, "H_Car", 2);
}

// --------------------------------------------------
// 四叉路 2
{
    var tJunction = Network.Templates.CreateJunction("4Forked2");
    tJunction.AddPort("S", "Layout2", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout2", 0, 0, 16, 0, 0, 0);
    tJunction.AddPort("E", "Layout2", 8, 0, 8, 0, 90, 0);
    tJunction.AddPort("W", "Layout2", -8, 0, 8, 0, -90, 0);

    var tJunctionPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0);
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["WhiteLine150"], 1.3, 0, 0, 0, 1, 0, 1);
    tJunctionPath.PutProp(["WhiteLine150"], 1.3, 0, 0, 1, 1, 2);

    tJunctionPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0);
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["WhiteLine150"], -1.3, 0, 0, 0, 1, 0, 1);
    tJunctionPath.PutProp(["WhiteLine150"], -1.3, 0, 0, 1, 1, 2);

    tJunctionPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Yield("NS_S", "SN_S", "SE_R", "WS_R", "NE_L", "NW_R");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("SE_R", "S", 2, "E", 1) // S→E 右折
        .Deflection(1)
        .Yield("NS_S", "NE_L")
        .TrafficDensity(0.125);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("WS_R", "S", 1, "W", 2) // W→S 右折
        .Deflection(-1)
        .Yield("NS_S", "SN_S", "ES_L", "SE_R", "SW_L", "EN_R", "NE_L", "NW_R", "EW_S");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("SW_L", "S", 2, "W", 1) // S→W 左折
        .Deflection(-1)
        .TrafficDensity(0.125);
    tJunctionPath.BezierToEnd(out var s1);
    tJunctionPath.PutProp(["WhiteLine150"], -1.3, 0, 0, s1 - 0.75, 1, 1);

    tJunctionPath = tJunction.Wire("SW3", "S", 3, "W", 0);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("EN_R","N", 1, "E", 2) // E→N 右折
        .Deflection(-1)
        .Yield("NS_S", "SN_S", "SE_R", "WS_R", "SW_L", "NE_L", "WN_L", "NW_R", "WE_S");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NE_L","N", 2, "E", 1) // N→E 左折
        .Deflection(-1)
        .TrafficDensity(0.125);
    tJunctionPath.BezierToEnd(out s1);
    tJunctionPath.PutProp(["WhiteLine150"], -1.3, 0, 0, s1 - 0.75, 1, 1);

    tJunctionPath = tJunction.Wire("NE3","N", 3, "E", 0);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NW0","N", 0, "W", 3);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("WN_L","N", 1, "W", 2) // W→N 左折
        .Deflection(1)
        .Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R");
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("NW_R","N", 2, "W", 1) // N→W 右折
        .Deflection(1)
        .Yield("SN_S", "SW_L", "SE_R")
        .TrafficDensity(0.125);
    tJunctionPath.BezierToEnd();

    tJunctionPath = tJunction.Wire("EW0", "E", 0, "W", 3);
    tJunctionPath.StraightToEnd();

    tJunctionPath = tJunction.Wire("WE_S", "E", 1, "W", 2) // W→E 直進
        .Deflection(0)
        .Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R")
        .TrafficDensity(0.25);
    tJunctionPath.StraightToEnd(out s1);
    tJunctionPath.PutProp(["WhiteLine500"], -1, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0.8, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], 1.3, 0, 0, s1 - 0.75, 1, 1, 1);

    tJunctionPath = tJunction.Wire("EW_S", "E", 2, "W", 1) // E→W 直進
        .Deflection(0)
        .Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R")
        .TrafficDensity(0.25);
    tJunctionPath.StraightToEnd();
    tJunctionPath.PutProp(["WhiteLine500"], -0.8, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine500"], 1, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tJunctionPath.PutProp(["WhiteLine150"], -1.3, 0, 0, -0.25, 1, 1, 1);

    tJunctionPath = tJunction.Wire("EW3", "E", 3, "W", 0);
    tJunctionPath.StraightToEnd();

    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8);
    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 90, 0);
    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 180, 0);
    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 270, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 90, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 180, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 270, 0);
}

// --------------------------------------------------
// 行き止まり
{
    var tJunction = Network.Templates.CreateJunction("DeadEnd1");
    tJunction.AddPort("0", "Layout1", 0, 0, 0, 0, 180, 0);
    var tJunctionPath = tJunction.Wire("", "0", 2, "0", 1)
        .SpeedLimit(20);
    tJunctionPath
        .BezierTo(-5, 0, 10, 0, 0, 0, out var s1)
        .BezierTo(5, 0, 10, 0, 180, 0, out var s2, 6.5)
        .BezierToEnd();
    tJunctionPath.Width
        .Constant(5)
        .TransitionTo(2.5, 3, s1 - 5)
        .Constant(s2 - s1);
    tJunction.PutProp("Road1_DeadEnd", 0, 0, 0);
}
