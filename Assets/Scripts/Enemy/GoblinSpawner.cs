using UnityEngine;

public class GoblinSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int _spawnCount = 2;
    [SerializeField] private float _interval = 15f;
    [SerializeField] private float _spawnRadius = 30f;
    
    private float _timer;
    private GoblinBase _goblinBase;

    void Awake()
    {
        _goblinBase = GetComponent<GoblinBase>();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _interval)
        {
            _timer = 0f;
            SpawnGoblins();
        }
    }

    private void SpawnGoblins()
    {
        for (int i = 0; i < _spawnCount; i++)
            SpawnOne();
    }

    private void SpawnOne()
    {
        // 랜덤 방향, 반경
        Vector2 dir = Random.insideUnitCircle.normalized;
        float dist = Random.Range(0f, _spawnRadius);

        Vector3 spawnPos = _goblinBase.transform.position + (Vector3)(dir * dist);

        Goblin goblin = GoblinPool.Instance.GetGoblin();
        goblin.SetOriginBase(_goblinBase);
        _goblinBase.OnGoblinSpawned();

        goblin.transform.position = spawnPos;
    }

    public void IncreaseSpawnCount(int amount)
    {
        _spawnCount += amount;
        _spawnRadius += 5f;
    }
}
