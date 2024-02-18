using System;

public class StateMachine<T>  {
    public IState<T> CurState { get; private set; }

    public event Action<IState<T>> OnStateEnter;
    public event Action<IState<T>> OnStateExit;

    public StateMachine(IState<T> initialState) {
        CurState = initialState;
    }

    public void ExecuteNextState() {
        // Exit current state
        CurState.Exit();
        OnStateExit?.Invoke(CurState);
        
        // Enter next state
        CurState = CurState.NextState();
        CurState.Enter();
        OnStateEnter?.Invoke(CurState);
    }
}
