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

    public DamageableType Type => DamageableType.Enemy;

    [Header("Runtime")]
    public Transform target;
    public Vector3? targetPos;
    public Transform originBaseTrf;

    // Internal
    private GoblinBase _originBase;
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private Animator _anim;
    private bool _isFacingRight = true;

    // Properties
    public float MoveSpeed => _moveSpeed;
    public float DetectRange => _detectRange;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public float Dmg => _dmg;
    public Rigidbody2D Rb => _rb;
    public Animator Anim => _anim;

    // FSM
    private GoblinFSM _fsm;

    public GoblinIdleState IdleState { get; private set; }
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
        ChaseState = new GoblinChaseState(this, _fsm);
        AttackState = new GoblinAttackState(this, _fsm);
        KidnapState = new GoblinKidnapState(this, _fsm);
    }

    void Start()
    {
        _fsm.Initialize(IdleState);
    }

    void Update()
    {
        _fsm.CurrentState.UpdateLogic();
    }

    void FixedUpdate()
    {
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

        _fsm.ChangeState(IdleState);
    }

    public void SetOriginBase(GoblinBase originBase)
    {
        _originBase = originBase;
        originBaseTrf = originBase.transform;

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

        // 경험치 get - Player가 죽였을 때만
        if (attacker.TryGetComponent<PlayerController>(out var player))
            player.AddExp(_exp);

        // 기지에 카운트 감소 처리
        if (_originBase != null)
            _originBase.OnGoblinReturned();

        // 2초 후 풀링으로 리턴 for 시체 연출
        Invoke(nameof(Despawn), 2f);
    }

    private void Despawn()
    {
        GoblinPool.Instance.ReturnGoblin(this);
    }

    // Move
    public void Move()
    {
        if (_curHp <= 0) return;

        Vector3? destination = null;

        // targetPos 우선
        if (targetPos.HasValue)
            destination = targetPos.Value;

        // target 없으면 Transform 우선
        if (target != null)
            destination = target.position;

        if (!destination.HasValue)
        {
            _anim.SetBool("IsRunning", false);
            return;
        }

        Vector3 dir = destination.Value - transform.position;
        float dist = dir.sqrMagnitude;

        // 타겟이 매우 가까우면 멈춤
        if (dist < 0.1f)
        {
            _anim.SetBool("IsRunning", false);
            return;
        }

        // 이동
        dir.Normalize();
        Vector2 dir2 = new Vector2(dir.x, dir.y);
        _rb.MovePosition(_rb.position + dir2 * _moveSpeed * Time.fixedDeltaTime);


        _anim.SetBool("IsRunning", true);
        HandleFlip(dir.x);
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _detectRange);

        foreach (var h in hits)
        {
            if (h.CompareTag("Player") ||
                h.CompareTag("Guard") ||
                h.CompareTag("Citizen"))
            {
                return h.transform;
            }
        }
        return null;
    }
}
