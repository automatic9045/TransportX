#load "__Editor.csx"

// --------------------------------------------------
// 直線・曲線道路 1
{
    var tSpline = Network.Templates.CreateSpline("Spline1", "Layout1")
        .SpeedLimit(1, 60)
        .SpeedLimit(2, 60);
    tSpline.PutExtrusion("DikeL", "DikeL", -7.75, 0, 5, 0.01, 2, 0);
    tSpline.PutExtrusion("DikeR", "DikeR", 7.75, 0, 5, 0.01, 2, 0);
    tSpline.PutExtrusion("RoadL", "Road1L", 5, 0.01, 2, 0);
    tSpline.PutExtrusion("RoadR", "Road1R", 5, 0.01, 2, 0);
    tSpline.PutExtrusion("WhiteLineL", "WhiteLine150", -4, -0.12, 5, 0.01, 2, 0);
    tSpline.PutExtrusion("WhiteLineR", "WhiteLine150", 4, -0.12, 5, 0.01, 2, 0);
}

// --------------------------------------------------
// 直線・曲線道路 2
{
    var tSpline = Network.Templates.CreateSpline("Spline2", "Layout2")
        .SpeedLimit(1, 60)
        .SpeedLimit(2, 60);
    tSpline.PutExtrusion("DikeL", "DikeL", -3.75, 0, 5, 0.01, 2, 0);
    tSpline.PutExtrusion("DikeR", "DikeR", 3.75, 0, 5, 0.01, 2, 0);
    tSpline.PutExtrusion("RoadL", "Road2L", 5, 0.01, 2, 0);
    tSpline.PutExtrusion("RoadR", "Road2R", 5, 0.01, 2, 0);
    tSpline.PutExtrusion("WhiteLineL", "WhiteLine150", -2.8, -0.12, 5, 0.01, 2, 0);
    tSpline.PutExtrusion("WhiteLineR", "WhiteLine150", 2.8, -0.12, 5, 0.01, 2, 0);
}

