using UnityEngine;

public class CitizenKidnapSate : CitizenState
{
    private Goblin _kidnapper;

    public CitizenKidnapSate(CitizenController citizen, CitizenFSM fsm) : base(citizen, fsm) { }

    public void SetKidnapper(Goblin goblin)
    {
        _kidnapper = goblin;
    }

    public override void Enter()
    {
        // 시민 상태 변경
        _citizen.IsKidnapped = true;
        _citizen.Anim.SetBool("IsRunning", false);

        // 충돌 제거 (고블린 이동 방해 방지)
        _citizen.Rb.simulated = false;
        _citizen.GetComponent<Collider2D>().enabled = false;

        // target, targetPos 비활성화
        _citizen.target = null;
        _citizen.targetPos = null;
    }

    public override void UpdateLogic()
    {
        if (_kidnapper == null || !_kidnapper.IsAlive)
        {
            // 고블린 사망 시 시민은 Flee
            RestoreCitizen();
            return;
        }
    }

    public override void UpdatePhysics()
    {
        if (_kidnapper == null) return;

        // 고블린에게 붙어서 이동
        _citizen.transform.position = _kidnapper.transform.position + new Vector3(0.3f, 0, 0);
    }

    private void RestoreCitizen()
    {
        _citizen.IsKidnapped = false;
        _citizen.Rb.simulated = true;
        _citizen.GetComponent<Collider2D>().enabled = true;

        _fsm.ChangeState(_citizen.FleeState);
    }
}
