using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData data;

    public Vector2Int Size => data.size;
    public int WoodCost => data.woodCost;
}
