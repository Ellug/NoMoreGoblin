using UnityEngine;

public class Goblin : MonoBehaviour
{
    [SerializeField] private float _maxHp = 10;
    private float _curHp;

    void Awake()
    {
        _curHp = _maxHp;
    }

    public void TakeDamage(float dmg)
    {
        _curHp -= dmg;
        if (_curHp <= 0)
            Die();
    }

    private void Die()
    {
        // Goblin 사망 처리
        // 오브젝트 풀링, exp 제공
    }
}
