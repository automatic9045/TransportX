#load "../../__Editor.csx"
{
    // <editor-generated version="1">
    var tJunction = Network.Templates.CreateJunction("DeadEnd1");
    tJunction.AddPort("0", "Layout1", 0, 0, 0, 0, 180, 0);

    var tPath_Anon = tJunction.Wire("", "0", 2, "0", 1);
    tPath_Anon
        .BezierTo(-5, 0, 10, 0, 0, 0, out var s_s_1)
        .BezierTo(5, 0, 10, 0, 180, 0, out var s_s_2, 0.7)
        .BezierToEnd();
    tPath_Anon.Width
        .Constant(5)
        .TransitionTo(2.5, 3, s_s_1 - 5);

    tJunction.PutProp("Road1_DeadEnd", 0, 0, 0, 0, 0, 0);
    // </editor-generated>

    // <user-code>
    tPath_Anon.SpeedLimit(20);
    tPath_Anon.Width.Constant(s_s_2 - s_s_1);
    // </user-code>
}
