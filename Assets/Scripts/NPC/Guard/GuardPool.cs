using System.Collections.Generic;
using UnityEngine;

public class GuardPool : MonoBehaviour
{
    public static GuardPool Instance { get; private set; }

    [SerializeField] private GameObject _guardPrefab;
    [SerializeField] private int _initialCount = 50;

    private Queue<GuardController> _pool = new Queue<GuardController>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 초기 생성
        for (int i = 0; i < _initialCount; i++)
            AddGuard();        
    }

    private void AddGuard()
    {
        GameObject obj = Instantiate(_guardPrefab, transform);
        obj.SetActive(false);
        _pool.Enqueue(obj.GetComponent<GuardController>());
    }

    public GuardController GetGuard()
    {
        if (_pool.Count == 0)
            AddGuard();

        GuardController guard = _pool.Dequeue();
        guard.ResetGuard();
        guard.gameObject.SetActive(true);
        return guard;
    }

    public void ReturnGuard(GuardController guard)
    {
        guard.gameObject.SetActive(false);
        _pool.Enqueue(guard);
    }
}
