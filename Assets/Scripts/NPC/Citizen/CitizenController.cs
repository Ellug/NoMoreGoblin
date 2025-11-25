using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class CitizenController : MonoBehaviour, IDamageable
{
    public DamageableType Type => DamageableType.Citizen;

    // MVC
    [Header("MVC")]
    [SerializeField] private CitizenModel _model;
    [SerializeField] private CitizenView _view;

    [Header("Runtime")]    
    public Transform target;
    public Vector3? targetPos;
    public Transform originBaseTrf;

    // Components
    private Rigidbody2D _rb;
    private Animator _anim;
    private Collider2D _collider;

    // Internal
    private House _originBase;
    private Vector2 _moveInput;
    private bool _isFacingRight = true;

    // Wrapper Properties
    public float MaxHp => _model.MaxHp;
    public float CurHp => _model.CurHp;
    public float DetectRange => _model.DetectRange;
    public float MoveSpeed => _model.MoveSpeed;
    public Vector2 MoveInput => _moveInput;
    public bool IsFacingRight => _isFacingRight;
    public bool IsAlive => _model.IsAlive;
    // 레이어 하드코딩
    public int layers = 1 << 8;
    public bool IsKidnapped { get; set; } = false;

    public Rigidbody2D Rb => _rb;
    public Animator Anim => _anim;

    // States
    public CitizenIdleState IdleState { get; private set; }
    public CitizenPatrolState PatrolState { get; private set; }
    public CitizenFleeState FleeState { get; private set; }
    public CitizenKidnapSate KidnapSate { get; private set; }

    private CitizenFSM _fsm;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();

        // Model / View 참조 체크
        if (_model == null)
            Debug.LogError("Citizen C: Where is Your Model?");
        if (_view == null)
            Debug.LogWarning("Citizen C: Where is Your View?");

        // Model 이벤트 구독 -> View 연결
        if (_model != null)
        {
            _model.OnDie += OnDie;
        }

        // FSM 초기화
        _fsm = new CitizenFSM();
        IdleState = new CitizenIdleState(this, _fsm);
        PatrolState = new CitizenPatrolState(this, _fsm);
        FleeState = new CitizenFleeState(this, _fsm);
        KidnapSate = new CitizenKidnapSate(this, _fsm);
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

    public void ResetCitizen()
    {
        _model.ResetCitizenModel();
        IsKidnapped = false;
        _rb.simulated = true;
        _collider.enabled = true;
        target = null;
        targetPos = null;
        originBaseTrf = null;
        _originBase = null;
        _model.SetIsAlive(true);

        _fsm.ChangeState(IdleState);
    }

    public void SetOriginBase(House originBase)
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

    // IDamageable -> 납치 발동
    public void TakeDamage(float dmg, GameObject attacker)
    {
        // _model.TakeDamage(dmg);
        // 납치
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
            _originBase.OnCitizenReturned();

        // 2초 후 풀링으로 리턴 for 시체 연출
        Invoke(nameof(Despawn), 2f);
    }

    private void Despawn()
    {
        CitizenPool.Instance.ReturnCitizen(this);
    }

    public void Kidnaped()
    {
        // 이미 납치된 상태면 무시
        if (IsKidnapped) return;

        IsKidnapped = true;
        _fsm.ChangeState(KidnapSate);
    }
}
