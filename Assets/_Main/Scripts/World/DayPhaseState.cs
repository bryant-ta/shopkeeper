public abstract class DayPhaseState : IState<DayPhase> {
    public abstract DayPhase ID { get; }
    public abstract IState<DayPhase> NextState();

    public virtual void Enter() { }
    public virtual void Exit() { }
}

public enum DayPhase {
    Delivery = 0,
    Open = 1,
    Close = 2,
}

public class DeliveryDayPhaseState : DayPhaseState {
    public override DayPhase ID { get; }

    public DeliveryDayPhaseState() { ID = DayPhase.Delivery; }

    public override IState<DayPhase> NextState() { return new OpenDayPhaseState(); }
}

public class OpenDayPhaseState : DayPhaseState {
    public override DayPhase ID { get; }

    public OpenDayPhaseState() { ID = DayPhase.Open; }

    public override IState<DayPhase> NextState() { return new CloseDayPhaseState(); }
}

public class CloseDayPhaseState : DayPhaseState {
    public override DayPhase ID { get; }

    public CloseDayPhaseState() { ID = DayPhase.Close; }

    public override IState<DayPhase> NextState() { return new DeliveryDayPhaseState(); }
}