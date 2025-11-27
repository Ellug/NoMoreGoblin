using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelupOption
{
    public string Name;
    public string Description;
    public Action<PlayerModel> ApplyEffect;

    public int MaxLevel;
    public Func<PlayerModel, int> GetCurrentLevel;
    public Action<PlayerModel> IncreaseLevel;

    public LevelupOption(
        string name,
        string desc,
        Action<PlayerModel> effect,
        int maxLv,
        Func<PlayerModel, int> getter,
        Action<PlayerModel> increaser)
    {
        Name = name;
        Description = desc;
        ApplyEffect = effect;

        MaxLevel = maxLv;
        GetCurrentLevel = getter;
        IncreaseLevel = increaser;
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
        // 3개 옵션 생성
        var options = GenerateRandomOptions();
        // 남은 옵션이 없다면 레벨업을 스킵
        if (options.Count == 0)
        {
            Debug.Log("LevelupManager: No more Levelup Options");
            return;
        }

        Time.timeScale = 0f;
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
        var list = new List<LevelupOption>();
        var m = _targetModel;

        // 이동속도 옵션
        if (m.MoveSpeedLevel < 10)
        {
            list.Add(new LevelupOption(
                "이동속도 증가",
                $"플레이어 이동속도 +0.5 (현재 이동속도: {m.MoveSpeed})",
                model => {
                    model.AddMoveSpeed(0.5f);
                    model.IncMoveSpeedLevel();
                },
                10,
                model => model.MoveSpeedLevel,
                model => model.IncMoveSpeedLevel()
            ));
        }

        // 공격력 옵션
        if (m.AttackDamageLevel < 8)
        {
            list.Add(new LevelupOption(
                "공격력 증가",
                $"데미지 +1 (현재 공격력: {m.AttackDamage})",
                model => {
                    model.AddAttackDamage(1f);
                    model.IncAttackDamageLevel();
                },
                8,
                model => model.AttackDamageLevel,
                model => model.IncAttackDamageLevel()
            ));
        }

        // 공격속도 옵션
        if (m.AttackSpeedLevel < 12)
        {
            list.Add(new LevelupOption(
                "공격속도 증가",
                $"공격속도 +0.25 (현재 공격속도: {m.AttackSpeed})",
                model => {
                    model.AddAttackSpeed(0.25f);
                    model.IncAttackSpeedLevel();
                },
                12,
                model => model.AttackSpeedLevel,
                model => model.IncAttackSpeedLevel()
            ));
        }

        // 공격 범위 옵션
        if (m.AttackRangeLevel < 10)
        {
            list.Add(new LevelupOption(
                "공격범위 증가",
                $"공격 범위 +0.25 (현재 공격 범위: {m.AttackRange})",
                model => {
                    model.AddAttackRange(0.25f);
                    model.IncAttackRangeLevel();
                },
                10,
                model => model.AttackRangeLevel,
                model => model.IncAttackRangeLevel()
            ));
        }

        // 최대 체력 옵션
        if (m.AttackRangeLevel < 10)
        {
            list.Add(new LevelupOption(
                "최대 체력 증가",
                $"최대 체력 +10 (현재 최대 HP: {m.MaxHp})",
                model => {
                    model.AddMaxHp(10f);
                    model.IncMaxHPLevel();
                },
                10,
                model => model.MaxHPLevel,
                model => model.IncMaxHPLevel()
            ));
        }

        // 추가 옵션 후보들?
        // 흠...

        if (list.Count == 0)
            return list;

        Shuffle(list);

        int count = Mathf.Min(3, list.Count);
        return list.GetRange(0, count);
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
