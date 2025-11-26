using System.Collections.Generic;
using UnityEngine;

public class SortManager : MonoBehaviour
{
    public static SortManager Instance { get; private set; }

    [Header("Sorting Config")]
    [SerializeField] private float worldOffsetY = 1000f;
    [SerializeField] private float multiplier = 10f;

    private readonly List<DynamicSortedObject> dynamicList = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void LateUpdate()
    {
        UpdateDynamicObjects();
    }

    public void RegisterDynamic(DynamicSortedObject obj)
    {
        if (!dynamicList.Contains(obj))
            dynamicList.Add(obj);
    }

    public void UnregisterDynamic(DynamicSortedObject obj)
    {
        if (dynamicList.Contains(obj))
            dynamicList.Remove(obj);
    }

    private void UpdateDynamicObjects()
    {
        foreach (var obj in dynamicList)
        {
            if (obj == null) continue;
            ApplySorting(obj.Sr);
        }
    }

    // sr 바닥 기준 정렬 계산
    public void ApplySorting(SpriteRenderer sr)
    {
        float bottomY = sr.bounds.min.y - 2.5f;
        sr.sortingOrder = -(int)((bottomY + worldOffsetY) * multiplier);
    }
}
