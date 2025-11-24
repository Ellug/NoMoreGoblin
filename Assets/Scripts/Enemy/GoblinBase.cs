using UnityEngine;

public class GoblinBase : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _curHp;

    public DamageableType Type => DamageableType.Enemy;

    public float spawnRadius = 30f;
    public int maxGoblinCount = 50;
    public int currentGoblinCount = 0;
    public bool isDestroyed = false;

    void Awake()
    {
        _curHp = _maxHp;
    }

    public void OnGoblinSpawned()
    {
        currentGoblinCount++;
    }

    public void OnGoblinReturned()
    {
        currentGoblinCount--;
    }

    public void TakeDamage(float dmg, GameObject attacker = null)
    {
        _curHp -= dmg;

        if (_curHp <= 0)
            DestroyBase();
    }

    public void DestroyBase()
    {
        isDestroyed = true;

        // 매니져에 통보
        GoblinBaseManager.Instance.OnBaseDestroyed(this);

        Destroy(gameObject);
    }
}
