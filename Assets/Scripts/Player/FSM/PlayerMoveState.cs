using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerFSM fsm) : base(player, fsm) { }

    public override void UpdateLogic()
    {
        Vector2 move = _player.MoveInput;

        // 이동 애니메이션
        _player.Anim.SetBool("isRunning", move.sqrMagnitude > 0.1f);

        // 공격 상태
        if (_player.AttackPressed && _player.CanAttack)
        {
            _fsm.ChangeState(_player.AttackState);
            return;
        }

        // 대시 상태
        if (_player.DashPressed && _player.CanDash)
        {
            if (_player.MoveInput == Vector2.zero) return;

            _fsm.ChangeState(_player.DashState);
            return;
        }

        // 빌드 상태
        if (_player.BuildPressed)
        {
            _fsm.ChangeState(_player.BuildState);
            return;
        }
    }

    public override void UpdatePhysics()
    {
        if (_player.IsDashing) return;
        _player.Move();
    }
}
