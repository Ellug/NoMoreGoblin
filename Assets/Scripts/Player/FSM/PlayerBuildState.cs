using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBuildState : PlayerState
{
    public PlayerBuildState(PlayerController player, PlayerStateMachine fsm) : base(player, fsm) { }

    public override void Enter()
    {
        _player.BuildPressed = false;
        _player.Anim.SetBool("isRunning", false);

        // BuildManager를 통해 UI 오픈
        BuildManager.Instance.ToggleBuildMode();
    }

    public override void UpdateLogic()
    {
        // B 키 다시 누르면 나가기
        if (_player.BuildPressed)
        {
            _player.BuildPressed = false;
            BuildManager.Instance.ToggleBuildMode();
            _fsm.ChangeState(_player.MoveState);
            return;
        }

        // Build Mode가 해제되었으면 자동 복귀
        if (!BuildManager.Instance.IsBuildMode)
        {
            _fsm.ChangeState(_player.MoveState);
            return;
        }

        // 좌클릭 설치 시도
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0;

            BuildManager.Instance.TryPlaceBuilding(mousePos);
        }

        // 우클릭, ESC = 취소
        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            BuildManager.Instance.ToggleBuildMode();
            _fsm.ChangeState(_player.MoveState);
        }
    }

    public override void Exit()
    {
        _player.BuildPressed = false;
    }
}
