using UnityEngine;

public class PlayerDashState : PlayerState
{
    private Vector2 _dir;
    private Vector2 _dashTarget;
    private bool _hasDashed = false;
    
    public PlayerDashState(PlayerController player, PlayerFSM fsm) : base(player, fsm) { }

    public override void Enter()
    {
        _player.CanDash = false;
        _player.IsDashing = true;
        _player.DashPressed = false;

        _player.Anim.SetTrigger("DashTrigger");

        PrepareToDash();
    }

    public override void Exit()
    {
        _hasDashed = false;
    }

    public override void UpdateLogic()
    {
        ////
        // 대시 종료 전에 어택시 대시어택 발동 명시 추가 필요 (현재는 애니메이션에서 트리거만 운좋게 작동)
        ////

        if (!_player.IsDashing)
            _fsm.ChangeState(_player.MoveState);
    }

    public override void UpdatePhysics()
    {
        if (_hasDashed) return;

        // Dash Active
        _player.Rb.MovePosition(_dashTarget);
        _hasDashed = true;

        // Dash End
        _player.Invoke(nameof(PlayerController.EndDash), 0.1f);
        _player.Invoke(nameof(PlayerController.ResetDash), _player.DashCoolDown);
    }    

    private void PrepareToDash()
    {
        // 방향 설정
        _dir = _player.MoveInput.normalized;

        // 충돌 검사
        RaycastHit2D hit = Physics2D.Raycast(_player.Rb.position, _dir, _player.DashDistance, LayerMask.GetMask("Building", "Tree"));

        // 장애물 처리
        if (hit.collider != null)
            _dashTarget = hit.point - (_dir * 0.1f);
        else
            _dashTarget = _player.Rb.position + _dir * _player.DashDistance;
    }
}
