using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GuardController : MonoBehaviour, IDamageable
{
    public DamageableType Type => DamageableType.Guard;

    // MVC
    [Header("MVC")]
    [SerializeField] private GuardModel _model;
    [SerializeField] private GuardView _view;

    [Header("Runtime")]    
    public Transform target;
    public Vector3? targetPos;
    public Transform originBaseTrf;

    // Components
    private Rigidbody2D _rb;
    private Animator _anim;
    private Collider2D _collider;
    private NpcMoveController _movement;

    // Internal
    private GuardBarrack _originBase;
    private Vector2 _moveInput;
    private bool _isFacingRight = true;

    // Runtime Flag for FSM
    public bool CanAttack { get; set; } = true;

    // Wrapper Properties
    public float MaxHp => _model.MaxHp;
    public float CurHp => _model.CurHp;
    public float DetectRange => _model.DetectRange;
    public float AttackCoolDown => _model.AttackCoolDown;
    public float AttackRange => _model.AttackRange;
    public float AttackDamage => _model.AttackDamage;
    public float AttackSpeed => _model.AttackSpeed;
    public float MoveSpeed => _model.MoveSpeed;
    public Vector2 MoveInput => _moveInput;
    public bool IsFacingRight => _isFacingRight;
    public bool IsAlive => _model.IsAlive;
    // 레이어 하드코딩
    public int layers = 1 << 8;

    public Rigidbody2D Rb => _rb;
    public Animator Anim => _anim;

    // States
    public GuardIdleState IdleState { get; private set; }
    public GuardPatrolState PatrolState { get; private set; }
    public GuardChaseState ChaseState { get; private set; }
    public GuardAttackState AttackState { get; private set; }

    private GuardFSM _fsm;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();

        // Model / View 참조 체크
        if (_model == null)
            Debug.LogError("Guard C: Where is Your Model?");
        if (_view == null)
            Debug.LogWarning("Guard C: Where is Your View?");

        // Model 이벤트 구독 -> View 연결
        if (_model != null)
        {
            _model.OnHpChanged  += ratio => _view?.UpdateHpBar(ratio);
            _model.OnDie += OnDie;
        }

        // FSM 초기화
        _fsm = new GuardFSM();
        IdleState = new GuardIdleState(this, _fsm);
        PatrolState = new GuardPatrolState(this, _fsm);
        ChaseState = new GuardChaseState(this, _fsm);
        AttackState = new GuardAttackState(this, _fsm);
        
        _movement = new NpcMoveController(_rb);
    }

    void Start()
    {
        _fsm.Initialize(IdleState);
    }

    void Update()
    {
        if(!IsAlive) return;
        _fsm.CurrentState.UpdateLogic();
    }

    void FixedUpdate()
    {
        if(!IsAlive) return;
        _fsm.CurrentState.UpdatePhysics();
    }

    public void ResetGuard()
    {
        _model.ResetGuardModel();
        _collider.enabled = true;
        target = null;
        targetPos = null;
        originBaseTrf = null;
        _originBase = null;
        _model.SetIsAlive(true);

        _fsm.ChangeState(IdleState);
    }

    public void SetOriginBase(GuardBarrack originBase)
    {
        _originBase = originBase;
        originBaseTrf = originBase.transform;

        _originBase.RegisterGuard(this);
    }

    // Move
    public void Move()
    {
        if (CurHp <= 0) return;

        Vector3? dest = targetPos ?? (target != null ? target.position : null);
        if (!dest.HasValue) return;

        _movement.MoveTo(dest.Value, MoveSpeed);

        HandleFlip(dest.Value.x - transform.position.x);
    }

    private void HandleFlip(float moveX)
    {
        if (moveX > 0 && !_isFacingRight)
            Flip(true);
        else if (moveX < 0 && _isFacingRight)
            Flip(false);
    }

    private void Flip(bool facingRight)
    {
        if (_isFacingRight == facingRight) return;

        _isFacingRight = facingRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        transform.localScale = scale;
    }

    // 타겟 감지
    public Transform DetectTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, DetectRange, layers);

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var h in hits)
        {
            if (!h.gameObject.activeInHierarchy) continue;
            if (!h.TryGetComponent<IDamageable>(out var t) || !t.IsAlive) continue;

            // 태그 필터
            if (!h.CompareTag("Enemy")) continue;

            float dist = Vector2.Distance(transform.position, h.transform.position);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = h.transform;
            }
        }

        return nearest;
    }

    // Idle State로 전환시 초기화 해야할 것들
    public void SetIdleState()
    {
        target = null;
        targetPos = null;
        _fsm.ChangeState(IdleState);
    }

    // Attack 쿨다운
    public void ResetAttack()
    {
        CanAttack = true;
    }

    // IDamageable 전용. 실 로직은 Model에
    public void TakeDamage(float dmg, GameObject attacker)
    {
        _anim.SetTrigger("HitTrigger");
        _model.TakeDamage(dmg);

        if (attacker != null &&
            attacker.activeInHierarchy &&
            attacker.TryGetComponent<IDamageable>(out var dmgable) && dmgable.IsAlive)
        {
            target = attacker.transform;
            targetPos = null;
            _fsm.ChangeState(ChaseState);
        }
    }

    public void TakeHeal()
    {
        _model.TakeHeal();
    }

    // Model 이벤트 콜백
    private void OnDie()
    {
        _anim.SetTrigger("DeathTrigger");
        _collider.enabled = false;
        _model.SetIsAlive(false);

        // 기지에 카운트 감소 처리
        if (_originBase != null)
            _originBase.UnregisterGuard(this);

        // 리소스 매니져에서 카운트 감소 처리
        ResourceManager.Instance.Add(ResourceType.Guard, -1);

        // 2초 후 풀링으로 리턴 for 시체 연출
        Invoke(nameof(Despawn), 2f);
    }

    private void Despawn()
    {
        GuardPool.Instance.ReturnGuard(this);
    }

    // 배럭 잃음
    public void OnOriginBarrackDestroyed()
    {
        OnDie();
    }
}
