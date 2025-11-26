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
        CitizenController citizen = CitizenPool.Instance.GetCitizen();
        citizen.SetOriginBase(_house);
        _house.OnCitizenSpawned();

        ResourceManager.Instance.Add(ResourceType.NPC, 1);
        citizen.transform.position = _door.position;
    }
}
