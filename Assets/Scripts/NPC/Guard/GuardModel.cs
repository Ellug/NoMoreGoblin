using System;
using UnityEngine;

public class GuardModel : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _attackDamage = 1f;
    [SerializeField] private float _attackCoolDown = 0.8f;
    [SerializeField] private float _attackRange = 2.5f;
    [SerializeField] private float _attackSpeed = 1f;
    [SerializeField] private float _detectRange = 1f;

    [Header("Runtime")]
    [SerializeField] private float _curHp;
    [SerializeField] private bool _isAlive = false;

    // Properties
    public float MaxHp => _maxHp;
    public float CurHp => _curHp;
    public float MoveSpeed => _moveSpeed;
    public float AttackDamage => _attackDamage;
    public float AttackCoolDown => _attackCoolDown;
    public float AttackRange => _attackRange;
    public float AttackSpeed => _attackSpeed;
    public float DetectRange => _detectRange;
    public bool IsAlive => _isAlive;

    // 이벤트 – View / Controller에서 구독
    public event Action<float> OnHpChanged;
    public event Action OnDie;

    void Awake()
    {
        _curHp  = _maxHp;

        OnHpChanged?.Invoke(_curHp / _maxHp);
    }

    public void TakeDamage(float dmg)
    {
        if (_curHp <= 0f) return;

        _curHp = Mathf.Max(0f, _curHp - dmg);
        OnHpChanged?.Invoke(_curHp / _maxHp);

        if (_curHp <= 0f)
            OnDie?.Invoke();
    }

    public void ResetGuardModel()
    {
        _curHp = _maxHp;
        _isAlive = true;
    }

    public void SetIsAlive(bool alive)
    {
        _isAlive = alive;
    }

    public void TakeHeal()
    {
        if (_curHp <= _maxHp)
            _curHp++;

        if (_curHp > _maxHp)
            _curHp = _maxHp;
    }
}
