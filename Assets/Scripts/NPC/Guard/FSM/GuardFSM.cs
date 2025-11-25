public class GuardFSM
{
    public GuardState CurrentState { get; private set; }

    public void Initialize(GuardState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(GuardState newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}