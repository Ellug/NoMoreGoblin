using UnityEngine;

public class GuardBarrack : BaseBuilding
{
    [Header("Barrack Settings")]
    public int maxGuardCount = 4;
    public int currentGuardCount = 0;

    public void OnGuardSpawned() => currentGuardCount++;
    public void OnGuardReturned() => currentGuardCount--;

    protected override void Die()
    {
        // Barrack 파괴
        BuildingStructureManager.Instance.OnGuardBarrackDestroyed(this);

        base.Die();
    }
}
