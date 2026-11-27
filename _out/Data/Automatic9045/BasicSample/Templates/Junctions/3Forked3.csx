#load "../../__Editor.csx"
{
    // <editor-generated version="1">
    var tJunction = Network.Templates.CreateJunction("3Forked3");
    tJunction.AddPort("S", "Layout2", 0, 0, 0, 0, 180, 0);
    tJunction.AddPort("N", "Layout2", 0, 0, 16, 0, 0, 0);
    tJunction.AddPort("E", "Layout2", 8, 0, 8, 0, 90, 0);

    var tPath_SN0 = tJunction.Wire("SN0", "S", 0, "N", 3);
    tPath_SN0
        .StraightToEnd();

    var tPath_NS_S = tJunction.Wire("NS_S", "S", 1, "N", 2);
    tPath_NS_S
        .StraightToEnd();
    tPath_NS_S.PutExtrusion("Dike", "DikeL", -5.25, 0.12, 5, 0.01, 2, 1, 0, 0, double.MaxValue);
    tPath_NS_S.PutExtrusion("WhiteLineDashed150", 1.3, 0, 5, 0.01, 2, 1, 0.5, 0, double.MaxValue);

    var tPath_SN_S = tJunction.Wire("SN_S", "S", 2, "N", 1);
    tPath_SN_S
        .StraightToEnd();
    tPath_SN_S.PutExtrusion("Road2L", 1.5, 0.12, 5, 0.01, 2, 1, 0, 0, double.MaxValue);
    tPath_SN_S.PutExtrusion("WhiteLine150", -1.3, 0, 5, 0.01, 2, 1, 0, 0, double.MaxValue);

    var tPath_SN3 = tJunction.Wire("SN3", "S", 3, "N", 0);
    tPath_SN3
        .StraightToEnd();

    var tPath_SE0 = tJunction.Wire("SE0", "S", 0, "E", 3);
    tPath_SE0
        .BezierToEnd();

    var tPath_ES_L = tJunction.Wire("ES_L", "S", 1, "E", 2);
    tPath_ES_L
        .BezierToEnd(out var s_s_1);
    tPath_ES_L.PutProp(["WhiteLine150"], 1.3, 0, 0, 0, 0, 0, s_s_1 - 0.75, 1, 1);
    tPath_ES_L.PutProp(["WhiteLine500"], -1, 0, 0, 0, 90, 0, s_s_1 - 0.5, 1, 1, 1);
    tPath_ES_L.PutProp(["WhiteLine500"], 0, 0, 0, 0, 90, 0, s_s_1 - 0.5, 1, 1, 1);
    tPath_ES_L.PutProp(["WhiteLine500"], 0.8, 0, 0, 0, 90, 0, s_s_1 - 0.5, 1, 1, 1);

    var tPath_SE_R = tJunction.Wire("SE_R", "S", 2, "E", 1);
    tPath_SE_R.TrafficDensity(0.25);
    tPath_SE_R
        .BezierToEnd();

    var tPath_SE3 = tJunction.Wire("SE3", "S", 3, "E", 0);
    tPath_SE3
        .BezierToEnd();

    var tPath_NE0 = tJunction.Wire("NE0", "N", 0, "E", 3);
    tPath_NE0
        .BezierToEnd();

    var tPath_EN_R = tJunction.Wire("EN_R", "N", 1, "E", 2);
    tPath_EN_R
        .BezierToEnd();

    var tPath_NE_L = tJunction.Wire("NE_L", "N", 2, "E", 1);
    tPath_NE_L.TrafficDensity(0.25);
    tPath_NE_L
        .BezierToEnd(out var s_s_1_2);
    tPath_NE_L.PutProp(["WhiteLine150"], -1.3, 0, 0, 0, 0, 0, s_s_1_2 - 0.75, 1, 1);

    var tPath_NE3 = tJunction.Wire("NE3", "N", 3, "E", 0);
    tPath_NE3
        .BezierToEnd();

    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 180, 0);
    tJunction.PutProp("Road2_Junction1_Road", 0, 0, 8, 0, 270, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 180, 0);
    tJunction.PutProp("Road2_Junction1_Dike", 0, 0, 8, 0, 270, 0);
    // </editor-generated>

    // <user-code>
    tPath_NS_S.Deflection(0);
    tPath_SN_S.Deflection(0);
    tPath_ES_L.Deflection(1).Yield("NS_S", "SE_R", "NE_L").SpeedLimit(30);
    tPath_SE_R.Deflection(1).Yield("NS_S", "NE_L");
    tPath_SE_R.SpeedLimit(30);
    tPath_EN_R.Deflection(-1).Yield("NS_S", "SN_S", "SE_R", "NE_L");
    tPath_NE_L.Deflection(-1);
    // </user-code>
}
