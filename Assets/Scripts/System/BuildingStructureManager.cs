using System.Collections.Generic;
using UnityEngine;

public class BuildingStructureManager : MonoBehaviour
{
    public static BuildingStructureManager Instance { get; private set; }

    [Header("Setting")]

    private List<GuardBarrack> _guardBarracks = new();
    private List<House> _houses = new();

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

    // 집 등록 및 파괴
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

    public void OnHouseDestroyed(House house)
    {
        _houses.Remove(house);
    }


    // 배럭 등록 및 파괴
    public void RegisterBarrack(GuardBarrack barrack)
    {
        if (!_guardBarracks.Contains(barrack))
            _guardBarracks.Add(barrack);
    }

    public void UnregisterBarrack(GuardBarrack barrack)
    {
        if (!_guardBarracks.Contains(barrack))
            _guardBarracks.Remove(barrack);
    }

    public void OnGuardBarrackDestroyed(GuardBarrack barrack)
    {
        _guardBarracks.Remove(barrack);
    }

    public List<House> GetHouses() => _houses;
    public List<GuardBarrack> GetBarracks() => _guardBarracks;
}
