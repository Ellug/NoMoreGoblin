using UnityEngine;

public class DynamicSortedObject : MonoBehaviour
{
    public SpriteRenderer _sr { get; private set; }

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        SortManager.Instance.RegisterDynamic(this);
    }

    private void OnDestroy()
    {
        if (SortManager.Instance != null)
            SortManager.Instance.UnregisterDynamic(this);
    }
}
