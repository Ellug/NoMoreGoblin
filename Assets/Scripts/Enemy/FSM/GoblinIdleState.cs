using UnityEngine;

public class GoblinIdleState : GoblinState
{
    private float _idleTimer;
    private float _idleDuration;
    private float _detectDurattion = 2f;
    private float _detectTimer;

    public GoblinIdleState(Goblin goblin, GoblinFSM fsm) : base(goblin, fsm) { }

    public override void Enter()
    {
        _idleTimer = 0f;
        _detectTimer = 0f;
        _idleDuration = Random.Range(5f, 20f);

        _goblin.target = null;
        _goblin.targetPos = null;

        _goblin.Anim.SetBool("IsRunning", false);
    }

    public override void UpdateLogic()
    {
        _detectTimer += Time.deltaTime;
        if (_detectTimer >= _detectDurattion)
        {
            _detectTimer = 0f;
            // 타겟 감지 -> Chase
            Transform detected = _goblin.DetectTarget();
            if (detected != null)
            {
                _goblin.target = detected;
                _fsm.ChangeState(_goblin.ChaseState);
                return;
            }
        }

        // Idle 시간 경과 -> Patrol로 전환
        _idleTimer += Time.deltaTime;        
        if (_idleTimer >= _idleDuration)
        {
            _fsm.ChangeState(_goblin.PatrolState);
            return;
        }
    }
}
