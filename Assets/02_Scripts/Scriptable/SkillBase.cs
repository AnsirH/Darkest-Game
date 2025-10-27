using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "NewSkillBase", menuName = "Create Data Asset/Create Skill Base")]
public class SkillBase : ScriptableObject
{
    public int AttackRatio;
    public int Accuracy;

    public TimelineAsset timelineAsset;
}
