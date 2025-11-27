using UnityEngine;

public class CitizenIdleState : CitizenState
{
    private float _idleTimer;
    private float _idleDuration;
    private float _healDuration = 10f;
    private float _detectDurattion = 2f;
    private float _detectTimer;

    public CitizenIdleState(CitizenController citizen, CitizenFSM fsm) : base(citizen, fsm) { }

    public override void Enter()
    {
        _idleTimer = 0f;
        _detectTimer = 0f;
        _idleDuration = Random.Range(3f, 7f);

        _citizen.target = null;
        _citizen.targetPos = null;

        _citizen.Anim.SetBool("IsRunning", false);
    }

    public override void UpdateLogic()
    {
        _detectTimer += Time.deltaTime;
        if (_detectTimer >= _detectDurattion)
        {
            _detectTimer = 0f;
            // 타겟 감지 -> 도주
            Transform detected = _citizen.DetectTarget();
            if (detected != null)
            {
                _citizen.target = detected;
                _fsm.ChangeState(_citizen.FleeState);
                return;
            }
        }

        _idleTimer += Time.deltaTime;

        // 체력 100% 일 때만 패트롤, 아니면 힐
        if (_citizen.CurHp == _citizen.MaxHp)
        {            
            // Idle 시간 경과 -> Patrol로 전환
            if (_idleTimer >= _idleDuration)
            {
                _fsm.ChangeState(_citizen.PatrolState);
                return;
            }
        }
        else
        {
            if (_idleTimer >= _healDuration)
            {
                _citizen.TakeHeal();
                _idleTimer = 0f;                
            }
        }
    }
}
