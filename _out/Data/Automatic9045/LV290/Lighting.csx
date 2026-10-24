#load "__Editor.csx"

Component<Lighting>().Blinkers
    .Lever("Blinker")
    .Hazard("Hazard")
    .RelayOnCount("BlinkerRelayOnCount")
    .RelayOffCount("BlinkerRelayOffCount")
    .Period(0.75)
    .DutyCycle(0.5)
    .CamAutoCancel("Steering", [0.25, 0.5, 0.75], 0.02);

Component<Lighting>().Blinkers.Left.AddEmissiveLamp("Front", "Body", "FrontBlinkerL", 10000, 10000, 10000, 0.1);
Component<Lighting>().Blinkers.Left.AddEmissiveLamp("Side", "Body", "SideBlinkerL", 10000, 10000, 10000, 0.1);
Component<Lighting>().Blinkers.Left.AddEmissiveLamp("Rear", "Body", "RearBlinkerL", 10000, 10000, 10000, 0.1);

Component<Lighting>().Blinkers.Right.AddEmissiveLamp("Front", "Body", "FrontBlinkerR", 10000, 10000, 10000, 0.1);
Component<Lighting>().Blinkers.Right.AddEmissiveLamp("Side", "Body", "SideBlinkerR", 10000, 10000, 10000, 0.1);
Component<Lighting>().Blinkers.Right.AddEmissiveLamp("Rear", "Body", "RearBlinkerR", 10000, 10000, 10000, 0.1);

Signals.ToIntSwitchFromCounter("BlinkerOnCount", "Blinker", 0);
Signals.ToIntSwitchCounter("BlinkerOffCount", "Blinker", 0);
Sounds.Create3D("BlinkerOn", "BlinkerOn", 0.9, 1.2, -1, 90, 0, 0, 1).PlayWhen("BlinkerOnCount");
Sounds.Create3D("BlinkerOff", "BlinkerOff", 0.9, 1.2, -1, 90, 0, 0, 1).PlayWhen("BlinkerOffCount");

Signals.ToBoolSwitchCounter("HazardOnCount", "Hazard", true);
Signals.ToBoolSwitchFromCounter("HazardOffCount", "Hazard", true);
Sounds.Create3D("HazardOn", "BlinkerOn", 0.9, 1.2, -1, 90, 0, 0, 1).PlayWhen("HazardOnCount");
Sounds.Create3D("HazardOff", "BlinkerOff", 0.9, 1.2, -1, 90, 0, 0, 1).PlayWhen("HazardOffCount");

Sounds.Create3D("Blinker", "Blinker", 0.67, 1.2, -0.8, 90, 0, 0, 1).PlayWhen("BlinkerRelayOnCount");


Component<Lighting>().Brakes
    .Signal("Brake");

Component<Lighting>().Brakes.AddEmissiveLamp("Rear", "Body", "RearBrake", 10000, 10000, 10000, 0.1);
