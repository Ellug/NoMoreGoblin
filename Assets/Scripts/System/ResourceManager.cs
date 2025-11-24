using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private int _wood = 0;
    [SerializeField] private int _food = 0;
    [SerializeField] private TMP_Text _woodCountText;
    [SerializeField] private TMP_Text _foodCountText;

    // Properties
    public int CurrentWood => _wood;
    public int CurrentFood => _food;

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
    }

    void Start()
    {
        RefreshUI();
    }

    public void AddWood(int amout)
    {
        _wood += amout;
        RefreshUI();
    }

    public bool TryConsume( int amout)
    {
        if (_wood < amout) return false;

        _wood -= amout;
        RefreshUI();
        return true;
    }

    private void RefreshUI()
    {
        if (_woodCountText != null)
            _woodCountText.text = _wood.ToString();

        if (_foodCountText != null)
            _foodCountText.text = _food.ToString();
    }
}
