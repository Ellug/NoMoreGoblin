using System.Collections.Generic;
using UnityEngine;

public class GuardBarrackManager : MonoBehaviour
{
    public static GuardBarrackManager Instance { get; private set; }

    [Header("Setting")]

    private List<GuardBarrack> _guardBarracks = new List<GuardBarrack>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            return;
        }
    }

    // 베이스 파괴 시 호출
    public void OnGuardBarrackDestroyed(GuardBarrack destroyedBase)
    {
        // 리스트에서 제거
        _guardBarracks.Remove(destroyedBase);
    }
}
