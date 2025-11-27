using UnityEngine;

public class GuardChaseState : GuardState
{
    private const float LoseTargetDistanceMultiplier = 1.2f;
    private float _retargetInterval = 1.5f;
    private float _retargetTimer = 0f;

    public GuardChaseState(GuardController guard, GuardFSM fsm) : base(guard, fsm) { }

    public override void Enter()
    {
        // Chase 들어갈 때는 Transform 타겟 기준으로 이동
        _guard.targetPos = null;
        _retargetTimer = 0f;

        _guard.Anim.SetBool("IsRunning", true);
    }

    public override void UpdateLogic()
    {
        // 타겟 유효성 검사
        if (_guard.target == null ||
            !_guard.target.gameObject.activeInHierarchy ||
            (_guard.target.TryGetComponent<IDamageable>(out var t) && !t.IsAlive)
        )
        {
            _guard.SetIdleState();
            return;
        }

        float dist = Vector2.Distance(_guard.transform.position, _guard.target.position);

        // 기존 타겟이 어택레인지보다 2배 이상 거리일 때
        if (dist > _guard.AttackRange * 2f)
        {
            // 1.5초마다 재탐색 후, dist 보다 가까우면 교체 교체
            if (TryRetarget(out var newTarget))
            {
                float newDist = Vector2.Distance(_guard.transform.position, newTarget.position);

                if (newDist <= _guard.AttackRange && newDist < dist)
                {                    
                    _guard.target = newTarget;
                    _fsm.ChangeState(_guard.AttackState);
                    return;
                }

            }
        }

        // 공격 사거리 안에 들어오면 공격
        if (dist <= _guard.AttackRange)
        {
            if (_guard.target.CompareTag("Enemy"))
            {                
                _fsm.ChangeState(_guard.AttackState);
                return;
            }
        }

        // 너무 멀어지면 타겟 포기 -> Idle
        if (dist > _guard.DetectRange * LoseTargetDistanceMultiplier)
        {
            _guard.SetIdleState();
            return;
        }
    }

    public override void UpdatePhysics()
    {
        _guard.Move();
    }

    private bool TryRetarget(out Transform newTarget)
    {
        _retargetTimer += Time.deltaTime;
        if (_retargetTimer < _retargetInterval)
        {
            newTarget = null;
            return false;
        }

        // 타이머 초기화
        _retargetTimer = 0f;

        // 실제 탐색
        newTarget = _guard.DetectTarget();
        return newTarget != null;
    }
}
