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
            Debug.LogError("PlayerController: Where is Your Model?");
        if (_view == null)
            Debug.LogWarning("PlayerController: Where is Your View?");

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
    }

    // Move
    public void Move()
    {
        if (CurHp <= 0) return;

        Vector3? destination = null;

        // targetPos 우선
        if (targetPos.HasValue)
            destination = targetPos.Value;

        // target 없으면 Transform 우선
        if (target != null)
            destination = target.position;

        if (!destination.HasValue) return;

        // Vector3 dir = destination.Value - transform.position;
        // dir.Normalize();

        // 회피 기동!
        Vector2 desired = ((Vector2)(destination.Value - transform.position)).normalized;
        Vector2 avoid = ObstacleAvoidance.GetAvoidDirection(transform, desired, 0.5f, 1f, 0.3f);

        // 최종 이동 방향
        Vector2 finalDir = avoid.normalized;

        // 이동
        _rb.MovePosition(_rb.position + MoveSpeed * Time.fixedDeltaTime * finalDir);

        HandleFlip(finalDir.x);
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

        // Flip 하기 전 위치 보존
        Vector3 pos = transform.position;

        // Flip
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        transform.localScale = scale;

        // 중앙 어긋남 조정을 위한 보정값 적용
        float offset = 0.1f;
        pos.x += facingRight ? offset : -offset;
        transform.position = pos;
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

    // Attack / Dash 쿨다운 관련 – FSM에서 호출
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
            _originBase.OnGuardReturned();

        // 2초 후 풀링으로 리턴 for 시체 연출
        Invoke(nameof(Despawn), 2f);
    }

    private void Despawn()
    {
        GuardPool.Instance.ReturnGuard(this);
    }
}
