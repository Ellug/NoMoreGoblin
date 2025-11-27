public class CitizenFSM
{
    public CitizenState CurrentState { get; private set; }

    public void Initialize(CitizenState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(CitizenState newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}