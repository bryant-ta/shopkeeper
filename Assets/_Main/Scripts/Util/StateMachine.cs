using System;

public class StateMachine<T>  {
    public IState<T> CurState { get; private set; }
    IState<T> initialState;

    public event Action<IState<T>> OnStateEnter;
    public event Action<IState<T>> OnStateExit;

    public StateMachine(IState<T> initialState) {
        this.initialState = initialState;
        CurState = null;
    }

    public void ExecuteNextState() {
        if (CurState != null) {
            CurState.Exit();
            OnStateExit?.Invoke(CurState);

            CurState = CurState.NextState();

            // When current state was terminal, do nothing (Must ExecuteNextState() to enter into initial state again)
            if (CurState == null) {
                return;
            }
        }
        else { // When first executed, enter into initial state
            CurState = initialState;
        }

        CurState.Enter();
        OnStateEnter?.Invoke(CurState);
    }

    public void Reset() {
        CurState = null;
    }
}

/// <remarks>
/// T should be an enum of State IDs. Considered just making ID of type int, but specifying generic type is a reminder of what type of
/// State you are working with. However, it does make the code "messier" and can still use non enum as ID. Maybe thats a plus?
/// </remarks>
public interface IState<out T>  {
    public T ID { get; }
    public IState<T> NextState();

    public void Enter();
    public void Exit();
}