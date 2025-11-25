using UnityEngine;

public class CitizenPatrolState : CitizenState
{
    private float _patrolRadius = 30f;
    private float _detectDurattion = 1f;
    private float _detectTimer;

    public CitizenPatrolState(CitizenController citizen, CitizenFSM fsm) : base(citizen, fsm) { }

    public override void Enter()
    {
        SetNewPatrolPoint();
        _detectTimer = 0;
        _citizen.Anim.SetBool("IsRunning", true);
    }

    public override void Exit()
    {
        // 다음 State에서는 targetPos 사용 안 하므로 초기화
        _citizen.targetPos = null;
    }

    public override void UpdateLogic()
    {
        _detectTimer += Time.deltaTime;
        if (_detectTimer >= _detectDurattion)
        {            
            // 타겟 감지시 도주
            Transform detected = _citizen.DetectTarget();
            if (detected != null)
            {
                _citizen.target = detected;
                _citizen.targetPos = null;
                _fsm.ChangeState(_citizen.FleeState);
                return;
            }
        }

        // PatrolPoint로 이동 완료시 Idle
        if (_citizen.targetPos.HasValue)
        {
            float dist = Vector2.Distance(_citizen.transform.position, _citizen.targetPos.Value);

            if (dist < 1f)
            {
                _citizen.SetIdleState();
                return;
            }
        }
    }

    public override void UpdatePhysics()
    {
        _citizen.Move();
    }

    private void SetNewPatrolPoint()
    {
        if (_citizen.originBaseTrf == null)
            return;

        Vector2 offset = Random.insideUnitCircle * _patrolRadius;
        Vector3 pos = _citizen.originBaseTrf.position + (Vector3)offset;

        _citizen.target = null;
        _citizen.targetPos = pos;
    }
}
