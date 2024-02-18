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