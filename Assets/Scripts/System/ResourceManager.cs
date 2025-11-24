using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField] private int _wood = 0;

    // Properties
    public int CurrentWood => _wood;

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

    public void AddWood(int amout)
    {
        _wood += amout;
    }

    public bool TryConsume( int amout)
    {
        if (_wood < amout) return false;

        _wood -= amout;
        return true;
    }
}
