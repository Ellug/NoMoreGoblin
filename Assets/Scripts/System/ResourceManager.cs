using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ResourceType
{
    Wood,
    Food,
    NPC,
    Guard
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private TMP_Text _woodText;
    [SerializeField] private TMP_Text _foodText;
    [SerializeField] private TMP_Text _npcText;
    [SerializeField] private TMP_Text _guardText;

    private Dictionary<ResourceType, int> _resources = new();
    private Dictionary<ResourceType, TMP_Text> _uiTexts;

    private float _interval = 30f;
    private float _timer = 0f;

    private Camera _cam;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기화
        _resources[ResourceType.Wood]  = 0;
        _resources[ResourceType.Food]  = 0;
        _resources[ResourceType.NPC]   = 0;
        _resources[ResourceType.Guard] = 0;

        _uiTexts = new()
        {
            { ResourceType.Wood,  _woodText },
            { ResourceType.Food,  _foodText },
            { ResourceType.NPC,   _npcText },
            { ResourceType.Guard, _guardText }
        };

        _cam = Camera.main;
    }

    void Start()
    {
        RefreshAllUI();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _interval)
        {
            _timer = 0;
            ConsumeResources();
        }
    }

    private void ConsumeResources()
    {
        int npc = _resources[ResourceType.NPC];
        int guard = _resources[ResourceType.Guard];

        // Food += NPC
        if (npc > 0)
            Add(ResourceType.Food, npc);

        // Food -= Guard * 2
        if (guard > 0)
            Add(ResourceType.Food, -guard * 2);
    }

    public void Add(ResourceType type, int amount)
    {
        _resources[type] += amount;

        // 음수 나오면 0
        if (_resources[type] < 0)
            _resources[type] = 0;

        RefreshUI(type);

        if (amount != 0)
            ShowFloating(type, amount);
    }

    public bool TryConsume(ResourceType type, int amount)
    {
        if (_resources[type] < amount)
            return false;

        _resources[type] -= amount;
        RefreshUI(type);
        ShowFloating(type, -amount);

        return true;
    }

    public int Get(ResourceType type)
    {
        return _resources[type];
    }

    private void RefreshUI(ResourceType type)
    {
        if (_uiTexts[type] != null)
            _uiTexts[type].text = _resources[type].ToString();
    }

    private void RefreshAllUI()
    {
        foreach (var kvp in _resources)
            RefreshUI(kvp.Key);
    }

    private void ShowFloating(ResourceType type, int amount)
    {
        string prefix = amount > 0 ? "+" : "";
        string msg = $"{prefix}{amount} {type}";

        Color col = amount > 0 ? Color.green : Color.red;

        Vector3 textPos = new(100, Screen.height - 300f, 0f);

        FloatingTextManager.Instance.ShowText(
            msg,
            _cam.ScreenToWorldPoint(textPos),
            col,
            speed: 80f,
            duration: 1.5f
        );
    }
}
