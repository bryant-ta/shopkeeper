using System;
using System.Collections.Generic;
using UnityEngine;

public struct ClickInputArgs {
    public Vector3 HitNormal;
    public Vector3 HitPoint;
    public GameObject TargetObj;
}

public struct MoveInputArgs {
    public Vector2 MoveInput;
}

public struct DeltaArgs {
    public int NewValue;
    public int DeltaValue;
}

[Serializable]
public struct DOTweenShakeArgs {
    public float Duration;
    public float Strength;
    public int Vibrato;
    public float Randomness;
}

[Serializable]
public struct MinMax {
    public int Min;
    public int Max;
}
    
[Serializable]
public struct ObjDifficulty {
    public float Day;
    public GameObject Obj;
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