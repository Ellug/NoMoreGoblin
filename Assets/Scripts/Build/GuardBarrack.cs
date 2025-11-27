using System.Collections.Generic;
using UnityEngine;

public class GuardBarrack : BaseBuilding
{
    [SerializeField] private Transform _door;

    [Header("Barrack Settings")]
    public int maxGuardCount = 4;

    private List<GuardController> _assignedGuards = new(); // 등록된 가드들

    public Transform Door => _door;
    public int CurrentCitizenCount => _assignedGuards.Count;
    public bool CanAcceptMoreCitizens => _assignedGuards.Count < maxGuardCount;

    protected override void Awake()
    {
        BuildingStructureManager.Instance.RegisterBarrack(this);

        base.Awake();
    }

    // 가드 등록 / 해제
    public void RegisterGuard(GuardController guard)
    {
        if (!_assignedGuards.Contains(guard))
            _assignedGuards.Add(guard);
    }

    public void UnregisterGuard(GuardController guard)
    {
        if (_assignedGuards.Contains(guard))
            _assignedGuards.Remove(guard);
    }

    // 파괴시 로직
    protected override void Die()
    {
        // 반복 도중 리스트 수정 방지
        var copy = new List<GuardController>(_assignedGuards);

        // 소속 가드 전부에게 알림
        foreach (var c in copy)
        {
            if (c != null && c.IsAlive)
                c.OnOriginBarrackDestroyed();
        }

        BuildingStructureManager.Instance.UnregisterBarrack(this);
        base.Die();
    }
}
