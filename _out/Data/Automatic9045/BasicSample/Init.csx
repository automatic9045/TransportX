#load "__Editor.csx"

Models.LoadList("Models.txt");

CrossSections.Load("DikeL", "CrossSections/DikeL.xml");
CrossSections.Load("DikeR", "CrossSections/DikeR.xml");
CrossSections.Load("Road1L", "CrossSections/Road1L.xml");
CrossSections.Load("Road1R", "CrossSections/Road1R.xml");
CrossSections.Load("Road2L", "CrossSections/Road2L.xml");
CrossSections.Load("Road2R", "CrossSections/Road2R.xml");
CrossSections.Load("WhiteLine150", "CrossSections/WhiteLine150.xml");
CrossSections.Load("WhiteLine500", "CrossSections/WhiteLine500.xml");
CrossSections.Load("WhiteLineDashed150", "CrossSections/WhiteLineDashed150.xml");
CrossSections.Load("WhiteLineDashed500", "CrossSections/WhiteLineDashed500.xml");
CrossSections.Load("YellowLine150", "CrossSections/YellowLine150.xml");

Network.LaneTraffic.AddType("Pedestrians", "歩行者", "#FFFF00");
Network.LaneTraffic.AddType("Buses", "バス", "#00FF00");
Network.LaneTraffic.AddType("NormalCars", "その他自動車", "#0000FF");
Network.LaneTraffic.AddGroup("Cars", "Buses|NormalCars", "#00FFFF");

Network.LaneLayouts.Load("Layout1", "LaneLayout1.xml");
Network.LaneLayouts.Load("Layout2", "LaneLayout2.xml");
Network.LaneLayouts.Load("LayoutSingle", "LaneLayoutSingle.xml");

Component<TrafficAgents>().AddSpawner<RandomTrafficSpawnerTemplate>("Random");
Component<TrafficAgents>().AddAgent<CarTemplate>("AICar");
Component<TrafficAgents>().Generate("NormalCars", "Traffic/NormalCars.xml");

Component<TrafficSignals>().AddController("4Forked1", "SignalControllers/4Forked1.xml");
Component<TrafficSignals>().AddController("4Forked2", "SignalControllers/4Forked2.xml");
