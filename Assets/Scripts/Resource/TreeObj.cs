using UnityEngine;

public class TreeObj : MonoBehaviour
{
    [SerializeField] private float _maxHp = 10f;
    [SerializeField] private float _exp = 1f;
    private float _curHp;

    void Awake()
    {
        _curHp = _maxHp;
    }

    public void TakeDamage(float dmg, PlayerController attacker)
    {
        _curHp -= dmg;
        if (_curHp <= 0)
            Die(attacker);
    }

    private void Die(PlayerController attacker)
    {
        attacker.AddExp(_exp);
        // 목재 획득 추가 필요
        Destroy(gameObject);
    }
}
