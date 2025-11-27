using System.Collections.Generic;
using UnityEngine;

public class CitizenPool : MonoBehaviour
{
    public static CitizenPool Instance { get; private set; }

    [SerializeField] private GameObject[] _citizenPrefab;
    [SerializeField] private int _initialCount = 50;

    private Queue<CitizenController> _pool = new Queue<CitizenController>();

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
            AddCitizen();        
    }

    private void AddCitizen()
    {
        int idx = Random.Range(0, _citizenPrefab.Length);
        GameObject prefab = _citizenPrefab[idx];

        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        _pool.Enqueue(obj.GetComponent<CitizenController>());
    }

    public CitizenController GetCitizen()
    {
        if (_pool.Count == 0)
            AddCitizen();

        CitizenController citizen = _pool.Dequeue();
        citizen.ResetCitizen();
        citizen.gameObject.SetActive(true);
        return citizen;
    }

    public void ReturnCitizen(CitizenController citizen)
    {
        citizen.gameObject.SetActive(false);
        _pool.Enqueue(citizen);
    }
}
