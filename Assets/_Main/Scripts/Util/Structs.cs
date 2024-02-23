using System;

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