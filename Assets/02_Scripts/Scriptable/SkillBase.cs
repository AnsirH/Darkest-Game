using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using DarkestLike.InDungeon.BattleSystem;

public enum TargetType
{
    Single,     // 단일 타겟
    Multi,      // 다중 타겟
    All         // 전체 타겟
}

[CreateAssetMenu(fileName = "NewSkillBase", menuName = "Create Data Asset/Create Skill Base")]
public class SkillBase : ScriptableObject
{
    [Header("Basic Info")]
    [TextArea]
    public string description;
    public Sprite icon;
    public int attackRatio = 100;
    public int accuracy = 85;

    [Header("Animation")]
    public TimelineAsset timelineAsset;

    [Header("Position Requirements")]
    [Tooltip("전열(1-2번 위치)에서 사용 가능 여부")]
    public bool canUseFromFront = true;
    [Tooltip("후열(3-4번 위치)에서 사용 가능 여부")]
    public bool canUseFromBack = true;

    [Header("Targeting")]
    public TargetType targetType = TargetType.Single;
    [Tooltip("Multi 타입일 때 타겟 수")]
    public int targetCount = 1;
    [Tooltip("전열 타겟 가능 여부")]
    public bool canTargetFront = true;
    [Tooltip("후열 타겟 가능 여부")]
    public bool canTargetBack = true;

    [Header("Status Effects")]
    public bool appliesStatusEffect = false;
    public StatusEffectType statusEffectType = StatusEffectType.Poison;
    [Range(1, 10)]
    public int statusEffectDuration = 3;
    public int statusEffectValue = 5;
    [Range(0, 100)]
    [Tooltip("상태이상 적용 확률 (%)")]
    public int statusEffectChance = 100;
}
