using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBuildState : PlayerState
{
    public PlayerBuildState(PlayerController player, PlayerStateMachine fsm) : base(player, fsm) { }

    public override void Enter()
    {
        // 입력 소모
        _player.BuildPressed = false;

        // 이동/공격 정지
        _player.Anim.SetBool("isRunning", false);

        // BuildManager를 통해 UI 오픈
        BuildManager.Instance.ToggleBuildMode();

        Debug.Log("Build Mode Enter");
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

        // 마우스 클릭 감지하여 설치 시도
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0;

            BuildManager.Instance.TryPlaceBuilding(mousePos);
        }

        // 우클릭 → 취소하고 이동 복귀
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            BuildManager.Instance.ToggleBuildMode();
            _fsm.ChangeState(_player.MoveState);
        }
    }

    public override void Exit()
    {
        _player.BuildPressed = false;
        Debug.Log("Build Mode Exit");
    }
}
