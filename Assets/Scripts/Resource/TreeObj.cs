using UnityEngine;
using UnityEngine.Tilemaps;

public class TreeObj : MonoBehaviour
{
    [SerializeField] private float _maxHp = 10f;
    [SerializeField] private float _exp = 1f;
    private float _curHp;

    private Tilemap _collisionTilemap;
    private Vector3Int _cellPos;

    void Awake()
    {
        _curHp = _maxHp;
    }

    public void Init(Tilemap collisionTilemap, Vector3Int cellPos)
    {
        _collisionTilemap = collisionTilemap;
        _cellPos = cellPos;
    }

    public void TakeDamage(float dmg, PlayerController attacker)
    {
        _curHp -= dmg;
        if (_curHp <= 0)
            Die(attacker);
    }

    private void Die(PlayerController attacker)
    {
        attacker.AddExp(_exp);
        // 목재 획득 추가 필요

        // 타일맵 충돌 제거
        if (_collisionTilemap != null)
            _collisionTilemap.SetTile(_cellPos, null);
            
        Destroy(gameObject);
    }
}
