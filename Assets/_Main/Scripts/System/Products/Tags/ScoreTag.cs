using UnityEngine;

namespace Tags {
public abstract class ScoreTag {
    public int ScoreMult { get; set; }

    public ScoreTag(int scoreMult) {
        ScoreMult = scoreMult;
    }

    public int CalculateScore(int baseScore) {
        return baseScore * ScoreMult;
    }
}

public class ScoreTagMult : ScoreTag {
    public ScoreTagMult(int scoreMult) : base(scoreMult) { }
}

public class ScoreTagFresh : ScoreTag {
    public ScoreTagFresh(int scoreMult) : base(scoreMult) {
        // TODO: modify when changing day phase stuff
        GameManager.Instance.SM_dayPhase.OnStateExit += ExitStateTrigger;
    }

    void ExitStateTrigger(IState<DayPhase> state) {
        if (state.ID == DayPhase.Close) Decay();
    }

    public void Decay() {
        if (ScoreMult == 1) return;
        ScoreMult -= 1;
    }
}

public enum ScoreTagID {
    None = 0,
    Mult = 1,
    Fresh = 2,
    Temperature = 3,
}
}