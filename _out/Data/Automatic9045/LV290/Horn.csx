#load "__Editor.csx"

{
    Signals.ToBoolSwitchCounter("HornStartCount", "Horn", true);
    Signals.ToBoolSwitchCounter("HornEndCount", "Horn", false);


    Sounds.CreateSource3D("HornStart", 0, 0.6, -0.2, 30, 0, 0, 1)
        .PlayStopWhen("HornStart", "HornStartCount", "HornEndCount")
        .Volume(0.25);

    double hornVolume = 0;
    double hornEndVolume = 0;
    Sounds.CreateSource3D("Horn", 0, 0.6, -0.2, 30, 0, 0, 1)
        .Loop("Horn")
        .Volume(elapsed =>
        {
            if (Signals.ReadBool("Horn"))
            {
                hornVolume = double.Min(hornVolume + elapsed.TotalSeconds * 100, 1);
            }
            else
            {
                hornEndVolume = hornVolume;
                hornVolume = 0;
            }

            return hornVolume * 0.25f;
        });

    Sounds.CreateSource3D("HornEnd", 0, 0.6, -0.2, 30, 0, 0, 1)
        .PlayWhen("HornEnd", "HornEndCount")
        .Volume(elapsed => hornEndVolume * 0.25f);
}
