using UnityEngine;

public class House : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHp = 50f;
    [SerializeField] private float _curHp;
    [SerializeField] private Transform _door;

    public DamageableType Type => DamageableType.Building;
    private bool _isAlive;
    public bool IsAlive => _isAlive;

    public float spawnRadius = 30f;
    public int maxCitizenCount = 8;
    public int currentCitizenCount = 0;
    public Transform Door => _door;

    void Awake()
    {
        _curHp = _maxHp;
        _isAlive = true;
    }

    public void OnCitizenSpawned()
    {
        currentCitizenCount++;
    }

    public void OnCitizenReturned()
    {
        currentCitizenCount--;
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
        // CitizenBarrackManager.Instance.OnCitizenBarrackDestroyed(this);

        _isAlive = false;
        Destroy(gameObject);
    }
}
