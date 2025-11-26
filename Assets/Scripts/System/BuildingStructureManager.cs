using System.Collections.Generic;
using UnityEngine;

public class BuildingStructureManager : MonoBehaviour
{
    public static BuildingStructureManager Instance { get; private set; }

    [Header("Setting")]

    private List<GuardBarrack> _guardBarracks = new List<GuardBarrack>();
    private List<House> _houses = new List<House>();

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
    public void OnGuardBarrackDestroyed(GuardBarrack barrack)
    {
        // 리스트에서 제거
        _guardBarracks.Remove(barrack);
    }

    public void OnHouseDestroyed(House house)
    {
        _houses.Remove(house);
    }

    public void RegisterHouse(House house)
    {
        if (!_houses.Contains(house))
            _houses.Add(house);
    }

    public void UnregisterHouse(House house)
    {
        if (!_houses.Contains(house))
            _houses.Remove(house);
    }

    public List<House> GetHouses()
    {
        return _houses;
    }
}
