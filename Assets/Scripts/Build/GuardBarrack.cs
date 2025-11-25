using UnityEngine;

public class GuardBarrack : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHp = 50f;
    [SerializeField] private float _curHp;

    public DamageableType Type => DamageableType.Building;
    private bool _isAlive;
    public bool IsAlive => _isAlive;

    public float spawnRadius = 30f;
    public int maxGuardCount = 8;
    public int currentGuardCount = 0;

    void Awake()
    {
        _curHp = _maxHp;
        _isAlive = true;
    }

    public void OnGuardSpawned()
    {
        currentGuardCount++;
    }

    public void OnGuardReturned()
    {
        currentGuardCount--;
    }

    public void TakeDamage(float dmg, GameObject attacker = null)
    {
        _curHp -= dmg;

        if (_curHp <= 0)
            Die();
    }

    public void Die()
    {
        // 매니져에 통보
        GuardBarrackManager.Instance.OnGuardBarrackDestroyed(this);

        _isAlive = false;
        Destroy(gameObject);
    }
}
