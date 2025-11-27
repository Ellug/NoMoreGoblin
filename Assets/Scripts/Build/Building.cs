using UnityEngine;

public class Building : BaseBuilding
{
    // 일단 Fence에만 쓰임
        
    public Vector2Int Size => data.size;
    public int WoodCost => data.woodCost;
}
