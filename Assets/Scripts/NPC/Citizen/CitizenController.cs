using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private bool _isKidnaped = false;

    // Components
    private Rigidbody2D _rb;
    private Animator _anim;
    private Collider2D _collider;
    private NpcMoveController _movement;

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
    public bool IsKidnapped { get { return _isKidnaped; } set { _isKidnaped = value; } }
    public bool IsInsideHouse { get; private set; }


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

    public void ResetCitizen()
    {
        _model.ResetCitizenModel();
        IsKidnapped = false;
        _rb.simulated = true;
        _collider.enabled = true;
        target = null;
        targetPos = null;

        // 집 초기화
        if (_originBase != null)
            _originBase.UnregisterCitizen(this);
        originBaseTrf = null;
        _originBase = null;

        _model.SetIsAlive(true);

        _fsm.ChangeState(IdleState);
    }

    // 자기 집 설정
    public void SetOriginBase(House originBase)
    {
        if (_originBase != null)
            _originBase.UnregisterCitizen(this);

        _originBase = originBase;
        originBaseTrf = originBase.transform;

        _originBase.RegisterCitizen(this);
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
        if (Mathf.Abs(moveX) < 0.1f)
        return;

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

    // IDamageable -> 얘넨 안 맞고 납치. 납치 끝나면 사망용
    public void TakeDamage(float dmg, GameObject attacker = null)
    {
        _model.TakeDamage(dmg);
    }

    public void TakeHeal()
    {
        _model.TakeHeal();
    }

    // Model 이벤트 콜백
    private void OnDie()
    {
        // _anim.SetTrigger("DeathTrigger");
        _collider.enabled = false;
        _model.SetIsAlive(false);

        // 기지에 카운트 감소 처리
        if (_originBase != null)
            _originBase.UnregisterCitizen(this);

        ResourceManager.Instance.Add(ResourceType.NPC, -1);

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

    // 집 입장
    public void EnterHouse(House house)
    {
        if (IsInsideHouse) return;

        IsInsideHouse = true;
        house.EnterHouse(this);

        // 시민 숨기기, 물리 끄기
        _collider.enabled = false;
        _rb.simulated = false;

        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.enabled = false;

        target = null;
        targetPos = null;
        _fsm.ChangeState(IdleState);
    }

    // 집 퇴장
    public void ExitHouse()
    {
        if (!IsInsideHouse) return;

        IsInsideHouse = false;
        _originBase.ExitHouse(this);

        // 다시 보이게
        _collider.enabled = true;
        _rb.simulated = true;

        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.enabled = true;

        _fsm.ChangeState(PatrolState);
    }

    // 집 잃음
    public void OnOriginHouseDestroyed()
    {
        originBaseTrf = null;
        _originBase = null;

        // 집 잃은 경우 → 새 집 찾기 시도
        TryFindNewHouse();
    }

    public void TryFindNewHouse()
    {
        List<House> houses = BuildingStructureManager.Instance.GetHouses();

        House best = null;
        float bestDist = float.MaxValue;

        foreach (var h in houses)
        {
            if (h == null) continue;
            if (!h.CanAcceptMoreCitizens) continue;  // 빈 자리 없음

            float dist = Vector2.Distance(transform.position, h.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = h;
            }
        }

        if (best != null)
        {
            SetOriginBase(best);
            targetPos = best.Door.position;
            target = best.Door;
            _fsm.ChangeState(FleeState);
        }
        else
        {
            // 집이 없음 → Idle
            SetIdleState();
        }
    }
}
