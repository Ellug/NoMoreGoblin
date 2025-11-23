using UnityEngine;

public class Building : MonoBehaviour, IDamageable
{
    [SerializeField] float _maxHp = 50;
    [SerializeField] float _curHp;

    public DamageableType Type => DamageableType.Building;

    public BuildingData data;

    public Vector2Int Size => data.size;
    public int WoodCost => data.woodCost;

    void Awake()
    {
        _curHp = _maxHp;
    }

    public void TakeDamage(float dmg, GameObject attacker = null)
    {
        _curHp -= dmg;

        if (_curHp <= 0)
            Die();
    }

    private void Die()
    {
        // 건물 파괴 처리
    }
}
