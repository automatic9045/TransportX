#load "../../__Editor.csx"
{
    // <editor-generated version="1">
    var tJunction = Network.Templates.CreateJunction("4Forked1");
    tJunction.AddPort("S", "Layout1", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout1", 0, 0, 24, 0, 0, 0);
    tJunction.AddPort("E", "Layout1", 12, 0, 12, 0, 90, 0);
    tJunction.AddPort("W", "Layout1", -12, 0, 12, 0, -90, 0);
    tJunction.AddRelay("SEIn", "SEOut", "LayoutSingle", -1.5, 0, 7, 0, 15, 0);
    tJunction.AddRelay("NWIn", "NWOut", "LayoutSingle", 1.5, 0, 17, 0, -165, 0);
    tJunction.AddRelay("ENIn", "ENOut", "LayoutSingle", 5, 0, 10.5, 0, -75, 0);
    tJunction.AddRelay("WSIn", "WSOut", "LayoutSingle", -5, 0, 13.5, 0, 105, 0);

    var tPath_SN0 = tJunction.Wire("SN0", "S", 0, "N", 3);
    tPath_SN0
        .StraightToEnd();

    var tPath_NS_S = tJunction.Wire("NS_S", "S", 1, "N", 2);
    tPath_NS_S
        .StraightToEnd();

    var tPath_SN_S = tJunction.Wire("SN_S", "S", 2, "N", 1);
    tPath_SN_S
        .StraightToEnd();

    var tPath_SN3 = tJunction.Wire("SN3", "S", 3, "N", 0);
    tPath_SN3
        .StraightToEnd();

    var tPath_SE0 = tJunction.Wire("SE0", "S", 0, "E", 3);
    tPath_SE0
        .BezierToEnd();

    var tPath_ES_L = tJunction.Wire("ES_L", "S", 1, "E", 2);
    tPath_ES_L
        .BezierToEnd(out var s_s_1);

    var tPath_SE_R_In = tJunction.Wire("SE_R_In", "S", 2, "SEIn", 0);
    tPath_SE_R_In
        .BezierToEnd(0.3);

    var tPath_SE_R_Out = tJunction.Wire("SE_R_Out", "SEOut", 0, "E", 1);
    tPath_SE_R_Out
        .BezierToEnd(0.3);

    var tPath_WS_R_Out = tJunction.Wire("WS_R_Out", "S", 1, "WSOut", 0);
    tPath_WS_R_Out
        .BezierToEnd(0.3);

    var tPath_WS_R_In = tJunction.Wire("WS_R_In", "WSIn", 0, "W", 2);
    tPath_WS_R_In
        .BezierToEnd(0.3);

    var tPath_SW_L = tJunction.Wire("SW_L", "S", 2, "W", 1);
    tPath_SW_L
        .BezierToEnd();

    var tPath_SW3 = tJunction.Wire("SW3", "S", 3, "W", 0);
    tPath_SW3
        .BezierToEnd();

    var tPath_EN_R_Out = tJunction.Wire("EN_R_Out", "N", 1, "ENOut", 0);
    tPath_EN_R_Out
        .BezierToEnd(0.3);

    var tPath_EN_R_In = tJunction.Wire("EN_R_In", "ENIn", 0, "E", 2);
    tPath_EN_R_In
        .BezierToEnd(0.3);

    var tPath_NE_L = tJunction.Wire("NE_L", "N", 2, "E", 1);
    tPath_NE_L
        .BezierToEnd();

    var tPath_NE3 = tJunction.Wire("NE3", "N", 3, "E", 0);
    tPath_NE3
        .BezierToEnd();

    var tPath_NW0 = tJunction.Wire("NW0", "N", 0, "W", 3);
    tPath_NW0
        .BezierToEnd();

    var tPath_WN_L = tJunction.Wire("WN_L", "N", 1, "W", 2);
    tPath_WN_L
        .BezierToEnd();

    var tPath_NW_R_In = tJunction.Wire("NW_R_In", "N", 2, "NWIn", 0);
    tPath_NW_R_In
        .BezierToEnd(0.3);

    var tPath_NW_R_Out = tJunction.Wire("NW_R_Out", "NWOut", 0, "W", 1);
    tPath_NW_R_Out
        .BezierToEnd(0.3);

    var tPath_EW0 = tJunction.Wire("EW0", "E", 0, "W", 3);
    tPath_EW0
        .StraightToEnd();

    var tPath_WE_S = tJunction.Wire("WE_S", "E", 1, "W", 2);
    tPath_WE_S
        .StraightToEnd();

    var tPath_EW_S = tJunction.Wire("EW_S", "E", 2, "W", 1);
    tPath_EW_S
        .StraightToEnd();

    var tPath_EW3 = tJunction.Wire("EW3", "E", 3, "W", 0);
    tPath_EW3
        .StraightToEnd();

    var tCurve_SN = tJunction.AddCurve("SN", 0, -0.12, 0, 0, 0, 0);
    tCurve_SN
        .StraightTo(0, -0.12, 24, 0, 0, 0, out var s_s_1_2);
    tCurve_SN.PutExtrusion("WhiteLine500", 11.75, 0, 5, 0.01, 2, 1, 0, 8, 12.075);
    tCurve_SN.PutExtrusion("WhiteLine500", -11.75, 0, 5, 0.01, 2, 1, 0, 11.925, 16);
    tCurve_SN.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 1, 0, 0, 2.5);
    tCurve_SN.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 2.5, double.MaxValue);
    tCurve_SN.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, 1, 0, 0, 2.5);
    tCurve_SN.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 2.5, double.MaxValue);
    tCurve_SN.PutExtrusion("WhiteLine500", -3.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_SN.PutExtrusion("WhiteLine500", -2.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_SN.PutExtrusion("WhiteLine500", -1.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_SN.PutExtrusion("WhiteLine500", -0.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_SN.PutExtrusion("WhiteLine500", 0.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_SN.PutExtrusion("WhiteLine500", 1.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_SN.PutExtrusion("WhiteLine500", 2.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_SN.PutExtrusion("WhiteLine500", 3.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_SN.PutExtrusion("WhiteLine500", -3.5, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 6.5, s_s_1_2 - 2.5);
    tCurve_SN.PutExtrusion("WhiteLine500", -2.5, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 6.5, s_s_1_2 - 2.5);
    tCurve_SN.PutExtrusion("WhiteLine500", -1.5, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 6.5, s_s_1_2 - 2.5);
    tCurve_SN.PutExtrusion("WhiteLine500", -0.5, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 6.5, s_s_1_2 - 2.5);
    tCurve_SN.PutExtrusion("WhiteLine500", 0.5, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 6.5, s_s_1_2 - 2.5);
    tCurve_SN.PutExtrusion("WhiteLine500", 1.5, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 6.5, s_s_1_2 - 2.5);
    tCurve_SN.PutExtrusion("WhiteLine500", 2.5, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 6.5, s_s_1_2 - 2.5);
    tCurve_SN.PutExtrusion("WhiteLine500", 3.5, 0, 5, 0.01, 2, 1, 0, s_s_1_2 - 6.5, s_s_1_2 - 2.5);

    var tCurve_EW = tJunction.AddCurve("EW", 12, -0.12, 12, 0, -90, 0);
    tCurve_EW
        .StraightTo(-12, -0.12, 12, 0, -90, 0, out var s_s_1_3);
    tCurve_EW.PutExtrusion("WhiteLine500", 11.75, 0, 5, 0.01, 2, 1, 0, 8, 12.075);
    tCurve_EW.PutExtrusion("WhiteLine500", -11.75, 0, 5, 0.01, 2, 1, 0, 11.925, 16);
    tCurve_EW.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 1, 0, 0, 2.5);
    tCurve_EW.PutExtrusion("WhiteLine150", -4, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 2.5, double.MaxValue);
    tCurve_EW.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, 1, 0, 0, 2.5);
    tCurve_EW.PutExtrusion("WhiteLine150", 4, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 2.5, double.MaxValue);
    tCurve_EW.PutExtrusion("WhiteLine500", -3.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_EW.PutExtrusion("WhiteLine500", -2.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_EW.PutExtrusion("WhiteLine500", -1.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_EW.PutExtrusion("WhiteLine500", -0.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_EW.PutExtrusion("WhiteLine500", 0.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_EW.PutExtrusion("WhiteLine500", 1.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_EW.PutExtrusion("WhiteLine500", 2.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_EW.PutExtrusion("WhiteLine500", 3.5, 0, 5, 0.01, 2, 1, 0, 2.5, 6.5);
    tCurve_EW.PutExtrusion("WhiteLine500", -3.5, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 6.5, s_s_1_3 - 2.5);
    tCurve_EW.PutExtrusion("WhiteLine500", -2.5, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 6.5, s_s_1_3 - 2.5);
    tCurve_EW.PutExtrusion("WhiteLine500", -1.5, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 6.5, s_s_1_3 - 2.5);
    tCurve_EW.PutExtrusion("WhiteLine500", -0.5, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 6.5, s_s_1_3 - 2.5);
    tCurve_EW.PutExtrusion("WhiteLine500", 0.5, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 6.5, s_s_1_3 - 2.5);
    tCurve_EW.PutExtrusion("WhiteLine500", 1.5, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 6.5, s_s_1_3 - 2.5);
    tCurve_EW.PutExtrusion("WhiteLine500", 2.5, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 6.5, s_s_1_3 - 2.5);
    tCurve_EW.PutExtrusion("WhiteLine500", 3.5, 0, 5, 0.01, 2, 1, 0, s_s_1_3 - 6.5, s_s_1_3 - 2.5);

    tJunction.PutProp("Road1_Junction1_RoadL", 0, 0, 12, 0, 0, 0);
    tJunction.PutProp("Road1_Junction1_RoadL", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_RoadR", 0, 0, 12, 0, 0, 0);
    tJunction.PutProp("Road1_Junction1_RoadR", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 0, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 90, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 180, 0);
    tJunction.PutProp("Road1_Junction1_Dike", 0, 0, 12, 0, 270, 0);
    tJunction.PutProp("Signal_L", -5.25, 0, 23, 0, 0, 0);
    tJunction.PutProp("Signal_L", 5.25, 0, 1, 0, 180, 0);
    tJunction.PutProp("Signal_L", 11, 0, 17.25, 0, 90, 0);
    tJunction.PutProp("Signal_L", -11, 0, 6.75, 0, -90, 0);
    // </editor-generated>

    // <user-code>
    tJunction.SignalController("4Forked2");
    tPath_NS_S.Deflection(0).Signal("V_Car");
    tPath_SN_S.Deflection(0).Signal("V_Car");
    tPath_ES_L.Deflection(1).Signal("H_Car").SpeedLimit(30);
    tPath_SE_R_In.Deflection(1).Signal("V_Car").SpeedLimit(30);
    tPath_SE_R_Out.Deflection(1).Yield(("NS_S", 7, 17), ("NE_L", 7, s_s_1)).SpeedLimit(30);
    tPath_WS_R_Out.Deflection(-1).Yield(("EW_S", 7, 17), ("ES_L", 0, s_s_1 - 7)).SpeedLimit(30);
    tPath_WS_R_In.Deflection(-1).Signal("H_Car").SpeedLimit(30);
    tPath_SW_L.Deflection(-1).Signal("V_Car").SpeedLimit(30);
    tPath_EN_R_Out.Deflection(-1).Yield(("WE_S", 7, 17), ("WN_L", 0, s_s_1 - 7)).SpeedLimit(30);
    tPath_EN_R_In.Deflection(-1).Signal("H_Car").SpeedLimit(30);
    tPath_NE_L.Deflection(-1).Signal("V_Car").SpeedLimit(30);
    tPath_WN_L.Deflection(1).Signal("H_Car").SpeedLimit(30);
    tPath_NW_R_In.Deflection(1).Signal("V_Car").SpeedLimit(30);
    tPath_NW_R_Out.Deflection(1).Yield(("SN_S", 7, 17), ("SW_L", 7, s_s_1)).SpeedLimit(30);
    tPath_WE_S.Deflection(0).Signal("H_Car");
    tPath_EW_S.Deflection(0).Signal("H_Car");
    tJunction.PutSignalProp("Signal_L_CarRed", -5.25, 0, 23, 0, 0, 0, "V_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", -5.25, 0, 23, 0, 0, 0, "V_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", -5.25, 0, 23, 0, 0, 0, "V_Car", 2);
    tJunction.PutSignalProp("Signal_L_CarRed", 5.25, 0, 1, 0, 180, 0, "V_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", 5.25, 0, 1, 0, 180, 0, "V_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", 5.25, 0, 1, 0, 180, 0, "V_Car", 2);
    tJunction.PutSignalProp("Signal_L_CarRed", 11, 0, 17.25, 0, 90, 0, "H_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", 11, 0, 17.25, 0, 90, 0, "H_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", 11, 0, 17.25, 0, 90, 0, "H_Car", 2);
    tJunction.PutSignalProp("Signal_L_CarRed", -11, 0, 6.75, 0, -90, 0, "H_Car", 0);
    tJunction.PutSignalProp("Signal_L_CarYellow", -11, 0, 6.75, 0, -90, 0, "H_Car", 1);
    tJunction.PutSignalProp("Signal_L_CarGreen", -11, 0, 6.75, 0, -90, 0, "H_Car", 2);
    // </user-code>
}
