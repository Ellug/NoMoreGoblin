using UnityEngine;

public class GuardPatrolState : GuardState
{
    private float _patrolRadius = 25f;
    private float _detectDurattion = 1f;
    private float _detectTimer;

    public GuardPatrolState(GuardController guard, GuardFSM fsm) : base(guard, fsm) { }

    public override void Enter()
    {
        SetNewPatrolPoint();
        _guard.Anim.SetBool("IsRunning", true);
    }

    public override void Exit()
    {
        // 다음 State에서는 targetPos 사용 안 하므로 초기화
        _guard.targetPos = null;
    }

    public override void UpdateLogic()
    {
        _detectTimer += Time.deltaTime;
        if (_detectTimer >= _detectDurattion)
        {            
            // 타겟 감지시 Chase
            Transform detected = _guard.DetectTarget();
            if (detected != null)
            {
                _guard.target = detected;
                _guard.targetPos = null;
                _fsm.ChangeState(_guard.ChaseState);
                return;
            }
        }

        // PatrolPoint로 이동 완료시 Idle
        if (_guard.targetPos.HasValue)
        {
            float dist = Vector2.Distance(_guard.transform.position, _guard.targetPos.Value);

            if (dist < 1f)
            {
                _guard.SetIdleState();
                return;
            }
        }
    }

    public override void UpdatePhysics()
    {
        _guard.Move();
    }

    private void SetNewPatrolPoint()
    {
        if (_guard.originBaseTrf == null)
            return;

        Vector2 offset = Random.insideUnitCircle * _patrolRadius;
        Vector3 pos = _guard.originBaseTrf.position + (Vector3)offset;

        _guard.target = null;
        _guard.targetPos = pos;
    }
}
