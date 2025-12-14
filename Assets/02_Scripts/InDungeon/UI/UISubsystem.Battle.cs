using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Unit;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public partial class UISubsystem
    {
        [SerializeField] BattleHud battleHud;
        [SerializeField] HpBarController hpBarController;
        [SerializeField] SelectedUnitBarController selectedUnitBarController;
        [SerializeField] StatusEffectBarController statusEffectBarController;

        public SelectedUnitBarController SelectedUnitBarController => selectedUnitBarController;

        public void CreateHpBar(CharacterUnit unit)
        {
            hpBarController.CreateHpBar(unit);
        }

        /// <summary>
        /// 유닛의 HP 바를 업데이트합니다.
        /// </summary>
        public void UpdateHpBar(CharacterUnit unit)
        {
            hpBarController.UpdateHpBarValue(unit);
        }

        /// <summary>
        /// 유닛의 HP 바를 제거합니다.
        /// </summary>
        public void RemoveHpBar(CharacterUnit unit)
        {
            hpBarController.RemoveHpBar(unit);
        }

        /// <summary>
        /// 유닛의 상태 이상 바를 생성합니다.
        /// </summary>
        public void CreateStatusEffectBar(CharacterUnit unit)
        {
            statusEffectBarController.CreateStatusEffectBar(unit);
        }

        /// <summary>
        /// 유닛의 상태 이상 바를 제거합니다.
        /// </summary>
        public void RemoveStatusEffectBar(CharacterUnit unit)
        {
            statusEffectBarController.RemoveStatusEffectBar(unit);
        }

        /// <summary>
        /// 유닛의 상태 이상 아이콘을 즉시 갱신합니다.
        /// </summary>
        public void UpdateStatusEffectIcons(CharacterUnit unit)
        {
            statusEffectBarController.UpdateStatusEffectIcons(unit);
        }

        public void OnSelectPlayerUnit(CharacterUnit characterUnit, bool[] skillUsableFlags)
        {
            battleHud.SetCharacterInfo(characterUnit);
            battleHud.UpdateSkillIcon(characterUnit.CharacterData.Base.skills, skillUsableFlags);

            // 선택 바 표시
            SelectedUnitBarController.SelectUnit(characterUnit.transform);

            // 첫 번째 사용 가능한 스킬 자동 선택
            var skills = characterUnit.CharacterData.Base.skills;
            for (int i = 0; i < skills.Length; i++)
            {
                if (skillUsableFlags[i])
                {
                    InDungeonManager.Inst.SelectSkill(skills[i]);
                    break;
                }
            }
        }

        /// <summary>
        /// 적 유닛 선택 시 UI 업데이트 (정보 표시 없이 선택 바만)
        /// </summary>
        public void OnSelectEnemyUnit(CharacterUnit enemyUnit)
        {
            // 선택 바 표시 (플레이어와 동일한 바 사용)
            SelectedUnitBarController.SelectUnit(enemyUnit.transform);

            // 사용자 요구사항: 적 유닛은 정보 표시 안 함
            // battleHud.SetCharacterInfo() 호출하지 않음
            // battleHud.UpdateSkillIcon() 호출하지 않음
        }

        public void OnSelectPlayerSkill(SkillBase skill)
        {
            battleHud.HighlightSkillIcon(skill);
            battleHud.UpdateSkilInfo(skill);
        }
    }
}
