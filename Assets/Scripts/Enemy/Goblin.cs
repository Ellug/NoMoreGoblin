using UnityEngine;

public class Goblin : MonoBehaviour
{
    [SerializeField] private float _maxHp = 10f;
    [SerializeField] private float _dmg = 1f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _exp = 3f;
    private float _curHp;
    private GoblinBase _originBase;
    private Collider2D _collider;
    private Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
        _curHp = _maxHp;
    }

    public void ResetGoblin()
    {
        _curHp = _maxHp;
        // AI BT 정보 여기에서 초기화
    }

    public void SetOriginBase(GoblinBase originBase)
    {
        _originBase = originBase;
    }

    public void TakeDamage(float dmg, PlayerController attacker)
    {
        if (_curHp <= 0) return;

        _curHp -= dmg;

        if (_curHp <= 0)
            Die(attacker);
    }

    private void Die(PlayerController attacker)
    {
        _anim.SetTrigger("DeadTrigger");
        _collider.enabled = false;

        // 경험치 get
        attacker.AddExp(_exp);

        // 기지에 카운트 감소 처리
        if (_originBase != null)
            _originBase.OnGoblinReturned();

        // 2초 후 풀링으로 리턴
        Invoke(nameof(Despawn), 2f);
    }

    private void Despawn()
    {
        GoblinPool.Instance.ReturnGoblin(this);
    }
}
