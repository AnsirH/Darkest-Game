using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _02_Scripts.InDungeon.UI
{
    public partial class BattleHud
    {
        [Header("skill area")] 
        [SerializeField] private Image[] skillIcons;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI skillStatsText;

        public void UpdateSkillIcon(SkillBase[] skills)
        {
            for (int i = 0; i < skillIcons.Length; ++i)
            {
                skillIcons[i].sprite = skills[i].icon;
            }
        }

        public void UpdateSkilInfo(SkillBase skill)
        {
            descriptionText.text = skill.description;
            sb.Clear();
            sb.AppendLine($"공격: {skill.attackRatio}");
            sb.AppendLine($"명중: {skill.accuracy}");
            skillStatsText.text = sb.ToString();
        }
    }
}
