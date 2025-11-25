using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelupOption
{
    public string Name;
    public string Description;
    public Action<PlayerModel> ApplyEffect;

    public LevelupOption(string name, string desc, Action<PlayerModel> effect)
    {
        Name = name;
        Description = desc;
        ApplyEffect = effect;
    }
}

public class LevelupManager : MonoBehaviour
{

    public static LevelupManager Instance { get; private set; }

    [SerializeField] private LevelupUI _ui;
    private PlayerModel _targetModel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void RequestLevelup(PlayerModel model)
    {
        _targetModel = model;

        Time.timeScale = 0f;

        // 3개 옵션 생성
        var options = GenerateRandomOptions();
        _ui.Open(options, OnOptionSelected);
    }

    private void OnOptionSelected(LevelupOption opt)
    {
        // 강화 적용
        opt.ApplyEffect?.Invoke(_targetModel);

        // UI 닫기
        _ui.Close();
        Time.timeScale = 1f;
    }

    private List<LevelupOption> GenerateRandomOptions()
    {
        var list = new List<LevelupOption>{
            // 이동속도 증가
            new(
            "이동속도 증가",
            "플레이어의 이동속도가 1 증가합니다.",
            (model) => model.AddMoveSpeed(1f)
        ),

            // 공격력 증가
            new(
            "공격력 증가",
            "플레이어의 데미지가 1 증가합니다.",
            (model) => model.AddAttackDamage(1f)
        ),

            // 공격속도 증가
            new(
            "공격속도 증가",
            "플레이어의 공격속도가 0.5 증가합니다.",
            (model) => model.AddAttackSpeed(0.5f)
        ),

            // 공격거리 증가
            new(
            "공격 거리 증가",
            "플레이어의 공격 거리가 0.5 증가합니다.",
            (model) => model.AddAttackRange(0.5f)
        ),

            // 공격속도 증가
            new(
            "최대 체력 증가",
            "플레이어의 최대 체력이 20 증가합니다.",
            (model) => model.AddMaxHp(20f)
        )};

        // 필요 시 랜덤 섞기
        Shuffle(list);

        // 맨 앞 3개만 UI에 노출
        return list.GetRange(0, 3);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
