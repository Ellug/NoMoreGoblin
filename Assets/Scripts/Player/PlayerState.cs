public abstract class PlayerState
{
    protected PlayerController _player;
    protected PlayerStateMachine _fsm;

    public PlayerState(PlayerController player, PlayerStateMachine fsm)
    {
        _player = player;
        _fsm = fsm;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void UpdateLogic() { }
    public virtual void UpdatePhysics() { }
}
