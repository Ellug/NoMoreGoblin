using UnityEngine;

public class Building : MonoBehaviour, IDamageable
{
    [SerializeField] float _maxHp = 50;
    [SerializeField] float _curHp;

    public DamageableType Type => DamageableType.Building;
    private bool _isAlive;
    public bool IsAlive => _isAlive;

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
        // 건물 전체 관리 매니져 작성 후 통보 필요. 배럭도 Building이랑 같이 관리?

        _isAlive = false;
        Destroy(gameObject);
    }
}
