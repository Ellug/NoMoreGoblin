using UnityEngine;

public class StaticSortedObject : MonoBehaviour
{
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        SortManager.Instance.ApplySorting(_sr);
    }
}
