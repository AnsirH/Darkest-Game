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
        [SerializeField] private Color disabledColor = Color.gray;

        private SkillBase currentSkill;
        private bool isUsable = true;

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
        /// 스킬 사용 가능 여부 설정 (위치 검증)
        /// </summary>
        /// <param name="usable">true: 사용 가능, false: 사용 불가(회색 표시)</param>
        public void SetUsable(bool usable)
        {
            isUsable = usable;
            if (!isUsable)
            {
                frame.color = disabledColor;
                icon.color = disabledColor;
            }
            else
            {
                frame.color = defaultColor;
                icon.color = Color.white;
            }
        }

        /// <summary>
        /// 스킬 아이콘 프레임을 하이라이트한다.
        /// </summary>
        /// <param name="active">하이라이트 유무(true: 하이라이트, false: 취소)</param>
        public void HighlightSkillIcon(bool active)
        {
            // 비활성화된 스킬은 하이라이트 불가
            if (!isUsable)
            {
                frame.color = disabledColor;
                return;
            }

            frame.color = active ? selectedColor : defaultColor;
        }

        public void OnClickHandler()
        {
            // 사용 불가능한 스킬은 선택 차단
            if (!isUsable)
            {
                Debug.LogWarning($"[SkillButton] {currentSkill.description}은(는) 현재 위치에서 사용할 수 없습니다.");
                return;
            }

            InDungeonManager.Inst.SelectSkill(currentSkill);
        }
    }
}
