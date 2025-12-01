using System;
using DarkestLike.InDungeon.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _02_Scripts.InDungeon.UI
{
    public class SkillButton : MonoBehaviour
    {
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color selectedColor;

        private SkillBase currentSkill;

        /// <summary>
        /// 현재 버튼이 표시하고 있는 스킬
        /// </summary>
        public SkillBase CurrentSkill => currentSkill;

        /// <summary>
        /// 스킬 데이터와 아이콘을 함께 설정합니다.
        /// </summary>
        /// <param name="skill">설정할 스킬</param>
        public void SetSkill(SkillBase skill)
        {
            currentSkill = skill;
            if (skill != null)
                icon.sprite = skill.icon;
        }

        /// <summary>
        /// 스킬 아이콘 프레임을 하이라이트한다.
        /// </summary>
        /// <param name="active">하이라이트 유무(true: 하이라이트, false: 취소)</param>
        public void HighlightSkillIcon(bool active) { frame.color = active? selectedColor : defaultColor; }

        public void OnClickHandler()
        {
            InDungeonManager.Inst.SelectSkill(currentSkill);
        }
    }
}
