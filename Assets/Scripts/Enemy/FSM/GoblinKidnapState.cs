using System;
using UnityEngine;

public class GoblinKidnapState : GoblinState
{
    private CitizenController _kidnappedCitizen;

    public GoblinKidnapState(Goblin goblin, GoblinFSM fsm) : base(goblin, fsm) { }

    public void SetCitizen(CitizenController citizen)
    {
        _kidnappedCitizen = citizen;
    }

    public override void Enter()
    {
        if (_kidnappedCitizen == null)
        {
            _fsm.ChangeState(_goblin.IdleState);
            return;
        }
        _goblin.IsKidnapping = true;
        _goblin.targetPos = null;

        // 시민 납치 상태 전환
        if (_kidnappedCitizen != null)
        {
            _kidnappedCitizen.Kidnaped();
            _kidnappedCitizen.KidnapSate.SetKidnapper(_goblin);
        }

        _goblin.Anim.SetBool("IsRunning", true);
    }

    public override void UpdateLogic()
    {
        if (_kidnappedCitizen == null || !_kidnappedCitizen.IsAlive)
        {
            EndKidnap();
            return;
        }

        // Goblin Base 도착 체크
        if (_goblin.originBaseTrf != null)
        {
            float dist = Vector2.Distance(_goblin.transform.position, _goblin.originBaseTrf.position);

            if (dist < 10f)
            {
                DeliverCitizen();
                EndKidnap();
                return;
            }
        }
    }

    public override void UpdatePhysics()
    {
        // base로 이동
        if (_goblin.originBaseTrf != null)
        {
            _goblin.target = _goblin.originBaseTrf;
            _goblin.Move();
        }
    }

    private void DeliverCitizen()
    {
        // GoblinBase에 시민 귀속?
        if (_kidnappedCitizen != null)
        {
            _kidnappedCitizen.IsKidnapped = false;
            _kidnappedCitizen.TakeDamage(100);
            // 후속 처리?            
        }
    }

    private void EndKidnap()
    {
        _goblin.IsKidnapping = false;
        _goblin.target = null;
        _fsm.ChangeState(_goblin.IdleState);
    }
}
