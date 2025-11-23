public class GoblinFSM
{
    public GoblinState CurrentState { get; private set; }

    public void Initialize(GoblinState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(GoblinState newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
