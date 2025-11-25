using UnityEngine;

public class GuardSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int _spawnCount = 1;
    [SerializeField] private float _interval = 15f;
    [SerializeField] private float _spawnRadius = 30f;
    
    private float _timer;
    private GuardBarrack _guardBarrack;

    void Awake()
    {
        _guardBarrack = GetComponent<GuardBarrack>();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _interval)
        {
            _timer = 0f;
            SpawnGuard();
        }
    }

    private void SpawnGuard()
    {
        for (int i = 0; i < _spawnCount; i++)
            SpawnOne();
    }

    private void SpawnOne()
    {
        // 랜덤 방향, 반경
        Vector2 dir = Random.insideUnitCircle.normalized;
        float dist = Random.Range(0f, _spawnRadius);

        Vector3 spawnPos = _guardBarrack.transform.position + (Vector3)(dir * dist);

        GuardController guard = GuardPool.Instance.GetGuard();
        guard.SetOriginBase(_guardBarrack);
        _guardBarrack.OnGuardSpawned();

        guard.transform.position = spawnPos;
    }
}
