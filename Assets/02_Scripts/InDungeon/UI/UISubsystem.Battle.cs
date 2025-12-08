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

        public void OnSelectPlayerUnit(CharacterUnit characterUnit)
        {
            battleHud.SetCharacterInfo(characterUnit);

            SelectedUnitBarController.SetActivePlayerBar(true);
            SelectedUnitBarController.SelectPlayerUnit(characterUnit.transform);
            battleHud.UpdateSkillIcon(characterUnit.CharacterData.Base.skills);
            OnSelectPlayerSkill(characterUnit.CharacterData.Base.skills[0]);
        }

        /// <summary>
        /// 적 유닛 선택 시 UI 업데이트 (정보 표시 없이 선택 바만)
        /// </summary>
        public void OnSelectEnemyUnit(CharacterUnit enemyUnit)
        {
            // 적 선택 바 활성화 및 타겟 설정
            SelectedUnitBarController.SetActiveEnemyBar(true);
            SelectedUnitBarController.SelectEnemyUnit(enemyUnit.transform);

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
