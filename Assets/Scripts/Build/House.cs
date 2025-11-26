using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : BaseBuilding
{
    [SerializeField] private Transform _door;

    [Header("House Settings")]
    public int maxCitizenCount = 6;
    private float _safeCheckRadius = 60f;
    private float _requiredSafeTime = 60f;
    private float _safeTimer = 0f;
    private bool _isSafe = false;

    private Coroutine _safeCheckRoutine;
    private List<CitizenController> _assignedCitizens = new(); // 등록된 시민들
    private List<CitizenController> _insideCitizens = new(); // 집 안에 입장한 시민들

    public bool IsSafe => _isSafe;
    public Transform Door => _door;
    public int CurrentCitizenCount => _assignedCitizens.Count;
    public bool CanAcceptMoreCitizens => _assignedCitizens.Count < maxCitizenCount;

    protected override void Awake()
    {
        BuildingStructureManager.Instance.RegisterHouse(this);

        base.Awake();
    }

    // 시민 등록
    public void RegisterCitizen(CitizenController citizen)
    {
        if (!_assignedCitizens.Contains(citizen))
            _assignedCitizens.Add(citizen);
    }

    // 시민 등록 해제
    public void UnregisterCitizen(CitizenController citizen)
    {
        if (_assignedCitizens.Contains(citizen))
            _assignedCitizens.Remove(citizen);

        // 집 안에 시민이 0명 되면 감지 중단
        if (_insideCitizens.Count == 0)
            StopSafeLoopIfRunning();
    }

    // 시민 집 입장
    public void EnterHouse(CitizenController citizen)
    {
        if (!_insideCitizens.Contains(citizen))
            _insideCitizens.Add(citizen);

        // 내부 체크 루프 시작 (0 -> 1 된 순간)
        if (_insideCitizens.Count == 1)
            StartSafeLoopIfNeeded();
    }

    // 시민 집 퇴장
    public void ExitHouse(CitizenController citizen)
    {
        _insideCitizens.Remove(citizen);

        if (_insideCitizens.Count == 0)
            StopSafeLoopIfRunning();
    }

    // 파괴시 로직
    protected override void Die()
    {
        StopSafeLoopIfRunning();

        BuildingStructureManager.Instance.UnregisterHouse(this);

        // 소속 시민 및 내부 시민 전부에게 알림
        foreach (var c in _assignedCitizens)
        {
            if (c != null && c.IsAlive)
                c.OnOriginHouseDestroyed();
        }

        base.Die();
    }

    // Safe Check System
    private void StartSafeLoopIfNeeded()
    {
        if (_safeCheckRoutine == null)
            _safeCheckRoutine = StartCoroutine(SafeCheckLoop());
    }

    private void StopSafeLoopIfRunning()
    {
        if (_safeCheckRoutine != null)
        {
            StopCoroutine(_safeCheckRoutine);
            _safeCheckRoutine = null;
        }

        // 안전 판정 초기화
        _safeTimer = 0f;
        _isSafe = false;
    }

    // 범위내 적 체크
    private bool CheckEnemyAround()
    {
        int layer = 1 << 8;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _safeCheckRadius, layer);

        foreach (var h in hits)
        {
            if (h.TryGetComponent<Goblin>(out var goblin) && goblin.IsAlive)
                return true;
        }

        return false;
    }

    // 안전 체크 완료 출가
    private void OnHouseSafe()
    {
        Debug.Log($"House {name} : Safe for 60 sec → Citizens go out.");

        foreach (var c in _assignedCitizens)
        {
            if (c == null || !c.IsAlive) continue;

            c.transform.position = Door.position;
            c.target = null;
            c.targetPos = null;
            c.SetIdleState();
        }

        _insideCitizens.Clear();
        _safeTimer = 0f;
    }

    private IEnumerator SafeCheckLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(2f);

        while (true)
        {
            yield return wait;

            // 시민이 없다면 즉시 loop 종료
            if (_insideCitizens.Count == 0)
            {
                StopSafeLoopIfRunning();
                yield break;
            }

            bool hasEnemy = CheckEnemyAround();

            // 적이 있으면 타이머 0
            if (hasEnemy)
            {
                _safeTimer = 0f;
                _isSafe = false;
                continue;
            }

            // 타이머 진행
            _safeTimer += 2f;

            if (!_isSafe && _safeTimer >= _requiredSafeTime)
            {
                _isSafe = true;
                OnHouseSafe();
            }
        }
    }
}
