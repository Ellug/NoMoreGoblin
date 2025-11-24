using System;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _dashDistance = 5f;
    [SerializeField] private float _dashCoolDown = 5f;
    [SerializeField] private float _attackDamage = 1f;
    [SerializeField] private float _attackCoolDown = 0.8f;
    [SerializeField] private float _attackRange = 2.5f;
    [SerializeField] private float _attackSpeed = 1f;
    [SerializeField] private float _maxExp = 50f;

    [Header("Runtime")]
    [SerializeField] private float _curHp;
    [SerializeField] private float _curExp;

    // Properties
    public float MaxHp => _maxHp;
    public float CurHp => _curHp;
    public float MoveSpeed => _moveSpeed;
    public float DashDistance => _dashDistance;
    public float DashCoolDown => _dashCoolDown;
    public float AttackDamage => _attackDamage;
    public float AttackCoolDown => _attackCoolDown;
    public float AttackRange => _attackRange;
    public float AttackSpeed => _attackSpeed;
    public float MaxExp => _maxExp;
    public float CurExp => _curExp;

    // 이벤트 – View / Controller에서 구독
    public event Action<float> OnHpChanged;
    public event Action<float> OnExpChanged;
    public event Action OnLevelUp;
    public event Action OnDie;

    void Awake()
    {
        _curHp  = _maxHp;
        _curExp = 0f;

        OnHpChanged?.Invoke(_curHp / _maxHp);
        OnExpChanged?.Invoke(_curExp / _maxExp);
    }

    public void TakeDamage(float dmg)
    {
        if (_curHp <= 0f) return;

        _curHp = Mathf.Max(0f, _curHp - dmg);
        OnHpChanged?.Invoke(_curHp / _maxHp);

        if (_curHp <= 0f)
            OnDie?.Invoke();
    }

    public void AddExp(float amount)
    {
        _curExp += amount;

        if (_curExp >= _maxExp)
        {
            _curExp -= _maxExp;
            _maxExp += 5f;
            OnLevelUp?.Invoke();
        }

        OnExpChanged?.Invoke(_curExp / _maxExp);
    }
}
