using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TreeSpawnerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _collisionTilemap;
    [SerializeField] private GameObject _treePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int _treeCount = 2000;
    [SerializeField] private float _minDistanceBetweenTrees = 1.3f;
    [SerializeField] private float _safeRadius = 30f;

    [Header("Start Zone")]
    [SerializeField] private Transform _startZoneCenter;

    private List<Vector3> _spawnedPositions = new List<Vector3>();

    void Start()
    {
        SpawnTrees();
    }

    private void SpawnTrees()
    {
        // Tilemap BoundsInt 범위 Load
        BoundsInt bounds = _groundTilemap.cellBounds;

        int attemps = 0;
        int maxAttempts = _treeCount * 10;

        while (_spawnedPositions.Count < _treeCount && attemps < maxAttempts)
        {
            attemps++;

            Vector3 randomWorld = GetRandomWorldPosition(bounds);

            // StartZone 제외
            if (Vector3.Distance(randomWorld, _startZoneCenter.position) < _safeRadius) continue;

            Vector3Int cell = _groundTilemap.WorldToCell(randomWorld);

            // 설치 불가 예외 처리
            // 땅 없음
            if (!_groundTilemap.HasTile(cell)) continue;

            // 기존 충돌 타일
            if (_collisionTilemap.HasTile(cell)) continue;

            // 기존 트리들과 거리 체크
            if (!IsFarEnough(randomWorld)) continue;

            Vector3 spawnPos = _groundTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
            var tree = Instantiate(_treePrefab, spawnPos, Quaternion.identity, transform);
            tree.GetComponent<TreeObj>().Init(_collisionTilemap, cell);

            // 타일맵에 충돌 표시
            _collisionTilemap.SetTile(cell, ScriptableObject.CreateInstance<Tile>());

            _spawnedPositions.Add(spawnPos);
        }
    }

    private Vector3 GetRandomWorldPosition(BoundsInt bounds)
    {
        int x = Random.Range(bounds.xMin, bounds.xMax);
        int y = Random.Range(bounds.yMin, bounds.yMax);

        Vector3Int cell = new Vector3Int(x, y, 0);
        return _groundTilemap.CellToWorld(cell);
    }

    private bool IsFarEnough(Vector3 pos)
    {
        foreach (var p in _spawnedPositions)
        {            
            if (Vector3.Distance(pos, p) < _minDistanceBetweenTrees)
                return false;
        }
        return true;
    }
}
