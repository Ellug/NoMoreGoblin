using UnityEngine;
using UnityEngine.UI;

public class GuardView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider _hpSlider;

    public void UpdateHpBar(float ratio)
    {
        if (_hpSlider != null)
            _hpSlider.value = Mathf.Clamp01(ratio);
    }

    public void ShowDeath()
    {
        Debug.Log("Guard is Dead");
        // TODO: 사망 애니메이션 / UI 표시
    }
}
