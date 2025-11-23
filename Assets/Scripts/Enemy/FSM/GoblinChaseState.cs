using UnityEngine;

public class GoblinChaseState : GoblinState
{
    private const float LoseTargetDistanceMultiplier = 1.2f;

    public GoblinChaseState(Goblin goblin, GoblinFSM fsm) : base(goblin, fsm) { }

    public override void Enter()
    {
        // Chase 들어갈 때는 Transform 타겟 기준으로 이동
        _goblin.targetPos = null;
    }

    public override void UpdateLogic()
    {
        if (_goblin.target == null)
        {
            _fsm.ChangeState(_goblin.IdleState);
            return;
        }

        float dist = Vector2.Distance(_goblin.transform.position, _goblin.target.position);

        // 공격 사거리 안에 들어오면 공격 or 납치
        if (dist <= _goblin.AttackRange)
        {
            if (_goblin.target.CompareTag("Citizen"))
            {
                _fsm.ChangeState(_goblin.KidnapState);
                return;
            }

            if (_goblin.target.CompareTag("Player") ||
                _goblin.target.CompareTag("Guard") ||
                _goblin.target.CompareTag("Building"))
            {                
                _fsm.ChangeState(_goblin.AttackState);
                return;
            }
        }

        // 너무 멀어지면 타겟 포기 -> Idle
        if (dist > _goblin.DetectRange * LoseTargetDistanceMultiplier)
        {
            _goblin.target = null;
            _goblin.targetPos = null;
            _fsm.ChangeState(_goblin.IdleState);
            return;
        }
    }

    public override void UpdatePhysics()
    {
        _goblin.Move();
    }
}
