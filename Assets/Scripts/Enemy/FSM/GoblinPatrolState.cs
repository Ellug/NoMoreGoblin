using UnityEngine;

public class GoblinPatrolState : GoblinState
{
    private float _patrolRadius = 40f;
    private float _detectDurattion = 1f;
    private float _detectTimer;

    public GoblinPatrolState(Goblin goblin, GoblinFSM fsm) : base(goblin, fsm) { }

    public override void Enter()
    {
        SetNewPatrolPoint();
        _goblin.Anim.SetBool("IsRunning", true);
    }

    public override void Exit()
    {
        // 다음 State에서는 targetPos 사용 안 하므로 초기화
        _goblin.targetPos = null;
    }

    public override void UpdateLogic()
    {
        _detectTimer += Time.deltaTime;
        if (_detectTimer >= _detectDurattion)
        {            
            // 타겟 감지시 Chase
            Transform detected = _goblin.DetectTarget();
            if (detected != null)
            {
                _goblin.target = detected;
                _goblin.targetPos = null;
                _fsm.ChangeState(_goblin.ChaseState);
                return;
            }
        }

        // PatrolPoint로 이동 완료시 Idle
        if (_goblin.targetPos.HasValue)
        {
            float dist = Vector2.Distance(_goblin.transform.position, _goblin.targetPos.Value);

            if (dist < 1f)
            {
                _goblin.SetIdleState();
                return;
            }
        }
    }

    public override void UpdatePhysics()
    {
        _goblin.Move();
    }

    private void SetNewPatrolPoint()
    {
        if (_goblin.originBaseTrf == null) return;
        if (_goblin.targetPos != null) return;

        Vector3 safePos;

        bool found = SafePatrolPoint.TryGetSafePatrolPoint(_goblin.originBaseTrf.position, _patrolRadius, out safePos);

        if (found)
        {
            _goblin.target = null;
            _goblin.targetPos = safePos;
        }
        else
        {
            // 실패시 Idle
            _goblin.SetIdleState();
        }
    }
}
