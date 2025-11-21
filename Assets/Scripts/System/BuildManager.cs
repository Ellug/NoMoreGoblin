using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _collisionTilemap;
    [SerializeField] private GridLayout _grid;              // 좌표 변환용

    [Header("Build Settings")]
    public bool IsBuildMode { get; private set; } = false;
    private BuildingData _selectedBuilding;
    private GameObject _previewObject;

    [SerializeField] private GameObject buildUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;            
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (!IsBuildMode || _selectedBuilding == null || _previewObject == null) return;

        FollowMousePreview();
    }

    public void ToggleBuildMode()
    {
        IsBuildMode = !IsBuildMode;

        // Build Mode Enter
        if (IsBuildMode)
        {
            buildUI.SetActive(true);
            return;
        }

        // Build Mode Exit
        _selectedBuilding = null;

        if (_previewObject != null)
            Destroy(_previewObject);

        buildUI.SetActive(false);
    }

    // 건물 선택
    public void SelectBuilding(BuildingData building)
    {
        if (!IsBuildMode) return;

        _selectedBuilding = building;
        Debug.Log($"{building.buildingName} 선택됨");

        // 기존 프리뷰 제거
        if (_previewObject != null) Destroy(_previewObject);

        // 새 프리뷰 생성
        _previewObject = Instantiate(building.previewPrefab);
    }


    private void FollowMousePreview()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0;

        Vector3Int cellPos = _grid.WorldToCell(mouseWorld);

        Vector3 worldCenter = _grid.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
        _previewObject.transform.position = worldCenter;

        // 설치 가능 여부에 따라 색 변경
        bool canPlace = CanPlace(cellPos);
        SetPreviewColor(canPlace);
    }

    private void SetPreviewColor(bool canBuild)
    {
        var sr = _previewObject.GetComponent<SpriteRenderer>();

        if (canBuild)
            sr.color = new Color(0, 1, 0, 0.5f);    // 초록색
        else
            sr.color = new Color(1, 0, 0, 0.5f);    // 빨간색
    }


    // 마우스 위치에서 설치 시도
    public void TryPlaceBuilding(Vector3 pos)
    {
        if (!IsBuildMode || _selectedBuilding == null) return;

        Vector3Int cellPos = _grid.WorldToCell(pos);
        
        if (CanPlace(cellPos))
        {
            Place(cellPos);
        }
        else
        {
            Debug.Log("설치 불가");
        }            
    }

    // 설치 가능 여부 확인
    private bool CanPlace(Vector3Int cellPos)
    {
        int width = _selectedBuilding.size.x;
        int height = _selectedBuilding.size.y;

        // 좌측 하단 anchor 기준
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = cellPos + new Vector3Int(x, y, 0);

                // 땅 타일이 없는 경우
                if (!_groundTilemap.HasTile(pos))
                    return false;

                // 충돌 타일이 있는 경우 (나무 / 건물 등)
                if (_collisionTilemap.HasTile(pos))
                    return false;
            }
        }

        return true;
    }

    // 설치
    private void Place(Vector3Int cellPos)
    {
        Vector3 worldPos = _grid.CellToWorld(cellPos);
        worldPos += new Vector3(0.5f, 0.5f, 0); // 타일 중앙 정렬

        Instantiate(_selectedBuilding.prefab, worldPos, Quaternion.identity);

        // 충돌 타일맵에 해당 영역을 사용됨으로 표시
        MarkCollision(cellPos);
        
        // 프리뷰 제거
        if (_previewObject != null)
            Destroy(_previewObject);

        // 건물 선택 해제
        _selectedBuilding = null;
    }

    // 설치 구역 충돌 타일맵에 표시
    private void MarkCollision(Vector3Int cellPos)
    {
        int width = _selectedBuilding.size.x;
        int height = _selectedBuilding.size.y;

        TileBase dummy = ScriptableObject.CreateInstance<Tile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = cellPos + new Vector3Int(x, y, 0);
                _collisionTilemap.SetTile(pos, dummy);
            }
        }
    }
}
