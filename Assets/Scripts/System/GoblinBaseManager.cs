using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GoblinBaseManager : MonoBehaviour
{
    public static GoblinBaseManager Instance { get; private set; }

    [Header("Setting")]
    [SerializeField] private int _baseCount = 7;
    [SerializeField] private float _minDistanceFromStart = 200f;
    [SerializeField] private float _minDistanceBetweenBases = 60f;

    [Header("References")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _collisionTilemap;
    [SerializeField] private Transform _startZone;
    [SerializeField] private GameObject _goblinBasePrefab;

    private int _curBaseCount;
    private List<GoblinBase> _goblinBases = new List<GoblinBase>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        _curBaseCount = _baseCount;
    }

    private void Start()
    {
        GenerateBases();
    }

    private void GenerateBases()
    {
        BoundsInt bounds = _groundTilemap.cellBounds;

        int attempts = 0;
        int maxAttempts = _baseCount * 50;

        while (_goblinBases.Count < _baseCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 world = GetRandomWorldPosition(bounds);

            // StartZone 거리 체크
            if (Vector3.Distance(world, _startZone.position) < _minDistanceFromStart)
                continue;

            Vector3Int cell = _groundTilemap.WorldToCell(world);

            // 맵 안에 실제 타일이 존재해야 함
            if (!_groundTilemap.HasTile(cell))
                continue;

            // 충돌 타일이 있으면 불가
            if (_collisionTilemap.HasTile(cell))
                continue;

            // 다른 베이스와 거리 체크
            if (!IsFarFromOtherBases(world))
                continue;

            // 베이스 생성
            Vector3 spawnPos = _groundTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);

            GameObject obj = Instantiate(_goblinBasePrefab, spawnPos, Quaternion.identity);
            GoblinBase gb = obj.GetComponent<GoblinBase>();

            _goblinBases.Add(gb);
        }
    }

    private Vector3 GetRandomWorldPosition(BoundsInt bounds)
    {
        int x = Random.Range(bounds.xMin, bounds.xMax);
        int y = Random.Range(bounds.yMin, bounds.yMax);

        return _groundTilemap.CellToWorld(new Vector3Int(x, y, 0));
    }

    private bool IsFarFromOtherBases(Vector3 pos)
    {
        foreach (var b in _goblinBases)
        {
            if (Vector3.Distance(pos, b.transform.position) < _minDistanceBetweenBases)
                return false;
        }
        return true;
    }

    // 베이스 파괴 시 호출
    public void OnBaseDestroyed(GoblinBase destroyedBase)
    {
        // 리스트에서 제거
        _goblinBases.Remove(destroyedBase);
        _curBaseCount--;

        int enhanceAmount = _baseCount - _curBaseCount;

        if (_curBaseCount > 0)
        {
            // 강화 처리
            foreach (var b in _goblinBases)
            {
                b.maxGoblinCount += 10 * enhanceAmount;
                b.spawnRadius += 5f;

                var spawner = b.GetComponent<GoblinSpawner>();
                if (spawner != null)
                    spawner.IncreaseSpawnCount(enhanceAmount);
            }
        }
        else
        {
            GameManager.Instance.GameWin();
        }
    }
}
