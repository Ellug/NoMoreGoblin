using System.Collections.Generic;
using UnityEngine;

public class GoblinPool : MonoBehaviour
{
    public static GoblinPool Instance { get; private set; }

    [SerializeField] private GameObject _goblinPrefab;
    [SerializeField] private int _initialCount = 100;

    private Queue<Goblin> _pool = new Queue<Goblin>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기 생성
        for (int i = 0; i < _initialCount; i++)
            AddGoblin();
    }

    private void AddGoblin()
    {
        GameObject obj = Instantiate(_goblinPrefab, transform);
        obj.SetActive(false);
        _pool.Enqueue(obj.GetComponent<Goblin>());
    }

    public Goblin GetGoblin()
    {
        if (_pool.Count == 0)
            AddGoblin();

        Goblin goblin = _pool.Dequeue();
        goblin.gameObject.SetActive(true);
        goblin.ResetGoblin();
        return goblin;
    }

    public void ReturnGoblin(Goblin goblin)
    {
        goblin.gameObject.SetActive(false);
        _pool.Enqueue(goblin);
    }
}
