using UnityEngine;
using UnityEngine.Tilemaps;

public class TreeObj : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHp = 10f;
    [SerializeField] private float _curHp;
    [SerializeField] private float _exp = 1f;

    public DamageableType Type => DamageableType.Tree;
    private bool _isAlive;
    public bool IsAlive => _isAlive;

    private Tilemap _collisionTilemap;
    private Vector3Int _cellPos;

    void Awake()
    {
        _curHp = _maxHp;
        _isAlive = true;
    }

    public void Init(Tilemap collisionTilemap, Vector3Int cellPos)
    {
        _collisionTilemap = collisionTilemap;
        _cellPos = cellPos;
    }

    public void TakeDamage(float dmg, GameObject attacker)
    {
        _curHp -= dmg;
        if (_curHp <= 0)
            Die(attacker);
    }

    private void Die(GameObject attacker)
    {
        // 경험치 get - Player가 죽였을 때만
        if (attacker.TryGetComponent<PlayerController>(out var player))
            player.AddExp(_exp);

        // 목재 획득 1~3 랜덤
        int woodGain = Random.Range(1, 4);
        ResourceManager.Instance.AddWood(woodGain);

        // 획득 플로팅 텍스트
        Vector3 textPos = transform.position + Vector3.up * 1f;
        FloatingTextManager.Instance.ShowText(
            $"Wood +{woodGain}",
            textPos,
            Color.skyBlue,
            40f,
            1.2f
        );

        // 타일맵 충돌 제거
        if (_collisionTilemap != null)
            _collisionTilemap.SetTile(_cellPos, null);

        _isAlive = false;  
        Destroy(gameObject);
    }
}
