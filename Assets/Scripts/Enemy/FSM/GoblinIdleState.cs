using UnityEngine;

public class GoblinIdleState : GoblinState
{
    private float _waitTimer;
    private float _waitDuration;
    private bool  _isWaiting;
    private float _patrolRadius = 30f;

    private float _detectTimer;
    private float _nextDetectTime;

    public GoblinIdleState(Goblin goblin, GoblinFSM fsm) : base(goblin, fsm) { }

    public override void Enter()
    {
        _isWaiting = false;
        _waitTimer = 0f;
        _waitDuration = 0f;

        _detectTimer = 0f;
        _nextDetectTime = Random.Range(0.8f, 1.5f);

        SetNewPatrolPoint();
    }

    public override void UpdateLogic()
    {
        // 디텍트 (랜덤 간격)
        _detectTimer += Time.deltaTime;
        if (_detectTimer >= _nextDetectTime)
        {
            _detectTimer = 0f;
            _nextDetectTime = Random.Range(0.8f, 1.5f);

            Transform detected = _goblin.DetectTarget();
            if (detected != null)
            {
                _goblin.target = detected;
                _goblin.targetPos = null;
                _fsm.ChangeState(_goblin.ChaseState);
                return;
            }
        }

        // 패트롤 도착 체크 / 대기
        if (!_isWaiting)
        {
            if (_goblin.targetPos.HasValue)
            {
                float dist = Vector2.Distance(
                    _goblin.transform.position,
                    _goblin.targetPos.Value
                );

                if (dist < 0.8f)
                    StartWaiting();
            }
        }
        else
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= _waitDuration)
                EndWaiting();
        }
    }

    public override void UpdatePhysics()
    {
        if (!_isWaiting)
            _goblin.Move();
    }

    public override void Exit()
    {
        _goblin.targetPos = null;
    }

    private void SetNewPatrolPoint()
    {
        if (_goblin.originBaseTrf == null)
            return;

        Vector2 offset = Random.insideUnitCircle * _patrolRadius;
        Vector3 pos = _goblin.originBaseTrf.position + (Vector3)offset;

        _goblin.target = null;
        _goblin.targetPos = pos;
        _isWaiting = false;
    }

    private void StartWaiting()
    {
        _isWaiting = true;
        _waitTimer = 0f;
        _waitDuration = Random.Range(3f, 8f);   // 대기 시간
        _goblin.targetPos = null;
    }

    private void EndWaiting()
    {
        SetNewPatrolPoint();
        _isWaiting = false;
    }
}
