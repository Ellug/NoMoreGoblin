using UnityEngine;

public abstract class BaseBuilding : MonoBehaviour, IDamageable
{
    [Header("Base Stats")]
    [SerializeField] protected float _maxHp = 50f;
    [SerializeField] protected float _curHp;

    public DamageableType Type => DamageableType.Building;

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
        Destroy(gameObject);
    }
}