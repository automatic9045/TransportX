#load "__Editor.csx"
#load "Spec.csx"

Component<Chassis>().AddAxle("F")
    .Beam("AxleF", "AxleF", 0, 0.4756, -FrontOverhang, 400)
    .WheelL("WheelFL", "WheelFL", -1.0515, 0.4756, -FrontOverhang, 100)
    .WheelR("WheelFR", "WheelFL", 1.0515, 0.4756, -FrontOverhang, 0, 180, 0, 100)
    .BrakeL("Brake", 8000)
    .BrakeR("Brake", 8000)
    .SteerableL("Steering", InnerSteeringAngle, OuterSteeringAngle)
    .SteerableR("Steering", OuterSteeringAngle, InnerSteeringAngle)
    .Build();

Component<Chassis>().AddAxle("R")
    .Beam("AxleR", "AxleR", 0, 0.4756, -FrontOverhang - Wheelbase, 700)
    .WheelL("WheelRL", "WheelRL", -0.905, 0.4756, -FrontOverhang - Wheelbase, 280)
    .WheelR("WheelRR", "WheelRL", 0.905, 0.4756, -FrontOverhang - Wheelbase, 0, 180, 0, 280)
    .BrakeL("Brake", 6000)
    .BrakeR("Brake", 6000)
    .Build();

Component<Chassis>().AddCoilRigidSuspension("SuspensionF", "Body", "F")
    .Spring(8)
    .Build();

Component<Chassis>().AddCoilRigidSuspension("SuspensionR", "Body", "R")
    .Spring(7)
    .Build();