// --------------------------------------------------
// 三叉路 1
{
    var tJunction = Network.Templates.CreateJunction("3Forked1")
        .SignalController("4Forked1");
    tJunction.AddPort("S", "Layout1", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout1", 0, 0, 24, 0, 0, 0);
    tJunction.AddPort("E", "Layout1", 12, 0, 12, 0, 90, 0);
    tJunction.AddRelay("SEIn", "SEOut", "LayoutSingle", -1.5, 0, 7, 0, 15, 0);

    var tPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0)
        .Signal("V_Car");
    tPath.StraightToEnd(out var s1);
    tPath.PutExtrusion("Dike", "DikeL", -9.75, 0.12, 5, 0.01, 2, 0);

    tPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0)
        .Signal("V_Car");
    tPath.StraightToEnd();

    tPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Signal("H_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd(out s1);

    tPath = tJunction.Wire("SE_R_In", "S", 2, "SEIn", 0) // S→E 右折
        .Deflection(1)
        .Signal("V_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);
    tPath = tJunction.Wire("SE_R_Out", "SEOut", 0, "E", 1)
        .Deflection(1)
        .Yield(("NS_S", 7, 17), ("NE_L", 7, s1))
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);

    tPath = tJunction.Wire("SE3", "S", 3, "E", 0);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NE0", "N", 0, "E", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("EN_R","N", 1, "E", 2)
        .Deflection(-1)
        .Signal("H_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NE_L","N", 2, "E", 1)
        .Deflection(-1)
        .Signal("V_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NE3","N", 3, "E", 0);
    tPath.BezierToEnd();

    var tCurve = tJunction.AddCurve("SN", 0, -0.12, 0, 0, 0, 0)
        .StraightTo(0, -0.12, 24, 0, 0, 0, out s1);
    tCurve.PutExtrusion("WhiteLine500", 11.75, 0, 5, 0.01, 2, 8, 12.075);
    tCurve.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 0, 2.5);
    tCurve.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 6.5, s1 - 6.5);
    tCurve.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, s1 - 2.5);
    tCurve.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, 0, 2.5);
    tCurve.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, s1 - 2.5);
    for (double x = -3.5; x <= 3.5; x += 1) tCurve.PutExtrusion("WhiteLine500", x, 0, 5, 0.01, 2, 2.5, 6.5);
    for (double x = -3.5; x <= 3.5; x += 1) tCurve.PutExtrusion("WhiteLine500", x, 0, 5, 0.01, 2, s1 - 6.5, s1 - 2.5);

    tCurve = tJunction.AddCurve("EW", 12, -0.12, 12, 0, -90, 0)
        .StraightTo(-12, -0.12, 12, 0, -90, 0);
    tCurve.PutExtrusion("WhiteLine500", 11.75, 0, 5, 0.01, 2, 8, 12.075);
    tCurve.PutExtrusion("WhiteLine500", -11.75, 0, 5, 0.01, 2, 11.925, 16);
    tCurve.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 0, 2.5);
    tCurve.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, 0, 2.5);
    for (double x = -3.5; x <= 3.5; x += 1) tCurve.PutExtrusion("WhiteLine500", x, 0, 5, 0.01, 2, 2.5, 6.5);

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

    var tPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0);
    tPath.StraightToEnd();
    tPath.PutExtrusion("Dike", "DikeL", -9.75, 0.12, 5, 0.01, 2, 0);
    tPath.PutProp(["WhiteLine150"], 2, 0, 0, 0, 1, 1, 3);
    tPath.PutProp(["WhiteLine150"], 2, 0, 0, 4, 1, 2, 8);
    tPath.PutProp(["WhiteLine150"], 2, 0, 0, 17, 1, 1);

    tPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0);
    tPath.StraightToEnd();
    tPath.PutProp(["WhiteLine150"], -2, 0, 0, 0, 1, 1);
    tPath.PutProp(["WhiteLine150"], 2, 0, 0, 5, 1, 1, 5);
    tPath.PutProp(["WhiteLine150"], 2, 0, 0, 15, 1, 1, 5);

    tPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Yield("NS_S", "SE_R", "NE_L")
        .SpeedLimit(30);
    tPath
        .BezierTo(7, -0.12, 8.5, 0, 90, 0)
        .StraightToEnd(out var s1);
    tPath.PutProp(["WhiteLine150"], 1.3, 0, 0, s1 - 4, 1, 1);
    tPath.PutProp(["WhiteLine500"], -1, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine500"], 0.8, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);

    tPath = tJunction.Wire("SE_R", "S", 2, "E", 1) // S→E 右折
        .Deflection(1)
        .Yield("NS_S", "NE_L")
        .SpeedLimit(30);
    tPath
        .BezierTo(7, -0.12, 11.5, 0, 90, 0)
        .StraightToEnd();

    tPath = tJunction.Wire("SE3", "S", 3, "E", 0);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NE0", "N", 0, "E", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("EN_R","N", 1, "E", 2) // E→N 右折
        .Deflection(-1)
        .Yield("NS_S", "SN_S", "SE_R", "NE_L");
    tPath
        .BezierTo(7, -0.12, 8.5, 0, 90, 0)
        .StraightToEnd();

    tPath = tJunction.Wire("NE_L","N", 2, "E", 1) // N→E 左折
        .Deflection(-1);
    tPath
        .BezierTo(7, -0.12, 11.5, 0, 90, 0)
        .StraightToEnd(out s1);
    tPath.PutProp(["WhiteLine150"], -1.3, 0, 0, s1 - 4, 1, 1);

    tPath = tJunction.Wire("NE3","N", 3, "E", 0);
    tPath.BezierToEnd();

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

    var tPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0);
    tPath.StraightToEnd();
    tPath.PutExtrusion("Dike", "DikeL", -5.25, 0.12, 5, 0.01, 2, 0);
    tPath.PutExtrusion("WhiteLineDashed150", 1.3, 0, 5, 0.01, 2, 1, 0.5, 0);

    tPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0);
    tPath.StraightToEnd();
    tPath.PutExtrusion("Road2L", 1.5, 0.12, 5, 0.01, 2, 0);
    tPath.PutExtrusion("WhiteLine150", -1.3, 0, 5, 0.01, 2, 0);

    tPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Yield("NS_S", "SE_R", "NE_L")
        .SpeedLimit(30);
    tPath
        .BezierToEnd(out var s1);
    tPath.PutProp(["WhiteLine150"], 1.3, 0, 0, s1 - 0.75, 1, 1);
    tPath.PutProp(["WhiteLine500"], -1, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine500"], 0.8, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);

    tPath = tJunction.Wire("SE_R", "S", 2, "E", 1) // S→E 右折
        .Deflection(1)
        .Yield("NS_S", "NE_L")
        .TrafficDensity(0.25)
        .SpeedLimit(30);
    tPath
        .BezierToEnd();

    tPath = tJunction.Wire("SE3", "S", 3, "E", 0);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NE0", "N", 0, "E", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("EN_R","N", 1, "E", 2) // E→N 右折
        .Deflection(-1)
        .Yield("NS_S", "SN_S", "SE_R", "NE_L");
    tPath
        .BezierToEnd();

    tPath = tJunction.Wire("NE_L","N", 2, "E", 1) // N→E 左折
        .Deflection(-1)
        .TrafficDensity(0.25);
    tPath
        .BezierToEnd(out s1);
    tPath.PutProp(["WhiteLine150"], -1.3, 0, 0, s1 - 0.75, 1, 1);

    tPath = tJunction.Wire("NE3","N", 3, "E", 0);
    tPath.BezierToEnd();

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
    tJunction.AddRelay("SEIn", "SEOut", "LayoutSingle", -1.5, 0, 7, 0, 15, 0);
    tJunction.AddRelay("NWIn", "NWOut", "LayoutSingle", 1.5, 0, 17, 0, -165, 0);
    tJunction.AddRelay("ENIn", "ENOut", "LayoutSingle", 5, 0, 10.5, 0, -75, 0);
    tJunction.AddRelay("WSIn", "WSOut", "LayoutSingle", -5, 0, 13.5, 0, 105, 0);

    var tPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0)
        .Signal("V_Car");
    tPath.StraightToEnd();

    tPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0)
        .Signal("V_Car");
    tPath.StraightToEnd();

    tPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Signal("H_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd(out var s1);

    tPath = tJunction.Wire("SE_R_In", "S", 2, "SEIn", 0) // S→E 右折
        .Deflection(1)
        .Signal("V_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);
    tPath = tJunction.Wire("SE_R_Out", "SEOut", 0, "E", 1)
        .Deflection(1)
        .Yield(("NS_S", 7, 17), ("NE_L", 7, s1))
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);

    tPath = tJunction.Wire("WS_R_Out", "S", 1, "WSOut", 0) // W→S 右折
        .Deflection(-1)
        .Yield(("EW_S", 7, 17), ("ES_L", 0, s1 - 7))
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);
    tPath = tJunction.Wire("WS_R_In", "WSIn", 0, "W", 2)
        .Deflection(-1)
        .Signal("H_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);

    tPath = tJunction.Wire("SW_L", "S", 2, "W", 1) // S→W 左折
        .Deflection(-1)
        .Signal("V_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("SW3", "S", 3, "W", 0);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("EN_R_Out", "N", 1, "ENOut", 0) // E→N 右折
        .Deflection(-1)
        .Yield(("WE_S", 7, 17), ("WN_L", 0, s1 - 7))
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);
    tPath = tJunction.Wire("EN_R_In", "ENIn", 0, "E", 2)
        .Deflection(-1)
        .Signal("H_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);

    tPath = tJunction.Wire("NE_L", "N", 2, "E", 1) // N→E 左折
        .Deflection(-1)
        .Signal("V_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NE3", "N", 3, "E", 0);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NW0", "N", 0, "W", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("WN_L", "N", 1, "W", 2) // W→N 左折
        .Deflection(1)
        .Signal("H_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NW_R_In", "N", 2, "NWIn", 0) // N→W 右折
        .Deflection(1)
        .Signal("V_Car")
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);
    tPath = tJunction.Wire("NW_R_Out", "NWOut", 0, "W", 1)
        .Deflection(1)
        .Yield(("SN_S", 7, 17), ("SW_L", 7, s1))
        .SpeedLimit(30);
    tPath.BezierToEnd(0.3);

    tPath = tJunction.Wire("EW0", "E", 0, "W", 3);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("WE_S", "E", 1, "W", 2) // W→E 直進
        .Deflection(0)
        .Signal("H_Car");
    tPath.StraightToEnd();

    tPath = tJunction.Wire("EW_S", "E", 2, "W", 1) // E→W 直進
        .Deflection(0)
        .Signal("H_Car");
    tPath.StraightToEnd();

    tPath = tJunction.Wire("EW3", "E", 3, "W", 0);
    tPath.StraightToEnd();

    tJunction.PutProp("Road1_Junction1_RoadL", 0, 0, 12);
    tJunction.PutProp("Road1_Junction1_RoadL", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_RoadR", 0, 0, 12);
    tJunction.PutProp("Road1_Junction1_RoadR", 0, 0, 12, 0, 180, 0);

    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 90, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 270, 0);

    var tCurve = tJunction.AddCurve("SN", 0, -0.12, 0, 0, 0, 0)
        .StraightTo(0, -0.12, 24, 0, 0, 0, out s1);
    tCurve.PutExtrusion("WhiteLine500", 11.75, 0, 5, 0.01, 2, 8, 12.075);
    tCurve.PutExtrusion("WhiteLine500", -11.75, 0, 5, 0.01, 2, 11.925, 16);
    tCurve.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 0, 2.5);
    tCurve.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, s1 - 2.5);
    tCurve.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, 0, 2.5);
    tCurve.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, s1 - 2.5);
    for (double x = -3.5; x <= 3.5; x += 1) tCurve.PutExtrusion("WhiteLine500", x, 0, 5, 0.01, 2, 2.5, 6.5);
    for (double x = -3.5; x <= 3.5; x += 1) tCurve.PutExtrusion("WhiteLine500", x, 0, 5, 0.01, 2, s1 - 6.5, s1 - 2.5);

    tCurve = tJunction.AddCurve("EW", 12, -0.12, 12, 0, -90, 0)
        .StraightTo(-12, -0.12, 12, 0, -90, 0, out s1);
    tCurve.PutExtrusion("WhiteLine500", 11.75, 0, 5, 0.01, 2, 8, 12.075);
    tCurve.PutExtrusion("WhiteLine500", -11.75, 0, 5, 0.01, 2, 11.925, 16);
    tCurve.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 0, 2.5);
    tCurve.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, s1 - 2.5);
    tCurve.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, 0, 2.5);
    tCurve.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, s1 - 2.5);
    for (double x = -3.5; x <= 3.5; x += 1) tCurve.PutExtrusion("WhiteLine500", x, 0, 5, 0.01, 2, 2.5, 6.5);
    for (double x = -3.5; x <= 3.5; x += 1) tCurve.PutExtrusion("WhiteLine500", x, 0, 5, 0.01, 2, s1 - 6.5, s1 - 2.5);

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

    var tPath = tJunction.Wire("SN0", "S", 0, "N", 3);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("NS_S", "S", 1, "N", 2) // N→S 直進
        .Deflection(0);
    tPath.StraightToEnd();
    tPath.PutExtrusion("WhiteLineDashed150", 1.3, 0, 5, 0.01, 2, 1, 0.5, 0);

    tPath = tJunction.Wire("SN_S", "S", 2, "N", 1) // S→N 直進
        .Deflection(0);
    tPath.StraightToEnd();
    tPath.PutExtrusion("WhiteLineDashed150", -1.3, 0, 5, 0.01, 2, 1, 0.5, 0);

    tPath = tJunction.Wire("SN3", "S", 3, "N", 0);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("SE0", "S", 0, "E", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("ES_L", "S", 1, "E", 2) // E→S 左折
        .Deflection(1)
        .Yield("NS_S", "SN_S", "SE_R", "WS_R", "NE_L", "NW_R");
    tPath.BezierToEnd();

    tPath = tJunction.Wire("SE_R", "S", 2, "E", 1) // S→E 右折
        .Deflection(1)
        .Yield("NS_S", "NE_L")
        .TrafficDensity(0.125);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("WS_R", "S", 1, "W", 2) // W→S 右折
        .Deflection(-1)
        .Yield("NS_S", "SN_S", "ES_L", "SE_R", "SW_L", "EN_R", "NE_L", "NW_R", "EW_S");
    tPath.BezierToEnd();

    tPath = tJunction.Wire("SW_L", "S", 2, "W", 1) // S→W 左折
        .Deflection(-1)
        .TrafficDensity(0.125);
    tPath.BezierToEnd(out var s1);
    tPath.PutProp(["WhiteLine150"], -1.3, 0, 0, s1 - 0.75, 1, 1);

    tPath = tJunction.Wire("SW3", "S", 3, "W", 0);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("EN_R","N", 1, "E", 2) // E→N 右折
        .Deflection(-1)
        .Yield("NS_S", "SN_S", "SE_R", "WS_R", "SW_L", "NE_L", "WN_L", "NW_R", "WE_S");
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NE_L","N", 2, "E", 1) // N→E 左折
        .Deflection(-1)
        .TrafficDensity(0.125);
    tPath.BezierToEnd(out s1);
    tPath.PutProp(["WhiteLine150"], -1.3, 0, 0, s1 - 0.75, 1, 1);

    tPath = tJunction.Wire("NE3","N", 3, "E", 0);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NW0","N", 0, "W", 3);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("WN_L","N", 1, "W", 2) // W→N 左折
        .Deflection(1)
        .Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R");
    tPath.BezierToEnd();

    tPath = tJunction.Wire("NW_R","N", 2, "W", 1) // N→W 右折
        .Deflection(1)
        .Yield("SN_S", "SW_L", "SE_R")
        .TrafficDensity(0.125);
    tPath.BezierToEnd();

    tPath = tJunction.Wire("EW0", "E", 0, "W", 3);
    tPath.StraightToEnd();

    tPath = tJunction.Wire("WE_S", "E", 1, "W", 2) // W→E 直進
        .Deflection(0)
        .Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R")
        .TrafficDensity(0.25);
    tPath.StraightToEnd(out s1);
    tPath.PutProp(["WhiteLine500"], -1, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine500"], 0.8, 0, 0, 0, 90, 0, s1 - 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine150"], 1.3, 0, 0, s1 - 0.75, 1, 1, 1);

    tPath = tJunction.Wire("EW_S", "E", 2, "W", 1) // E→W 直進
        .Deflection(0)
        .Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R")
        .TrafficDensity(0.25);
    tPath.StraightToEnd();
    tPath.PutProp(["WhiteLine500"], -0.8, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine500"], 1, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tPath.PutProp(["WhiteLine150"], -1.3, 0, 0, -0.25, 1, 1, 1);

    tPath = tJunction.Wire("EW3", "E", 3, "W", 0);
    tPath.StraightToEnd();

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
    var tPath = tJunction.Wire("", "0", 2, "0", 1)
        .SpeedLimit(20);
    tPath
        .BezierTo(-5, 0, 10, 0, 0, 0, out var s1)
        .BezierTo(5, 0, 10, 0, 180, 0, out var s2, 0.7)
        .BezierToEnd();
    tPath.Width
        .Constant(5)
        .TransitionTo(2.5, 3, s1 - 5)
        .Constant(s2 - s1);
    tJunction.PutProp("Road1_DeadEnd", 0, 0, 0);
}
