using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _curHp;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _dashDistance = 5f;
    [SerializeField] private float _dashCoolDown = 5f;
    [SerializeField] private float _attackDamage = 1f;
    [SerializeField] private float _attackCoolDown = 0.8f;
    [SerializeField] private float _attackRange = 2.5f;
    [SerializeField] private float _attackSpeed = 1f;
    [SerializeField] private float _maxExp = 50f;
    [SerializeField] private float _curExp = 0f;

    public DamageableType Type => DamageableType.Player;
    
    // Components
    private Rigidbody2D _rb;
    private Animator _anim;

    // Internal
    private Vector2 _moveInput;
    private bool _isFacingRight = true;

    // Properties
    public float DashDistance => _dashDistance;
    public float DashCoolDown => _dashCoolDown;
    public Vector2 MoveInput => _moveInput;
    public bool IsFacingRight => _isFacingRight;
    public bool AttackPressed { get; set; }
    public float AttackCoolDown => _attackCoolDown;
    public float AttackRange => _attackRange;
    public float AttackDamage => _attackDamage;
    public float AttackSpeed => _attackSpeed;
    public bool CanAttack { get; set; } = true;
    public bool DashPressed { get; set; }
    public bool CanDash { get; set; }
    public bool IsDashing { get; set; }
    public bool BuildPressed { get; set; }

    public Rigidbody2D Rb => _rb;
    public Animator Anim => _anim;

    // States
    public PlayerMoveState MoveState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerBuildState BuildState { get; private set; }

    private PlayerStateMachine _fsm;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();

        _fsm = new PlayerStateMachine();

        MoveState = new PlayerMoveState(this, _fsm);
        AttackState = new PlayerAttackState(this, _fsm);
        DashState = new PlayerDashState(this, _fsm);
        BuildState = new PlayerBuildState(this, _fsm);

        _curHp = _maxHp;
    }

    void Start()
    {
        CanDash = true;
        _fsm.Initialize(MoveState);
    }

    void Update()
    {
        _fsm.CurrentState.UpdateLogic();
    }

    void FixedUpdate()
    {
        _fsm.CurrentState.UpdatePhysics();
    }

    // Input
    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (CanAttack)
            AttackPressed = true;
        else
            AttackPressed = false;
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (CanDash && MoveInput != Vector2.zero)
            DashPressed = true;
        else
            DashPressed = false;
    }

    public void OnBuildMode(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            BuildPressed = true;
    }

    // Move Logics
    public void Move()
    {
        bool isRunning = _moveInput.sqrMagnitude > 0.1f;
        _anim.SetBool("isRunning", isRunning);

        if (_moveInput == Vector2.zero) return;

        Vector2 next = _moveInput * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + next);

        HandleFlip();
    }

    private void HandleFlip()
    {
        if (_moveInput.x > 0 && !_isFacingRight)
            Flip(true);
        else if (_moveInput.x < 0 && _isFacingRight)
            Flip(false);
    }

    private void Flip(bool facingRight)
    {
        if (_isFacingRight == facingRight) return;

        _isFacingRight = facingRight;

        // Flip 하기 전 위치 보존
        Vector3 pos = transform.position;

        // Flip
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        transform.localScale = scale;

        // 중앙 어긋남 조정을 위한 보정값 적용
        float offset = 1.2f;
        pos.x += facingRight ? offset : -offset;
        transform.position = pos;
    }

    // Attack CoolDown for Invoke
    public void ResetAttack()
    {
        CanAttack = true;
    }

    // Dash End & CoolDown for Invoke
    public void EndDash()
    {
        IsDashing = false;
    }

    public void ResetDash()
    {
        CanDash = true;
    }

    // Take Damage
    public void TakeDamage(float dmg, GameObject attacker = null)
    {
        if (_curHp > 0)
            _curHp -= dmg;

        if (_curHp <= 0)
            Die();
    }

    public void AddExp(float exp)
    {
        _curExp += exp;

        if(_curExp >= _maxExp)
        {
            // 레벨업 로직
            _curExp -= _maxExp;
            _maxExp += 5f;

            // 게임 일시정지 후 랜덤 스탯 선택 UI 출력
        }
    }

    private void Die()
    {
        Debug.Log("Player is Dead");
    }
}