#load "../../__Editor.csx"
{
    // <editor-generated version="1">
    var tSpline = Network.Templates.CreateSpline("Spline1", "Layout1");
    tSpline.PutExtrusion("DikeL", "DikeL", -7.75, 0, 5, 0.01, 2, 1, 0, 0);
    tSpline.PutExtrusion("DikeR", "DikeR", 7.75, 0, 5, 0.01, 2, 1, 0, 0);
    tSpline.PutExtrusion("RoadL", "Road1L", 0, 0, 5, 0.01, 2, 1, 0, 0);
    tSpline.PutExtrusion("RoadR", "Road1R", 0, 0, 5, 0.01, 2, 1, 0, 0);
    tSpline.PutExtrusion("WhiteLineL", "WhiteLine150", -4, -0.12, 5, 0.01, 2, 1, 0, 0);
    tSpline.PutExtrusion("WhiteLineR", "WhiteLine150", 4, -0.12, 5, 0.01, 2, 1, 0, 0);
    // </editor-generated>

    // <user-code>
    tSpline.SpeedLimit(1, 60).SpeedLimit(2, 60);
    // </user-code>
}
