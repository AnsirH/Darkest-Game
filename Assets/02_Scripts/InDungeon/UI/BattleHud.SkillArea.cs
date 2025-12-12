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

        /// <summary>
        /// 스킬 아이콘 업데이트 (사용 가능 여부 포함)
        /// </summary>
        /// <param name="skills">스킬 배열</param>
        /// <param name="skillUsableFlags">각 스킬의 사용 가능 여부 (BattleSubsystem에서 검증된 데이터)</param>
        public void UpdateSkillIcon(SkillBase[] skills, bool[] skillUsableFlags)
        {
            for (int i = 0; i < skillButtons.Length; ++i)
            {
                if (i < skills.Length)
                {
                    skillButtons[i].SetSkill(skills[i]);

                    // UI는 받은 데이터를 표시만 함 (비즈니스 로직 X)
                    skillButtons[i].SetUsable(skillUsableFlags[i]);
                }
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
