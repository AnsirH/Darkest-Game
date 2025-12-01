using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _02_Scripts.InDungeon.UI
{
    public partial class BattleHud
    {
        [Header("skill area")]
        [SerializeField] private SkillButton[] skillButtons;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI skillStatsText;

        public void UpdateSkillIcon(SkillBase[] skills)
        {
            for (int i = 0; i < skillButtons.Length; ++i)
            {
                if (i < skills.Length)
                    skillButtons[i].SetSkill(skills[i]);
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

        public void HighlightSkillIcon(SkillBase skill)
        {
            // 모든 스킬 버튼을 순회하며 일치하는 스킬 찾기
            foreach (var button in skillButtons)
            {
                button.HighlightSkillIcon(button.CurrentSkill == skill);
            }
        }
    }
}
