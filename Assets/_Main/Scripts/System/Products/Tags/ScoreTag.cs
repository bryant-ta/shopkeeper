using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Tags {
[Serializable]
public abstract class ScoreTag {
    [field: SerializeField, ReadOnly] protected ScoreTagID id;

    [SerializeField] protected int scoreMult = 0;
    public int ScoreMult => scoreMult;

    public int CalculateScore(int baseScore) { return baseScore * ScoreMult; }
    public void ModifyScoreMult(int delta) {
        int newScoreMult = scoreMult + delta;
        if (newScoreMult < 0) newScoreMult = 0;

        scoreMult = newScoreMult;
    }
}

public class ScoreTagMult : ScoreTag {
    public ScoreTagMult() { id = ScoreTagID.Mult; }
}

public class ScoreTagFresh : ScoreTag {
    public ScoreTagFresh() {
        id = ScoreTagID.Fresh;

        // TODO: modify when changing day phase stuff
        GameManager.Instance.SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Close) Decay();
    }

    void Decay() {
        if (scoreMult == 1) return;
        scoreMult -= 1;
    }
}

public enum ScoreTagID {
    None = 0,
    Mult = 1,
    Fresh = 2,
    Temperature = 3,
}

public static class LookUpScoreTag {
    static Dictionary<ScoreTagID, ScoreTag> LookUpDict = new Dictionary<ScoreTagID, ScoreTag> {
        {ScoreTagID.None, null},
        {ScoreTagID.Mult, new ScoreTagMult()},
        {ScoreTagID.Fresh, new ScoreTagFresh()},
    };

    public static ScoreTag LookUp(ScoreTagID id, int scoreMult) {
        ScoreTag tag = LookUpDict[id];
        if (tag == null) {
            if (id == ScoreTagID.None) return null;
            Debug.LogError("Unable to look up Score Tag ID.");
            return null;
        }

        tag.ModifyScoreMult(scoreMult);

        return tag;
    }
}
}