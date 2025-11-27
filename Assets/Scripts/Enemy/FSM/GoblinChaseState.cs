using UnityEngine;

public class GoblinChaseState : GoblinState
{
    private const float LoseTargetDistanceMultiplier = 1.2f;
    private float _retargetInterval = 1f;
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
            (_goblin.target.TryGetComponent<IDamageable>(out var t) && !t.IsAlive))
        {
            if (TryRetarget(out var newTarget))
                _goblin.target = newTarget;
            else
                _goblin.SetIdleState();

            return;
        }

        // Citizen 예외 처리
        if (_goblin.target.TryGetComponent<CitizenController>(out var citizen))
        {
            if (citizen.IsInsideHouse || citizen.IsKidnapped)
            {
                _goblin.SetIdleState();
                return;
            }
        }

        // Collider 기반 거리 계산
        float dist;
        if (_goblin.target.TryGetComponent<Collider2D>(out var col))
        {
            Vector2 closest = col.ClosestPoint(_goblin.transform.position);
            dist = Vector2.Distance(_goblin.transform.position, closest);
        }
        else
        {
            dist = Vector2.Distance(_goblin.transform.position, _goblin.target.position);
        }


        // Chase -> Attack 전환 판정 Collider 기준
        if (dist <= _goblin.AttackRange)
        {
            // Kidnap
            if (_goblin.target.CompareTag("Citizen"))
            {
                _goblin.KidnapState.SetCitizen(citizen);
                _fsm.ChangeState(_goblin.KidnapState);
                return;
            }

            // 일반 공격
            if (_goblin.target.CompareTag("Player") ||
                _goblin.target.CompareTag("Guard") ||
                _goblin.target.CompareTag("Building"))
            {
                _fsm.ChangeState(_goblin.AttackState);
                return;
            }
        }

        // Retarget 로직도 Collider 기반으로 검사해야 정확함
        if (dist > _goblin.AttackRange)
        {
            if (TryRetarget(out var newTarget))
            {
                float newDist;

                if (newTarget.TryGetComponent<Collider2D>(out var newCol))
                {
                    Vector2 newClosest = newCol.ClosestPoint(_goblin.transform.position);
                    newDist = Vector2.Distance(_goblin.transform.position, newClosest);
                }
                else
                {
                    newDist = Vector2.Distance(_goblin.transform.position, newTarget.position);
                }

                if (newDist <= _goblin.AttackRange && newDist < dist)
                {
                    _goblin.target = newTarget;
                    _fsm.ChangeState(_goblin.AttackState);
                    return;
                }
            }
        }

        // 너무 멀어지면 Idle
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
