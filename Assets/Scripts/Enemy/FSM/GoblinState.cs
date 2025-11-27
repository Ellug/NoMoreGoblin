public abstract class GoblinState
{
    protected Goblin _goblin;
    protected GoblinFSM _fsm;

    protected GoblinState(Goblin goblin, GoblinFSM fsm)
    {
        _goblin = goblin;
        _fsm = fsm;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void UpdateLogic() { }
    public virtual void UpdatePhysics() { }
}