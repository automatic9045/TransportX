#load "__Editor.csx"
#load "Spec.csx"

Spec.SetSize(Width, Height, Length);

Viewpoints.AddDriver(0.67, 1.8, -1.35, 4, 0, 0, 50);
Viewpoints.AddDriver(0.67, 1.8, -1.35, 0, 15, 0, 55);
Viewpoints.AddDriver(0.67, 1.8, -1.35, 0, 60, 0, 50);
Viewpoints.AddDriver(0.67, 1.8, -1.35, 9, -75, 0, 50);
Viewpoints.AddDriver(0.67, 1.8, -1.35, 0, -25, 0, 55);

Viewpoints.AddPasssenger(-0.9, 2, -9.5, 5, 0, 0);
Viewpoints.AddPasssenger(0.3, 1.8, -6, 0, -90, 0, 50);

Viewpoints.SetBird(0, 2, -3, 20, 18, 0);

Models.LoadList("Models.txt");
Sounds.LoadList("Sounds.txt");
