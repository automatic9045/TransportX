#load "../../__Editor.csx"
{
    // <editor-generated version="1">
    var tJunction = Network.Templates.CreateJunction("4Forked2");
    tJunction.AddPort("S", "Layout2", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout2", 0, 0, 16, 0, 0, 0);
    tJunction.AddPort("E", "Layout2", 8, 0, 8, 0, 90, 0);
    tJunction.AddPort("W", "Layout2", -8, 0, 8, 0, -90, 0);

    var tPath_SN0 = tJunction.Wire("SN0", "S", 0, "N", 3);
    tPath_SN0
        .StraightToEnd();

    var tPath_NS_S = tJunction.Wire("NS_S", "S", 1, "N", 2);
    tPath_NS_S
        .StraightToEnd();
    tPath_NS_S.PutExtrusion("WhiteLineDashed150", 1.3, 0, 5, 0.01, 2, 1, 0.5, 0, double.MaxValue);

    var tPath_SN_S = tJunction.Wire("SN_S", "S", 2, "N", 1);
    tPath_SN_S
        .StraightToEnd();
    tPath_SN_S.PutExtrusion("WhiteLineDashed150", -1.3, 0, 5, 0.01, 2, 1, 0.5, 0, double.MaxValue);

    var tPath_SN3 = tJunction.Wire("SN3", "S", 3, "N", 0);
    tPath_SN3
        .StraightToEnd();

    var tPath_SE0 = tJunction.Wire("SE0", "S", 0, "E", 3);
    tPath_SE0
        .BezierToEnd();

    var tPath_ES_L = tJunction.Wire("ES_L", "S", 1, "E", 2);
    tPath_ES_L
        .BezierToEnd();

    var tPath_SE_R = tJunction.Wire("SE_R", "S", 2, "E", 1);
    tPath_SE_R.TrafficDensity(0.125);
    tPath_SE_R
        .BezierToEnd();

    var tPath_WS_R = tJunction.Wire("WS_R", "S", 1, "W", 2);
    tPath_WS_R
        .BezierToEnd();

    var tPath_SW_L = tJunction.Wire("SW_L", "S", 2, "W", 1);
    tPath_SW_L.TrafficDensity(0.125);
    tPath_SW_L
        .BezierToEnd(out var s_s_1);
    tPath_SW_L.PutProp(["WhiteLine150"], -1.3, 0, 0, 0, 0, 0, s_s_1 - 0.75, 1, 1);

    var tPath_SW3 = tJunction.Wire("SW3", "S", 3, "W", 0);
    tPath_SW3
        .BezierToEnd();

    var tPath_EN_R = tJunction.Wire("EN_R", "N", 1, "E", 2);
    tPath_EN_R
        .BezierToEnd();

    var tPath_NE_L = tJunction.Wire("NE_L", "N", 2, "E", 1);
    tPath_NE_L.TrafficDensity(0.125);
    tPath_NE_L
        .BezierToEnd(out var s_s_1_2);
    tPath_NE_L.PutProp(["WhiteLine150"], -1.3, 0, 0, 0, 0, 0, s_s_1_2 - 0.75, 1, 1);

    var tPath_NE3 = tJunction.Wire("NE3", "N", 3, "E", 0);
    tPath_NE3
        .BezierToEnd();

    var tPath_NW0 = tJunction.Wire("NW0", "N", 0, "W", 3);
    tPath_NW0
        .BezierToEnd();

    var tPath_WN_L = tJunction.Wire("WN_L", "N", 1, "W", 2);
    tPath_WN_L
        .BezierToEnd();

    var tPath_NW_R = tJunction.Wire("NW_R", "N", 2, "W", 1);
    tPath_NW_R.TrafficDensity(0.125);
    tPath_NW_R
        .BezierToEnd();

    var tPath_EW0 = tJunction.Wire("EW0", "E", 0, "W", 3);
    tPath_EW0
        .StraightToEnd();

    var tPath_WE_S = tJunction.Wire("WE_S", "E", 1, "W", 2);
    tPath_WE_S.TrafficDensity(0.25);
    tPath_WE_S
        .StraightToEnd(out var s_s_1_3);
    tPath_WE_S.PutProp(["WhiteLine500"], -1, 0, 0, 0, 90, 0, s_s_1_3 - 0.5, 1, 1, 1);
    tPath_WE_S.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, s_s_1_3 - 0.5, 1, 1, 1);
    tPath_WE_S.PutProp(["WhiteLine500"], 0.8, 0, 0, 0, 90, 0, s_s_1_3 - 0.5, 1, 1, 1);
    tPath_WE_S.PutProp(["WhiteLine150"], 1.3, 0, 0, 0, 0, 0, s_s_1_3 - 0.75, 1, 1, 1);

    var tPath_EW_S = tJunction.Wire("EW_S", "E", 2, "W", 1);
    tPath_EW_S.TrafficDensity(0.25);
    tPath_EW_S
        .StraightToEnd();
    tPath_EW_S.PutProp(["WhiteLine500"], -0.8, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tPath_EW_S.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tPath_EW_S.PutProp(["WhiteLine500"], 1, 0, 0, 0, 90, 0, 0.5, 1, 1, 1);
    tPath_EW_S.PutProp(["WhiteLine150"], -1.3, 0, 0, 0, 0, 0, -0.25, 1, 1, 1);

    var tPath_EW3 = tJunction.Wire("EW3", "E", 3, "W", 0);
    tPath_EW3
        .StraightToEnd();

    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 0, 0);
    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 90, 0);
    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 180, 0);
    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 270, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 0, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 90, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 180, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 270, 0);
    // </editor-generated>

    // <user-code>
    tPath_NS_S.Deflection(0);
    tPath_SN_S.Deflection(0);
    tPath_ES_L.Deflection(1).Yield("NS_S", "SN_S", "SE_R", "WS_R", "NE_L", "NW_R");
    tPath_SE_R.Deflection(1).Yield("NS_S", "NE_L");
    tPath_WS_R.Deflection(-1).Yield("NS_S", "SN_S", "ES_L", "SE_R", "SW_L", "EN_R", "NE_L", "NW_R", "EW_S");
    tPath_SW_L.Deflection(-1);
    tPath_EN_R.Deflection(-1).Yield("NS_S", "SN_S", "SE_R", "WS_R", "SW_L", "NE_L", "WN_L", "NW_R", "WE_S");
    tPath_NE_L.Deflection(-1);
    tPath_WN_L.Deflection(1).Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R");
    tPath_NW_R.Deflection(1).Yield("SN_S", "SW_L", "SE_R");
    tPath_WE_S.Deflection(0).Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R");
    tPath_EW_S.Deflection(0).Yield("NS_S", "SN_S", "SE_R", "SW_L", "NE_L", "NW_R");
    // </user-code>
}
