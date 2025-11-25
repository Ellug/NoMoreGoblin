using UnityEngine;

public class GuardIdleState : GuardState
{
    private float _idleTimer;
    private float _idleDuration;
    private float _healDuration = 10f;
    private float _detectDurattion = 2f;
    private float _detectTimer;

    public GuardIdleState(GuardController guard, GuardFSM fsm) : base(guard, fsm) { }

    public override void Enter()
    {
        _idleTimer = 0f;
        _detectTimer = 0f;
        _idleDuration = Random.Range(5f, 20f);

        _guard.target = null;
        _guard.targetPos = null;

        _guard.Anim.SetBool("IsRunning", false);
    }

    public override void UpdateLogic()
    {
        _detectTimer += Time.deltaTime;
        if (_detectTimer >= _detectDurattion)
        {
            _detectTimer = 0f;
            // 타겟 감지 -> Chase
            Transform detected = _guard.DetectTarget();
            if (detected != null)
            {
                _guard.target = detected;
                _fsm.ChangeState(_guard.ChaseState);
                return;
            }
        }

        _idleTimer += Time.deltaTime;

        // 체력 100% 일 때만 패트롤, 아니면 힐
        if (_guard.CurHp == _guard.MaxHp)
        {            
            // Idle 시간 경과 -> Patrol로 전환
            if (_idleTimer >= _idleDuration)
            {
                _fsm.ChangeState(_guard.PatrolState);
                return;
            }
        }
        else
        {
            if (_idleTimer >= _healDuration)
            {
                _guard.TakeHeal();
                _idleTimer = 0f;                
            }
        }
    }
}
