using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinBase : MonoBehaviour, IDamageable
{    
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _curHp;
    public int maxGoblinCount = 50;
    public int currentGoblinCount = 0; 

    [Header("Patrol Settings")]
    [SerializeField] private float _minPatrolInterval = 30f;
    [SerializeField] private float _maxPatrolInterval = 90f;

    private float _patrolTimer = 0f;
    private float _nextPatrolTime = 0f;
    private bool _isAlive;
    public bool IsAlive => _isAlive;
    private List<Goblin> _assignedGoblins = new();
    private WaitForSeconds _waitForSeconds = new WaitForSeconds(10f);

    public DamageableType Type => DamageableType.Enemy;

    void Awake()
    {
        _curHp = _maxHp;
        _isAlive = true;
        SetNextPatrolTime();
    }

    void Start()
    {
        StartCoroutine(ClearTreesDelay());
    }

    private IEnumerator ClearTreesDelay()
    {
        yield return _waitForSeconds;
        ClearTreesAroundBase();
    }

    private void ClearTreesAroundBase()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 20f);

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            if (hit.TryGetComponent<IDamageable>(out var dmgable))
            {
                if (dmgable.Type == DamageableType.Tree)
                    dmgable.TakeDamage(500f, gameObject);
            }
        }
    }

    void Update()
    {
        if (!_isAlive) return;

        // 간헐적 공격
        _patrolTimer += Time.deltaTime;
        if (_patrolTimer >= _nextPatrolTime)
        {
            _patrolTimer = 0f;
            SetNextPatrolTime();
            SendPatrolGroup();

            // 고블린이 꽉 차있으면 총공격
            if (currentGoblinCount >= maxGoblinCount)
                RaidAttack();
        }
    }

     private void SetNextPatrolTime()
    {
        _nextPatrolTime = Random.Range(_minPatrolInterval, _maxPatrolInterval);
    }

    // Goblin 귀속 등록 / 해제
    public void RegisterGoblin(Goblin goblin)
    {
        if (!_assignedGoblins.Contains(goblin))
            _assignedGoblins.Add(goblin);

        currentGoblinCount = _assignedGoblins.Count;
    }

    public void UnregisterGoblin(Goblin goblin)
    {
        if (_assignedGoblins.Contains(goblin))
            _assignedGoblins.Remove(goblin);

        currentGoblinCount = _assignedGoblins.Count;
    }

    // 랜덤 소규모 패트롤 (House 기반)
    private void SendPatrolGroup()
    {
        if (_assignedGoblins.Count <= 0) return;

        // House 목록에서 랜덤하게 선택
        List<House> houses = BuildingStructureManager.Instance.GetHouses();
        if (houses == null || houses.Count == 0) return;

        House targetHouse = houses[Random.Range(0, houses.Count)];
        if (targetHouse == null) return;

        int count = Mathf.Min(Random.Range(2, 5), _assignedGoblins.Count);

        // 집 주변 10f 범위 내 랜덤 위치로 패트롤
        for (int i = 0; i < count; i++)
        {
            Goblin g = _assignedGoblins[Random.Range(0, _assignedGoblins.Count)];
            if (!g.IsAlive) continue;
            if (g.IsKidnapping) continue;

            if (SafePatrolPoint.TryGetSafePatrolPoint(targetHouse.transform.position, 10f, out Vector3 safePos))
            {
                g.target = null;
                g.targetPos = safePos;
                g.ChangeToPatrolState();
            }
        }
    }

    // 카운트 다 차면 총공격
    public void RaidAttack()
    {
        if (_assignedGoblins.Count == 0) return;

        List<House> houses = BuildingStructureManager.Instance.GetHouses();
        if (houses == null || houses.Count == 0) return;

        foreach (Goblin g in _assignedGoblins)
        {
            if (g == null || !g.IsAlive) continue;
            if (g.IsKidnapping) continue;

            House targetHouse = houses[Random.Range(0, houses.Count)];
            if (targetHouse == null) continue;

            if (SafePatrolPoint.TryGetSafePatrolPoint(targetHouse.transform.position, 10f, out Vector3 safePos))
            {
                g.target = null;
                g.targetPos = safePos;
                g.ChangeToPatrolState();
            }
        }
    }

    public void TakeDamage(float dmg, GameObject attacker = null)
    {
        _curHp -= dmg;

        if (_curHp <= 0)
            DestroyBase();
    }

    public void DestroyBase()
    {
        _isAlive = false;
        GoblinBaseManager.Instance.OnBaseDestroyed(this);
        Destroy(gameObject);
    }

    public List<Goblin> GetAssignedList() => _assignedGoblins;
}
