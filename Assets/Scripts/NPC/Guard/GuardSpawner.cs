using UnityEngine;

public class GuardSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float _interval = 15f;
    [SerializeField] private Transform _door;
    
    
    private float _timer;
    private GuardBarrack _guardBarrack;

    void Awake()
    {
        _guardBarrack = GetComponent<GuardBarrack>();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _interval)
        {
            _timer = 0f;
            SpawnGuard();
        }
    }

    private void SpawnGuard()
    {
        GuardController guard = GuardPool.Instance.GetGuard();
        guard.SetOriginBase(_guardBarrack);
        _guardBarrack.OnGuardSpawned();

        guard.transform.position = _door.position;
    }
}
