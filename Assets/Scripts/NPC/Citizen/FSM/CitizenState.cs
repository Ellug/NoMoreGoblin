public abstract class CitizenState
{
    protected CitizenController _citizen;
    protected CitizenFSM _fsm;

    public CitizenState(CitizenController citizen, CitizenFSM fsm)
    {
        _citizen = citizen;
        _fsm = fsm;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void UpdateLogic() { }
    public virtual void UpdatePhysics() { }
}
