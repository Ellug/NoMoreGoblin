using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private BoxCollider2D _confinerCollider;
    [SerializeField] private RectTransform _minimapArea;
    [SerializeField] private RectTransform _playerDot;
    [SerializeField] private RectTransform _viewRect;
    [SerializeField] private GameObject _treeDotPrefab;
    [SerializeField] private GameObject _buildingDotPrefab;
    [SerializeField] private float _scanInterval = 1.5f;

    private Camera _cam;
    private Vector2 _confinerMin;
    private Vector2 _confinerMax;
    private WaitForSeconds _minimapScanDelay;

    private Dictionary<TreeObj, RectTransform> _treeIcons = new();
    private Dictionary<BaseBuilding, RectTransform> _buildingIcons = new();

    void Start()
    {
        Bounds b = _confinerCollider.bounds;
        _confinerMin = b.min;
        _confinerMax = b.max;
        _cam = Camera.main;
        _minimapScanDelay = new WaitForSeconds(_scanInterval);

        StartCoroutine(ScanRoutine());
    }

    void Update()
    {
        UpdatePlayerDot();
        UpdateCameraRect();
    }

    private IEnumerator ScanRoutine()
    {
        while (true)
        {
            UpdateMinimapIcons();
            yield return _minimapScanDelay;
        }
    }

    private void UpdateMinimapIcons()
    {
        // 트리 갱신
        TreeObj[] allTrees = FindObjectsByType<TreeObj>(FindObjectsSortMode.None);

        // 새로 생긴 트리 추가
        foreach (var tree in allTrees)
        {
            if (!_treeIcons.ContainsKey(tree))
                AddTreeIcon(tree);
        }

        // 사라진 트리 제거
        List<TreeObj> removedTrees = new List<TreeObj>();

        foreach (var kvp in _treeIcons)
        {
            if (kvp.Key == null)
                removedTrees.Add(kvp.Key);
        }

        foreach (var t in removedTrees)
        {
            Destroy(_treeIcons[t].gameObject);
            _treeIcons.Remove(t);
        }

        // 건물 갱신
        BaseBuilding[] allBuildings = FindObjectsByType<BaseBuilding>(FindObjectsSortMode.None);

        // 새로 생긴 건물 추가
        foreach (var b in allBuildings)
        {
            if (!_buildingIcons.ContainsKey(b))
                AddBuildingIcon(b);
        }

        // 사라진 건물 제거
        List<BaseBuilding> removedBuildings = new List<BaseBuilding>();

        foreach (var kvp in _buildingIcons)
        {
            if (kvp.Key == null)
                removedBuildings.Add(kvp.Key);
        }

        foreach (var b in removedBuildings)
        {
            Destroy(_buildingIcons[b].gameObject);
            _buildingIcons.Remove(b);
        }

        // 위치 업데이트
        UpdateIconPositions();
    }

    // 아이콘 생성
    private void AddTreeIcon(TreeObj tree)
    {
        RectTransform icon = Instantiate(_treeDotPrefab, _minimapArea).GetComponent<RectTransform>();
        _treeIcons.Add(tree, icon);
    }

    private void AddBuildingIcon(BaseBuilding building)
    {
        RectTransform icon = Instantiate(_buildingDotPrefab, _minimapArea).GetComponent<RectTransform>();
        _buildingIcons.Add(building, icon);
    }

    // 위치 실시간 갱신
    private void UpdateIconPositions()
    {
        foreach (var kvp in _treeIcons)
        {            
            if (kvp.Key != null)
                kvp.Value.anchoredPosition = WorldToMinimap(kvp.Key.transform.position);
        }

        foreach (var kvp in _buildingIcons)
        {            
            if (kvp.Key != null)
                kvp.Value.anchoredPosition = WorldToMinimap(kvp.Key.transform.position);
        }
    }

    private Vector2 WorldToMinimap(Vector3 worldPos)
    {
        Vector2 normalized = new Vector2(
            Mathf.InverseLerp(_confinerMin.x, _confinerMax.x, worldPos.x),
            Mathf.InverseLerp(_confinerMin.y, _confinerMax.y, worldPos.y)
        );

        Vector2 panelSize = _minimapArea.rect.size;

        return new Vector2(
            (normalized.x * panelSize.x) - (panelSize.x * 0.5f),
            (normalized.y * panelSize.y) - (panelSize.y * 0.5f)
        );
    }

    private void UpdatePlayerDot()
    {
        Vector2 worldPos = _player.position;

        // Confiner 내부에서 노멀라이즈 (0~1)
        Vector2 normalized = new Vector2(
            Mathf.InverseLerp(_confinerMin.x, _confinerMax.x, worldPos.x),
            Mathf.InverseLerp(_confinerMin.y, _confinerMax.y, worldPos.y)
        );

        // MinimapPanel 내부 좌표로 변환
        Vector2 panelSize = _minimapArea.rect.size;

        Vector2 uiPos = new Vector2(
            (normalized.x * panelSize.x) - (panelSize.x * 0.5f),
            (normalized.y * panelSize.y) - (panelSize.y * 0.5f)
        );

        _playerDot.anchoredPosition = uiPos;
    }

    private void UpdateCameraRect()
    {
        // 카메라 시야 월드 영역 계산
        float halfH = _cam.orthographicSize;
        float halfW = halfH * _cam.aspect;

        // 카메라 Transform
        Vector3 camPos = _cam.transform.position;

        float left = camPos.x - halfW;
        float right = camPos.x + halfW;
        float bottom = camPos.y - halfH;
        float top = camPos.y + halfH;

        // Confiner 대비 Normalized (0~1)
        float nLeft = Mathf.InverseLerp(_confinerMin.x, _confinerMax.x, left);
        float nRight = Mathf.InverseLerp(_confinerMin.x, _confinerMax.x, right);
        float nBottom = Mathf.InverseLerp(_confinerMin.y, _confinerMax.y, bottom);
        float nTop = Mathf.InverseLerp(_confinerMin.y, _confinerMax.y, top);

        // UI 크기 변환
        Vector2 panelSize = _minimapArea.rect.size;

        float uiLeft = (nLeft * panelSize.x) - (panelSize.x * 0.5f);
        float uiRight = (nRight * panelSize.x) - (panelSize.x * 0.5f);
        float uiBottom = (nBottom * panelSize.y) - (panelSize.y * 0.5f);
        float uiTop = (nTop * panelSize.y) - (panelSize.y * 0.5f);

        float width = uiRight - uiLeft;
        float height = uiTop - uiBottom;

        // ViewRect UI 배치
        _viewRect.anchoredPosition = new Vector2(
            (uiLeft + uiRight) * 0.5f,
            (uiBottom + uiTop) * 0.5f
        );

        _viewRect.sizeDelta = new Vector2(width, height);
    }
}
