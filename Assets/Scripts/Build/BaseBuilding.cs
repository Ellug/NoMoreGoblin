using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBuilding : MonoBehaviour, IDamageable
{
    [Header("Base Stats")]
    [SerializeField] protected float _maxHp = 50f;
    [SerializeField] protected float _curHp;
    
    public DamageableType Type => DamageableType.Building;

        // 타일맵 정보 저장
    protected List<Vector3Int> _occupiedCells = new();
    protected bool _isAlive = true;
    public bool IsAlive => _isAlive;

    [Header("Data (ScriptableObject)")]
    public BuildingData data;

    protected virtual void Awake()
    {
        _curHp = _maxHp;
        _isAlive = true;
    }

    public virtual void TakeDamage(float dmg, GameObject attacker)
    {
        if (!_isAlive) return;

        _curHp -= dmg;

        if (_curHp <= 0)
            Die();
    }

    protected virtual void Die()
    {
        _isAlive = false;
        BuildManager.Instance.RemoveCollision(_occupiedCells);
        Destroy(gameObject);
    }

    public void SetOccupiedCells(List<Vector3Int> cells)
    {
        _occupiedCells = cells;
    }
}