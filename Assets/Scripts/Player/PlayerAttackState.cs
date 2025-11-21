using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private float _timer = 0f;
    private const float BaseAttackDuration = 0.75f;
    private float _attackCoolDown;
    private float _attackDuration;

    public PlayerAttackState(PlayerController player, PlayerStateMachine fsm) : base(player, fsm) { }

    public override void Enter()
    {
        _timer = 0f;

        // 비례 계산
        _attackDuration = BaseAttackDuration / _player.AttackSpeed;
        _attackCoolDown = _player.AttackCoolDown / (_player.AttackSpeed * _player.AttackSpeed);

        _player.CanAttack = false;
        _player.AttackPressed = false;

        // 애니메이션 속도 조정 및 처리
        _player.Anim.SetFloat("AttackSpeed", _player.AttackSpeed);
        _player.Anim.SetTrigger("Attack1Trigger");
    }
    
    public override void Exit()
    {
        _player.AttackPressed = false;
    }

    public override void UpdateLogic()
    {
        _timer += Time.deltaTime;

        // 공격 모션 끝나면 MoveState 복귀
        if (_timer >= _attackDuration)
        {
            _player.Invoke(nameof(_player.ResetAttack), _attackCoolDown);
            _fsm.ChangeState(_player.MoveState);
        }
    }
}
