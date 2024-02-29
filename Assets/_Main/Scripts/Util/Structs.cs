using System;
using System.Collections.Generic;

public struct DeltaArgs {
    public int NewValue;
    public int DeltaValue;
}

[Serializable]
public struct DOTweenShakeArgs {
    public float Duration; // .2
    public float Strength; // .1
    public int Vibrato;    // 20
    public float Randomness; //20
}

// Inspector serializable list of lists
[Serializable]
public class ListT<T> {
    public List<T> innerList;
}
[Serializable]
public class ListList<T> {
    public List<ListT<T>> outerList;
}

////////////////////////////   Debug   ////////////////////////////

[Serializable]
public struct DebugDayClockTimes {
    public string DayStartClockTime;
    public string DayEndClockTime;
    public float DayClockTickDurationSeconds;
    public int DayclockTickStepMinutes;
    public string DeliveryPhaseClockTime;
    public string OpenPhaseClockTime;
    public string ClosePhaseClockTime;
}