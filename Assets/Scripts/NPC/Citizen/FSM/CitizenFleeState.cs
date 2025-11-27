using UnityEngine;

public class CitizenFleeState : CitizenState
{
    public CitizenFleeState(CitizenController citizen, CitizenFSM fsm) : base(citizen, fsm) { }

    public override void Enter()
    {
        // flee 목적지는 originBaseTrf의 _door 로 설정
        _citizen.target = null;

        // 도망 목적지 설정
        if (_citizen.originBaseTrf != null)
        {
            House house = _citizen.originBaseTrf.GetComponent<House>();
            if (house != null && house.Door != null)
                _citizen.targetPos = house.Door.position;
        }

        _citizen.Anim.SetBool("IsRunning", true);
    }

    public override void UpdateLogic()
    {
        // 도망 목적지가 삭제되면 Idle로 돌아감
        if (!_citizen.targetPos.HasValue)
        {
            _citizen.SetIdleState();
            return;
        }
        
        // 집 없으면 Idle
        if (_citizen.originBaseTrf == null)
        {
            _citizen.SetIdleState();
            return;
        }

        // 도착 체크
        float dist = Vector2.Distance(_citizen.transform.position, _citizen.targetPos.Value);
        if (dist < 4f)
        {
            // 시민 입장 처리
            House house = _citizen.originBaseTrf.GetComponent<House>();
            _citizen.EnterHouse(house);
            return;
        }
    }

    public override void UpdatePhysics()
    {
        _citizen.Move();
    }
}
