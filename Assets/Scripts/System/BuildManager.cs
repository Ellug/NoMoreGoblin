using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _collisionTilemap;
    [SerializeField] private GridLayout _grid;              // 좌표 변환용
    [SerializeField] private Collider2D _confiner;

    [Header("Build Settings")]
    public bool IsBuildMode { get; private set; } = false;
    private BuildingData _selectedBuilding;
    private GameObject _previewObject;

    [SerializeField] private GameObject buildUI;

    private static TileBase _dummyTile;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // 더미타일 초기화
             if (_dummyTile == null)
                _dummyTile = ScriptableObject.CreateInstance<Tile>();
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

        // 기존 프리뷰 제거
        if (_previewObject != null) Destroy(_previewObject);

        // 새 프리뷰 생성
        _previewObject = Instantiate(building.previewPrefab);
    }


    // Bottom-Center 기준
    private void FollowMousePreview()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        // 건물 하단 중앙 타일
        Vector3Int centerCell = _grid.WorldToCell(mouseWorld);

        // 좌측 하단 타일로 변환
        Vector3Int bottomLeftCell = GetBottomLeftCell(centerCell, _selectedBuilding.size);

        // Bottom-Center pivot 기준 월드 위치 계산
        Vector3 bottomLeftWorld = _grid.CellToWorld(bottomLeftCell);
        Vector3 pivotWorldPos = bottomLeftWorld + new Vector3(_selectedBuilding.size.x * 0.5f, 0f, 0f);

        _previewObject.transform.position = pivotWorldPos;

        // 설치 가능 여부에 따라 색 변경
        bool canPlace = CanPlace(centerCell);
        SetPreviewColor(canPlace);
    }

    private void SetPreviewColor(bool canBuild)
    {
        var sr = _previewObject.GetComponent<SpriteRenderer>();

        if (sr == null) return;

        if (canBuild)
            sr.color = new Color(0f, 1f, 0f, 0.5f);    // 초록색
        else
            sr.color = new Color(1f, 0f, 0f, 0.5f);    // 빨간색
    }

    // 마우스 위치에서 설치 시도
    public void TryPlaceBuilding(Vector3 worldPos)
    {
        // UI 클릭 중이면 설치 금지
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (!IsBuildMode || _selectedBuilding == null) 
            return;

        Vector3Int centerCell = _grid.WorldToCell(worldPos);

        if (CanPlace(centerCell))
            Place(centerCell);
    }

    // 설치 가능 여부 검사
    private bool CanPlace(Vector3Int centerCell)
    {
        Vector2Int size = _selectedBuilding.size;
        Vector3Int bottomLeftCell = GetBottomLeftCell(centerCell, size);

        int width = size.x;
        int height = size.y;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = bottomLeftCell + new Vector3Int(x, y, 0);

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

    // 설치 실행
    private void Place(Vector3Int centerCell)
    {
        // 비용 검사
        if (!ResourceManager.Instance.TryConsume(ResourceType.Wood, _selectedBuilding.woodCost))
        {
            FloatingTextManager.Instance.ShowText(
                "자원이 부족합니다.",
                centerCell,
                Color.red,
                50f,
                2f
            );
            return;
        }

        Vector2Int size = _selectedBuilding.size;
        Vector3Int bottomLeftCell = GetBottomLeftCell(centerCell, size);

        // bottom-left 기준으로 pivot = Bottom-Center 위치 계산
        Vector3 bottomLeftWorld = _grid.CellToWorld(bottomLeftCell);
        Vector3 worldPos = bottomLeftWorld + new Vector3(size.x * 0.5f, 0f, 0f);

        // 건물 생성 및 타일 정보 전달
        GameObject b = Instantiate(_selectedBuilding.prefab, worldPos, Quaternion.identity);

        // 충돌 타일 칠해지기 전에 차지한 타일 리스트 계산
        var occupied = CalculateOccupiedTiles(bottomLeftCell, size);

        // 건물에게 알려주기
        if (b.TryGetComponent<BaseBuilding>(out var building))
            building.SetOccupiedCells(occupied);

        // 충돌 타일맵 채우기
        MarkCollision(occupied);

        // 프리뷰 제거
        if (_previewObject != null)
            Destroy(_previewObject);

        // 건물 선택 해제
        _selectedBuilding = null;
    }

    // 충돌 타일맵 채우기
    private void MarkCollision(List<Vector3Int> cells)
    {
        foreach (var pos in cells)
            _collisionTilemap.SetTile(pos, _dummyTile);
    }

    // 타일맵 마크 제거
    public void RemoveCollision(List<Vector3Int> cells)
    {
        foreach (var pos in cells)
            _collisionTilemap.SetTile(pos, null);
    }

    // 타일 리스트
    private List<Vector3Int> CalculateOccupiedTiles(Vector3Int bottomLeftCell, Vector2Int size)
    {
        List<Vector3Int> result = new();

        int width = size.x;
        int height = size.y;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = bottomLeftCell + new Vector3Int(x, y, 0);
                result.Add(pos);
            }
        }

        return result;
    }

    /// 실제 타일 영역의 좌측 하단 타일을 계산
    /// width 홀수로 정해야 안정적
    private Vector3Int GetBottomLeftCell(Vector3Int centerCell, Vector2Int size)
    {
        int width = size.x;

        // center 기준 왼쪽 계산
        int halfLeft = (width - 1) / 2;

        int originX = centerCell.x - halfLeft;
        int originY = centerCell.y;

        return new Vector3Int(originX, originY, centerCell.z);
    }

    public bool IsGround(Vector3 worldPos)
    {
        Vector3Int cell = _grid.WorldToCell(worldPos);
        return _groundTilemap.HasTile(cell);
    }

    public bool IsCollisionTile(Vector3 worldPos)
    {
        Vector3Int cell = _grid.WorldToCell(worldPos);
        return _collisionTilemap.HasTile(cell);
    }

    public bool IsInsideConfiner(Vector3 pos)
    {
        return _confiner.OverlapPoint(pos);
    }
}
