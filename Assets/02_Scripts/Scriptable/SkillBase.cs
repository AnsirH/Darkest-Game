using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "NewSkillBase", menuName = "Create Data Asset/Create Skill Base")]
public class SkillBase : ScriptableObject
{
    [TextArea]
    public string description;
    public Sprite icon;
    public int attackRatio;
    public int accuracy;

    public TimelineAsset timelineAsset;
}
