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

        public void OnSelectPlayerSkill(SkillBase skill)
        {
            battleHud.HighlightSkillIcon(skill);
            battleHud.UpdateSkilInfo(skill);
        }
    }
}
