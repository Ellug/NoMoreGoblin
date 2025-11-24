using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 30f;
    private float _fadeDuration = 1f;
    private TMP_Text _text;
    private Color _color;
    private float _timer = 0f;

    void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _color = _text.color;
    }

    void Update()
    {
        _timer += Time.deltaTime;

        transform.Translate(Vector3.up * _moveSpeed * Time.deltaTime);

        float alpha = Mathf.Lerp(1f, 0f, _timer / _fadeDuration);
        Color c = _color;
        c.a = alpha;
        _text.color = c;

        if (_timer >= _fadeDuration)
            Destroy(gameObject);
    }

    public void Setup(Color color, float duration, float speed)
    {
        _fadeDuration = duration;

        _color = color;
        _text.color = color;
        _moveSpeed = speed;
    }
}
