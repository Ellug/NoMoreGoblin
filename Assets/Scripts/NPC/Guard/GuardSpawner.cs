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
        ResourceManager res = ResourceManager.Instance;

        if (!res.TryConsume(ResourceType.Food, 3))
        {
            FloatingTextManager.Instance.ShowText(
                "식량이 부족합니다.",
                _door.position,
                Color.red,
                50f,
                2f
            );
            return;
        }

        if (!res.TryConsume(ResourceType.Wood, 1))
        {
            FloatingTextManager.Instance.ShowText(
                "나무가 부족합니다.",
                _door.position,
                Color.red,
                50f,
                2f
            );
            return;
        }

        GuardController guard = GuardPool.Instance.GetGuard();
        guard.SetOriginBase(_guardBarrack);
        _guardBarrack.OnGuardSpawned();

        res.Add(ResourceType.Guard, 1);
        guard.transform.position = _door.position;
    }
}
