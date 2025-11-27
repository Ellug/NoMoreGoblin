public abstract class GuardState
{
    protected GuardController _guard;
    protected GuardFSM _fsm;

    public GuardState(GuardController guard, GuardFSM fsm)
    {
        _guard = guard;
        _fsm = fsm;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void UpdateLogic() { }
    public virtual void UpdatePhysics() { }
}
