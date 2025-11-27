using UnityEngine;

public class Goblin : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float _maxHp = 10f;
    [SerializeField] private float _curHp;
    [SerializeField] private float _dmg = 1f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _detectRange = 20f;
    [SerializeField] private float _attackRange = 2.5f;
    [SerializeField] private float _attackCooldown = 1f;
    [SerializeField] private float _exp = 3f;
    [SerializeField] private bool _isKidnap = false;

    public DamageableType Type => DamageableType.Enemy;

    [Header("Runtime")]
    public Transform target;
    public Vector3? targetPos;
    public Transform originBaseTrf;
    private bool _isAlive = false;

    // Internal
    private GoblinBase _originBase;
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private Animator _anim;
    private NpcMoveController _movement;
    private bool _isFacingRight = true;

    // Properties
    public float MoveSpeed => _moveSpeed;
    public float DetectRange => _detectRange;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public float Dmg => _dmg;
    public Rigidbody2D Rb => _rb;
    public Animator Anim => _anim;
    public bool IsAlive => _isAlive;
    // 일단 레이어 하드코딩
    public int layers = (1 << 9) | (1 << 6) | (1 << 10);
    public bool IsKidnapping { get { return _isKidnap; } set { _isKidnap = value; } }

    // FSM
    private GoblinFSM _fsm;

    public GoblinIdleState IdleState { get; private set; }
    public GoblinPatrolState PatrolState { get; private set; }
    public GoblinChaseState ChaseState { get; private set; }
    public GoblinAttackState AttackState { get; private set; }
    public GoblinKidnapState KidnapState { get; private set; }

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
        _curHp = _maxHp;

        _fsm = new GoblinFSM();

        IdleState = new GoblinIdleState(this, _fsm);
        PatrolState = new GoblinPatrolState(this, _fsm);
        ChaseState = new GoblinChaseState(this, _fsm);
        AttackState = new GoblinAttackState(this, _fsm);
        KidnapState = new GoblinKidnapState(this, _fsm);

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

    public void ResetGoblin()
    {
        _curHp = _maxHp;
        _collider.enabled = true;
        target = null;
        targetPos = null;
        originBaseTrf = null;
        _originBase = null;
        _isAlive = true;
        IsKidnapping = false;

        _fsm.ChangeState(IdleState);
    }

    public void SetOriginBase(GoblinBase originBase)
    {
        _originBase = originBase;
        originBaseTrf = originBase.transform;

        _originBase.RegisterGoblin(this);
    }
    
    // Move
    public void Move()
    {
        if (_curHp <= 0) return;

        Vector3 dest;

        // 타겟 Transform 추적 (Chase/Attack)
        if (target != null)
        {
            // 기본 pivot 방향
            Vector3 rawTargetPos = target.position;

            // Collider 기반 보정
            if (target.TryGetComponent<Collider2D>(out var col))
            {
                // Collider 외곽의 고블린과 가장 가까운 점을 목표로 사용
                Vector2 closestPoint = col.ClosestPoint(transform.position);
                dest = closestPoint;
            }
            else
            {
                dest = rawTargetPos;
            }
        }
        // Patrol / Kidnap 시 targetPos 사용
        else if (targetPos.HasValue)
        {
            dest = targetPos.Value;
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        // 실제 이동
        _movement.MoveTo(dest, _moveSpeed);

        HandleFlip(dest.x - transform.position.x);
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

    public void TakeDamage(float dmg, GameObject attacker)
    {
        if (_curHp <= 0) return;

        if (attacker != null)
            target = attacker.transform;

        _curHp -= dmg;
        targetPos = null;

        if (_curHp <= 0)
            Die(attacker);
    }

    private void Die(GameObject attacker)
    {
        _anim.SetTrigger("DeadTrigger");
        _collider.enabled = false;
        _isAlive = false;

        // 경험치 get - Player가 죽였을 때만
        if (attacker.TryGetComponent<PlayerController>(out var player))
            player.AddExp(_exp);

        // 기지에 카운트 감소 처리
        if (_originBase != null)
            _originBase.UnregisterGoblin(this);

        // 2초 후 풀링으로 리턴 for 시체 연출
        Invoke(nameof(Despawn), 2f);
    }

    public void RemoveFromBase()
    {
        if (_originBase != null)
            _originBase.UnregisterGoblin(this);

        _originBase = null;
        originBaseTrf = null;
    }

    // 베이스 파괴시 호출될 재귀속
    public void AssignToNewBase(GoblinBase newBase)
    {
        RemoveFromBase();
        SetOriginBase(newBase);
    }

    // 베이스 파괴 시 호출될 FSM 전환
    public void ChangeToPatrolState()
    {
        _fsm.ChangeState(PatrolState);
    }

    private void Despawn()
    {
        GoblinPool.Instance.ReturnGoblin(this);
    }

    // 타겟 감지 (가장 가까운 적)
    public Transform DetectTarget()
    {
        if (IsKidnapping) return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _detectRange, layers);

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var h in hits)
        {
            if (!h.gameObject.activeInHierarchy) continue;
            if (!h.TryGetComponent<IDamageable>(out var t) || !t.IsAlive) continue;
            if (h.TryGetComponent<CitizenController>(out var citizen))
            {
                if (citizen.IsInsideHouse) continue;
                if (citizen.IsKidnapped) continue;
            }

            // 태그 필터
            if (!(h.CompareTag("Player") ||
                h.CompareTag("Guard") ||
                h.CompareTag("Citizen") ||
                h.CompareTag("Building")))
                continue;

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
}
