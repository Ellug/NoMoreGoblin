using TMPro;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [SerializeField] private GameObject floatingTextPrefab;

    private Camera _cam;

    private void Awake()
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

        _cam = Camera.main;
    }

    public void ShowText(string msg, Vector3 pos, Color color, float speed, float duration = 2f)
    {
        Vector3 screenPos = _cam.WorldToScreenPoint(pos);

        GameObject obj = Instantiate(floatingTextPrefab, transform);
        TMP_Text text = obj.GetComponent<TMP_Text>();
        FloatingText ft = obj.GetComponent<FloatingText>();

        text.text = msg;
        text.color = color;
        
        obj.transform.position = screenPos;
        ft.Setup(color, duration, speed);
    }
}
