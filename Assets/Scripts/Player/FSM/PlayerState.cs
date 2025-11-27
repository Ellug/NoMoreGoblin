public abstract class PlayerState
{
    protected PlayerController _player;
    protected PlayerFSM _fsm;

    public PlayerState(PlayerController player, PlayerFSM fsm)
    {
        _player = player;
        _fsm = fsm;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void UpdateLogic() { }
    public virtual void UpdatePhysics() { }
}
