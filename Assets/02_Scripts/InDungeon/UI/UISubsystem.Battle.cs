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

        public void OnSelectPlayerUnit(CharacterUnit characterUnit)
        {
            battleHud.SetCharacterInfo(characterUnit);
            battleHud.UpdateSkillIcon(characterUnit.CharacterData.Base.skills);

            // 선택 바 표시
            SelectedUnitBarController.SelectUnit(characterUnit.transform);

            // 첫 번째 스킬 자동 선택 (UI + BattleSubsystem 모두 업데이트)
            var firstSkill = characterUnit.CharacterData.Base.skills[0];
            InDungeonManager.Inst.SelectSkill(firstSkill);
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
