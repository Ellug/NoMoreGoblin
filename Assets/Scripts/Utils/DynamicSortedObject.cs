using UnityEngine;

public class DynamicSortedObject : MonoBehaviour
{
    public SpriteRenderer Sr { get; private set; }
    private bool _isRegistered = false;

    private void Awake()
    {
        Sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (!_isRegistered && SortManager.Instance != null)
        {
            SortManager.Instance.RegisterDynamic(this);
            _isRegistered = true;
        }
    }

    private void OnDisable()
    {
        if (_isRegistered && SortManager.Instance != null)
        {
            SortManager.Instance.UnregisterDynamic(this);
            _isRegistered = false;
        }
    }

    private void OnDestroy()
    {
        if (_isRegistered && SortManager.Instance != null)
        {
            SortManager.Instance.UnregisterDynamic(this);
            _isRegistered = false;
        }
    }
}