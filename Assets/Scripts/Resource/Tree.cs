using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField] private float _maxHp = 10f;
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
        Debug.Log("Tree destroyed. Player gets wood +1");
        Destroy(gameObject);
    }
}
