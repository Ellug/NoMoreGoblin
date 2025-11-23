using UnityEngine;

public class GoblinKidnapState : GoblinState
{
    public GoblinKidnapState(Goblin goblin, GoblinFSM fsm) : base(goblin, fsm) { }

    public override void Enter()
    {
        Debug.Log("Goblin → Kidnap State");

        // 납치 대상 고정
        _goblin.targetPos = null;

        // 여기서 Citizen을 들고 이동하거나,
        // Citizen.SetKidnapped(), 이런식으로 구현 한 뒤에 붙어있게 해야하나?
        // Goblin이 Base로 귀환하는 로직 등 수행
    }

    public override void UpdateLogic()
    {
        if (_goblin.target == null)
        {
            _fsm.ChangeState(_goblin.IdleState);
            return;
        }
    }

    public override void UpdatePhysics()
    {
        _goblin.Move();
    }
}
