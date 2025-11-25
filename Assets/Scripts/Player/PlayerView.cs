using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Slider _expSlider;

    public void UpdateHpBar(float ratio)
    {
        if (_hpSlider != null)
            _hpSlider.value = Mathf.Clamp01(ratio);
    }

    public void UpdateExpBar(float ratio)
    {
        if (_expSlider != null)
            _expSlider.value = Mathf.Clamp01(ratio);
    }

    public void ShowDeath()
    {
        Debug.Log("PlayerView: Show death UI / animation");
        // TODO: 사망 애니메이션 / UI 표시
    }
}
