using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelupOptionButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _desc;
    [SerializeField] private Button _button;

    private LevelupOption _option;
    private System.Action<LevelupOption> _callback;

    public void Setup(LevelupOption option, System.Action<LevelupOption> callback)
    {
        _option = option;
        _callback = callback;

        _title.text = option.Name;
        _desc.text = option.Description;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => _callback?.Invoke(_option));
    }
}
