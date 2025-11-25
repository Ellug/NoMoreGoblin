using UnityEngine;

public class GoblinChaseState : GoblinState
{
    private const float LoseTargetDistanceMultiplier = 1.2f;
    private float _retargetInterval = 1.5f;
    private float _retargetTimer = 0f;

    public GoblinChaseState(Goblin goblin, GoblinFSM fsm) : base(goblin, fsm) { }

    public override void Enter()
    {
        // Chase 들어갈 때는 Transform 타겟 기준으로 이동
        _goblin.targetPos = null;
        _retargetTimer = 0f;

        _goblin.Anim.SetBool("IsRunning", true);
    }

    public override void UpdateLogic()
    {
        // Target 유효성 검사
        if (_goblin.target == null ||
            !_goblin.target.gameObject.activeInHierarchy ||
            (_goblin.target.TryGetComponent<IDamageable>(out var t) && !t.IsAlive)
        )
        {
            _goblin.SetIdleState();
            return;
        }

        // 이미 다른 고블린에게 납치된 시민이면 추적 포기
        if (_goblin.target.TryGetComponent<CitizenController>(out var citizen) && citizen.IsKidnapped)
        {
            _goblin.SetIdleState();
            return;
        }

        float dist = Vector2.Distance(_goblin.transform.position, _goblin.target.position);

        // 기존 타겟이 어택레인지보다 2배 이상 거리일 때
        if (dist > _goblin.AttackRange * 2f)
        {
            // 1.5초마다 재탐색 후, dist 보다 가까우면 교체 교체
            if (TryRetarget(out var newTarget))
            {
                float newDist = Vector2.Distance(_goblin.transform.position, newTarget.position);

                if (newDist <= _goblin.AttackRange && newDist < dist)
                {                    
                    _goblin.target = newTarget;
                    _fsm.ChangeState(_goblin.AttackState);
                    return;
                }
            }
        }

        // 공격 사거리 안에 들어오면 공격 or 납치
        if (dist <= _goblin.AttackRange)
        {
            // 시민이면 납치
            if (_goblin.target.CompareTag("Citizen"))
            {
                _goblin.KidnapState.SetCitizen(citizen);
                _fsm.ChangeState(_goblin.KidnapState);
                return;
            }

            // 얘넨 공격
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
            _goblin.SetIdleState();
            return;
        }
    }

    public override void UpdatePhysics()
    {
        _goblin.Move();
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
        newTarget = _goblin.DetectTarget();
        return newTarget != null;
    }
}
