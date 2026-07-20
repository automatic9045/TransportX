#load "__Editor.csx"

enum AmtPhase
{
    Idle,
    ThrottleCut,
    ClutchDisengage,
    GearChange,
    ClutchEngage,
}

{
    float BestRpm = 1500;
    float MinRpm = 600;
    float MaxRpm = 2500;

    TimeSpan Duration_Idle = TimeSpan.FromSeconds(0.2);
    TimeSpan Duration_ThrottleCut1 = TimeSpan.FromSeconds(0.2);
    TimeSpan Duration_ThrottleCut2 = TimeSpan.FromSeconds(0.25);
    TimeSpan Duration_ClutchDisengage = TimeSpan.FromSeconds(0.4);
    TimeSpan Duration_GearChange = TimeSpan.FromSeconds(0.4);
    TimeSpan Duration_ClutchEngage = TimeSpan.FromSeconds(0.5);

    var stateMachine = new TransportX.Mathematics.TickStateMachine<AmtPhase>(AmtPhase.Idle);
    var lockupHysteresis = new TransportX.Mathematics.Hysteresis(700, 600, false);

    var upshiftMap = new TransportX.Mathematics.Surface([
        (2, 0, 1800),
        (2, 0.5, 2050),
        (2, 1, 2300),

        (3, 0, 1900),
        (3, 0.5, 1900),
        (3, 1, 2300),

        (4, 0, 1750),
        (4, 0.5, 1750),
        (4, 1, 2100),

        (5, 0, 1650),
        (5, 0.5, 1650),
        (5, 1, 2000),
    ]);

    var downshiftMap = new TransportX.Mathematics.Surface([
        (3, 0, 600),
        (3, 1, 800),

        (4, 0, 800),
        (4, 1, 1200),

        (5, 0, 900),
        (5, 1, 1200),

        (6, 0, 1000),
        (6, 1, 1200),
    ]);

    var engine = (EngineFactory)Component<Powertrain>().Modules.Factories["Engine"];
    var clutch = (FluidClutchFactory)Component<Powertrain>().Modules.Factories["Clutch"];
    var gearbox = (GearboxFactory)Component<Powertrain>().Modules.Factories["Gearbox"];

    int srcGear = 0;
    int destGear = 0;

    string lastShifterPosition = "N";
    int lastShifterMUpCount = 0;
    int lastShifterMDownCount = 0;


    var shifter = Component<Powertrain>().Controllers.AddShifter("Shifter")
        .UpDownButton("ShifterUp", "ShifterDown")
        .LeftRightButton("ShifterLeft", "ShifterRight");

    var slotN = shifter.RootSlot("N");
    var slotR = slotN.SlotUpParallel("R");
    var slotD = slotN.SlotDownParallel("D");
    var slotM = slotD.SlotRightParallel("M")
        .ActionUp("ShifterMUp")
        .ActionDown("ShifterMDown");

    Component<Powertrain>().Controllers.AddScriptable("TCU")
        .OnInit(TcuInit)
        .OnTick(TcuTick);

        
    void TcuInit()
    {
        var minThrottle = Signals.WriteFloat("TcuMinThrottle", 0);
        var maxThrottle = Signals.WriteFloat("TcuMaxThrottle", 1);
        var clutchEngagement = Signals.WriteFloat("TcuClutch", 1);

        srcGear = gearbox.BuiltModule.Gear;
        destGear = gearbox.BuiltModule.Gear;

        stateMachine.AddState(TickState<AmtPhase>.Create(AmtPhase.Idle)
            .OnTick((elapsed, stateTime) =>
            {
                minThrottle.Value = 0;
                maxThrottle.Value = 1;
                clutchEngagement.Value = 1;
                gearbox.BuiltModule.Gear = destGear;
            }));

        stateMachine.AddState(TickState<AmtPhase>.Create(AmtPhase.ThrottleCut)
            .OnTick((elapsed, stateTime) =>
            {
                minThrottle.Value = 0;
                if (stateTime < Duration_Idle)
                {
                    maxThrottle.Value = Signals.ReadFloat("PedalThrottle") * 0.625f;
                }
                else
                {
                    float amount = (float)((stateTime - Duration_Idle) / Duration_ThrottleCut1);
                    maxThrottle.Value = float.Lerp(0.5f, 0, amount);
                }
            })
            .EvaluateTransition((elapsed, stateTime) =>
            {
                if (Duration_Idle + Duration_ThrottleCut1 <= stateTime) return AmtPhase.ClutchDisengage;
                else return AmtPhase.ThrottleCut;
            }));

        stateMachine.AddState(TickState<AmtPhase>.Create(AmtPhase.ClutchDisengage)
            .OnEnter(() =>
            {
                minThrottle.Value = 0;
                maxThrottle.Value = 0;
            })
            .OnTick((elapsed, stateTime) =>
            {
                float amount = (float)(stateTime / Duration_ThrottleCut2);
                clutchEngagement.Value = float.Lerp(1, 0, amount);
            })
            .EvaluateTransition((elapsed, stateTime) =>
            {
                if (Duration_ThrottleCut2 <= stateTime) return AmtPhase.GearChange;
                else return AmtPhase.ClutchDisengage;
            }));

        stateMachine.AddState(TickState<AmtPhase>.Create(AmtPhase.GearChange)
            .OnEnter(() =>
            {
                minThrottle.Value = 0;
                maxThrottle.Value = 0;
                clutchEngagement.Value = 0;
                gearbox.BuiltModule.Gear = 0; // まずNへ抜く
            })
            .OnTick((elapsed, stateTime) =>
            {
                if (Duration_ClutchDisengage <= stateTime && gearbox.BuiltModule.Gear == 0)
                {
                    gearbox.BuiltModule.Gear = destGear; // 抜くラグが明けたら次のギアを噛ませる
                }
            })
            .EvaluateTransition((elapsed, stateTime) =>
            {
                if (Duration_ClutchDisengage + Duration_GearChange <= stateTime) return AmtPhase.ClutchEngage;
                else return AmtPhase.GearChange;
            }));

        stateMachine.AddState(TickState<AmtPhase>.Create(AmtPhase.ClutchEngage)
            .OnTick((elapsed, stateTime) =>
            {
                float amount = (float)(stateTime / Duration_ClutchEngage);
                maxThrottle.Value = float.Lerp(0, 1, amount);

                if (0 < destGear && destGear < srcGear && 100 < engine.BuiltModule.Rpm)
                {
                    minThrottle.Value = float.Min(0.75f, float.Lerp(2, 0, amount));
                    maxThrottle.Value = float.Max(0.75f, maxThrottle.Value);
                }
                else
                {
                    minThrottle.Value = 0;
                }

                clutchEngagement.Value = float.Max(0, float.Lerp(-0.5f, 1, amount));
            })
            .EvaluateTransition((elapsed, stateTime) =>
            {
                if (Duration_ClutchEngage <= stateTime) return AmtPhase.Idle;
                else return AmtPhase.ClutchEngage;
            }));
    }

    void TcuTick(TimeSpan elapsed)
    {
        float pedalThrottle = Signals.ReadFloat("PedalThrottle");

        // --------------------------------------------------
        // シフト操作の受け入れ
        // --------------------------------------------------

        var shifterMUp = Signals.Int("ShifterMUp");
        var shifterMDown = Signals.Int("ShifterMDown");

        var shifterPosition = shifter.BuiltController.Lever.Position;
        switch (shifterPosition.Key)
        {
            case "R":
            {
                if (lastShifterPosition != shifterPosition.Key) TcuRequestShift(-1);
                break;
            }

            case "N":
            {
                if (lastShifterPosition != shifterPosition.Key) TcuRequestShift(0);
                break;
            }

            case "D":
            {
                if (stateMachine.State == AmtPhase.Idle)
                {
                    int gear = gearbox.BuiltModule.Gear;
                    float outputRpm = gearbox.Output.BuiltShaft.Rpm;

                    if (lastShifterPosition == shifterPosition.Key)
                    {
                        float theoreticalRpm = outputRpm * gearbox.BuiltModule.GetGearRatio(gear);

                        float upshiftRpm = upshiftMap.GetValue(gear, pedalThrottle);
                        float downshiftRpm = downshiftMap.GetValue(gear, pedalThrottle);

                        if (upshiftRpm < theoreticalRpm && gear < gearbox.BuiltModule.MaxGear)
                        {
                            TcuRequestShift(gear + 1);
                        }
                        else if (theoreticalRpm < downshiftRpm && 2 < gear)
                        {
                            TcuRequestShift(gear - 1);
                        }
                    }
                    else
                    {
                        int bestGear = 2;
                        float bestDiff = float.MaxValue;

                        for (int g = 2; g <= gearbox.BuiltModule.MaxGear; g++)
                        {
                            float estimatedRpm = outputRpm * gearbox.BuiltModule.GetGearRatio(g);

                            float diff = float.Abs(estimatedRpm - BestRpm);
                            if (diff < bestDiff && MinRpm < estimatedRpm && estimatedRpm < MaxRpm)
                            {
                                bestGear = g;
                                bestDiff = diff;
                            }
                        }

                        TcuRequestShift(bestGear);
                    }
                }
                break;
            }

            case "M":
            {
                int gear = gearbox.BuiltModule.Gear;
                float outputRpm = gearbox.Output.BuiltShaft.Rpm;

                float theoreticalRpm = gearbox.Output.BuiltShaft.Rpm * gearbox.BuiltModule.GetGearRatio(gearbox.BuiltModule.Gear);

                if (lastShifterMUpCount != shifterMUp.Value && MinRpm < theoreticalRpm)
                {
                    TcuRequestShift(destGear + 1);
                }

                if (1 < destGear && lastShifterMDownCount != shifterMDown.Value)
                {
                    TcuRequestShift(destGear - 1);
                }
                break;
            }
        }

        lastShifterPosition = shifterPosition.Key;
        lastShifterMUpCount = shifterMUp.Value;
        lastShifterMDownCount = shifterMDown.Value;


        // --------------------------------------------------
        // エンジン、クラッチ、ギアの制御
        // --------------------------------------------------

        stateMachine.Tick(elapsed);
        
        float clutchEngagement = Signals.ReadFloat("TcuClutch");

        var minThrottle = Signals.Float("TcuMinThrottle");
        var maxThrottle = Signals.Float("TcuMaxThrottle");
        float currentMinThrottle = minThrottle.Value;
        float currentMaxThrottle = maxThrottle.Value;

        bool isShifting = stateMachine.State != AmtPhase.Idle;
        bool immediateLockup = stateMachine.State == AmtPhase.ClutchEngage;
        bool lockup = clutch.BuiltModule.Lockup;

        if (gearbox.BuiltModule.Gear == 0) lockup = false;
        else if (immediateLockup) lockup = true;
        else if (4 <= gearbox.BuiltModule.Gear)
        {
            lockup = 600 <= engine.BuiltModule.Rpm;
        }
        else
        {
            float lockupRpm = float.Lerp(1400, 1500, float.Clamp((pedalThrottle - 0.5f) / 0.5f, 0, 1));
            lockupHysteresis.ThresholdHigh = lockupRpm;
            lockup = lockupHysteresis.Next(engine.BuiltModule.Rpm);

            if (engine.BuiltModule.Rpm < 600) lockup = false;
        }

        if (clutch.BuiltModule.LockupRate == 0)
        {
            maxThrottle.Value = float.Max(currentMinThrottle, currentMaxThrottle * 0.6f);
        }
        else if (clutch.BuiltModule.LockupRate < 1)
        {
            maxThrottle.Value = float.Max(currentMinThrottle, currentMaxThrottle * 0.175f);
        }

        if (!isShifting && engine.BuiltModule.Rpm < 550)
        {
            clutchEngagement = float.Clamp((engine.BuiltModule.Rpm - 500) / 50, 0, clutchEngagement);
        }

        clutch.BuiltModule.Lockup = lockup;
        clutch.BuiltModule.LockupMode = immediateLockup 
            ? TransportX.Domains.RoadVehicles.Powertrain.Modules.FluidClutch.LockupResponseMode.Immediate 
            : TransportX.Domains.RoadVehicles.Powertrain.Modules.FluidClutch.LockupResponseMode.Normal;
        
        clutch.BuiltModule.Engagement = clutchEngagement;
    }
    
    void TcuRequestShift(int targetGear) 
    {
        var gearbox = (GearboxFactory)Component<Powertrain>().Modules.Factories["Gearbox"];

        targetGear = int.Clamp(targetGear, -gearbox.BuiltModule.MinGear, gearbox.BuiltModule.MaxGear);
        if (targetGear == destGear) return;
        
        srcGear = gearbox.BuiltModule.Gear;
        destGear = targetGear;

        if (destGear == 0)
        {
            stateMachine.TransitionTo(AmtPhase.ClutchDisengage);
        }
        else if (stateMachine.State == AmtPhase.GearChange || stateMachine.State == AmtPhase.ClutchEngage
            || srcGear == 0 || (0 < destGear && destGear < srcGear) || Signals.ReadFloat("PedalThrottle") < 0.05f)
        {
            stateMachine.TransitionTo(AmtPhase.ClutchDisengage);
        }
        else
        {
            stateMachine.TransitionTo(AmtPhase.ThrottleCut);
        }
    }
}
