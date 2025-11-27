using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Vector2Int size = new Vector2Int(1, 1); // 건물이 차지할 타일 크기
    public int woodCost; // 재료
    public GameObject prefab;
    public GameObject previewPrefab;
}
