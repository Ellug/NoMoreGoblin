using UnityEngine;

public class GoblinSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int _spawnCount = 2;
    [SerializeField] private float _interval = 10f;
    [SerializeField] private Vector2 _spawnAreaSize = new Vector2(30f, 30f);
    [SerializeField] private Transform _spawnPoint;

    private float _timer;

    private void Update()
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
        Vector2 randomPos = new Vector2(
            Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
            Random.Range(-_spawnAreaSize.y / 2, _spawnAreaSize.y / 2)
        );

        Vector3 worldPos = _spawnPoint.position + (Vector3)randomPos;

        Goblin goblin = GoblinPool.Instance.GetGoblin();
        goblin.transform.position = worldPos;
    }
}
