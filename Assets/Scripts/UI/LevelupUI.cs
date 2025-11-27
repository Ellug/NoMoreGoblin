using System.Collections.Generic;
using UnityEngine;

public class LevelupUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    // [SerializeField] private LevelupOptionButtonUI[] _optionButtons;
    [SerializeField] private Transform _root;
    [SerializeField] private LevelupOptionButtonUI _optionPrefab;

    private System.Action<LevelupOption> _callback;

    public void Open(List<LevelupOption> options, System.Action<LevelupOption> callback)
    {
        _callback = callback;
        _panel.SetActive(true);

        // 기존 UI 버튼 정리 (중복 생성 방지)
        foreach (Transform child in _root)
            Destroy(child.gameObject);

        // 새 버튼 생성
        foreach (var opt in options)
        {
            var btn = Instantiate(_optionPrefab, _root);
            btn.Setup(opt, OnOptionClicked);
        }
    }

    private void OnOptionClicked(LevelupOption opt)
    {
        _callback?.Invoke(opt);
    }

    public void Close()
    {
        _panel.SetActive(false);
    }
}
