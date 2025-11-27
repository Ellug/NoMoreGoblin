using UnityEngine;

public class GoblinSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int _spawnCount = 2;
    [SerializeField] private float _interval = 15f;
    [SerializeField] private Transform _door;
    
    private float _timer;
    private GoblinBase _goblinBase;

    void Awake()
    {
        _goblinBase = GetComponent<GoblinBase>();
    }

    void Update()
    {
        if (_goblinBase.currentGoblinCount >= _goblinBase.maxGoblinCount) return;

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
        Goblin goblin = GoblinPool.Instance.GetGoblin();
        goblin.SetOriginBase(_goblinBase);
        _goblinBase.RegisterGoblin(goblin);

        goblin.transform.position = _door.position;
    }

    public void IncreaseSpawnCount(int amount)
    {
        _spawnCount += amount;
    }
}
