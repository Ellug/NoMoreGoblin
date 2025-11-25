using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    // MVC
    [Header("MVC")]
    [SerializeField] private PlayerModel _model;
    [SerializeField] private PlayerView _view;

    // Components
    private Rigidbody2D _rb;
    private Animator _anim;

    // Internal
    private Vector2 _moveInput;
    private bool _isFacingRight = true;

    // FSM에서 쓰는 런타임 상태 플래그
    public bool AttackPressed { get; set; }
    public bool CanAttack { get; set; } = true;
    public bool DashPressed { get; set; }
    public bool CanDash { get; set; }
    public bool IsDashing { get; set; }
    public bool BuildPressed { get; set; }

    // Model에서 값 읽어오는 래퍼 프로퍼티
    public float DashDistance => _model.DashDistance;
    public float DashCoolDown => _model.DashCoolDown;
    public float AttackCoolDown => _model.AttackCoolDown;
    public float AttackRange => _model.AttackRange;
    public float AttackDamage => _model.AttackDamage;
    public float AttackSpeed => _model.AttackSpeed;
    public Vector2 MoveInput => _moveInput;
    public bool IsFacingRight => _isFacingRight;

    public Rigidbody2D Rb => _rb;
    public Animator Anim => _anim;

    // States
    public PlayerMoveState MoveState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerBuildState BuildState { get; private set; }

    private PlayerFSM _fsm;

    public DamageableType Type => DamageableType.Player;
    private bool _isAlive;
    public bool IsAlive => _isAlive;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();

        // Model / View 참조 체크
        if (_model == null)
            Debug.LogError("PlayerController: Where is Your Model?");
        if (_view == null)
            Debug.LogWarning("PlayerController: Where is Your View?");

        // Model 이벤트 구독 -> View 연결
        if (_model != null)
        {
            _model.OnHpChanged  += ratio => _view?.UpdateHpBar(ratio);
            _model.OnExpChanged += ratio => _view?.UpdateExpBar(ratio);
            _model.OnDie += OnPlayerDie;
            _model.OnLevelUp += OnPlayerLevelUp;
        }

        // FSM 초기화
        _fsm = new PlayerFSM();
        MoveState = new PlayerMoveState(this, _fsm);
        AttackState = new PlayerAttackState(this, _fsm);
        DashState = new PlayerDashState(this, _fsm);
        BuildState = new PlayerBuildState(this, _fsm);

        _isAlive = true;
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
        AttackPressed = CanAttack;
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        DashPressed = CanDash && MoveInput != Vector2.zero;
    }

    public void OnBuildMode(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            BuildPressed = true;
    }

    // Move Logic
    public void Move()
    {
        bool isRunning = _moveInput.sqrMagnitude > 0.1f;
        _anim.SetBool("isRunning", isRunning);

        if (_moveInput == Vector2.zero) return;

        // 이동 속도는 Model에서 가져옴
        Vector2 next = _moveInput * _model.MoveSpeed * Time.fixedDeltaTime;
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

    // Attack / Dash 쿨다운 관련 – FSM에서 호출
    public void ResetAttack()
    {
        CanAttack = true;
    }

    public void EndDash()
    {
        IsDashing = false;
    }

    public void ResetDash()
    {
        CanDash = true;
    }

    // IDamageable 전용. 실 로직은 Model에
    public void TakeDamage(float dmg, GameObject attacker = null)
    {
        _model.TakeDamage(dmg);
    }

    public void AddExp(float exp)
    {
        _model.AddExp(exp);
    }

    // Model 이벤트 콜백
    private void OnPlayerDie()
    {
        Debug.Log("PlayerController: Player is Dead");
        _isAlive = false;
        _view.ShowDeath();

        // TODO: FSM 정지, GameOver UI 호출 등
    }

    private void OnPlayerLevelUp()
    {
        LevelupManager.Instance.RequestLevelup(_model);
    }
}
