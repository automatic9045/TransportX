#load "__Editor.csx"

Models.LoadList("Models.txt");

Network.LaneTraffic.AddType("Pedestrians", "歩行者", "#FFFF00");
Network.LaneTraffic.AddType("Buses", "バス", "#00FF00");
Network.LaneTraffic.AddType("NormalCars", "その他自動車", "#0000FF");
Network.LaneTraffic.AddGroup("Cars", "Buses|NormalCars", "#00FFFF");

Network.LaneLayouts.Load("Layout1", "LaneLayout1.xml");
Network.LaneLayouts.Load("Layout2", "LaneLayout2.xml");

Component<TrafficAgents>().AddSpawner<RandomTrafficSpawnerTemplate>("Random");
Component<TrafficAgents>().AddAgent<CarTemplate>("AICar");
Component<TrafficAgents>().Generate("NormalCars", @"Traffic\NormalCars.xml");

Component<TrafficSignals>().AddController("4Forked1", @"SignalControllers\4Forked1.xml");
Component<TrafficSignals>().AddController("4Forked2", @"SignalControllers\4Forked2.xml");
