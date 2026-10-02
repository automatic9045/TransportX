#load "__Editor.csx"

{
    Signals.ToSwitchCounter("HornStartCount", "Horn", true);
    Signals.ToSwitchCounter("HornEndCount", "Horn", false);


    Sounds.Create3D("HornStart", "HornStart", 0, 0.6, -0.2, 30, 0, 0, 1)
        .PlayStopWhen("HornStartCount", "HornEndCount");

    double hornVolume = 0;
    double hornEndVolume = 0;
    Sounds.Create3D("Horn", "Horn", 0, 0.6, -0.2, 30, 0, 0, 1)
        .Loop()
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

            return hornVolume;
        });

    Sounds.Create3D("HornEnd", "HornEnd", 0, 0.6, -0.2, 30, 0, 0, 1)
        .PlayWhen("HornEndCount")
        .Volume(elapsed => hornEndVolume);
}
