using UnityEngine;

public class CitizenSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float _interval = 15f;
    [SerializeField] private Transform _door;
    
    private float _timer;
    private House _house;

    void Awake()
    {
        _house = GetComponent<House>();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _interval)
        {
            _timer = 0f;
            SpawnCitizen();
        }
    }

    private void SpawnCitizen()
    {
        // 집에 인구수 다 차면 생성 X
        if (!_house.CanAcceptMoreCitizens) return;

        CitizenController citizen = CitizenPool.Instance.GetCitizen();
        citizen.SetOriginBase(_house);

        ResourceManager.Instance.Add(ResourceType.NPC, 1);
        citizen.transform.position = _door.position;
    }
}
