using DarkestLike.Character;
using DarkestLike.InDungeon.BattleSystem;
using DarkestLike.Map;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Unit;
using UnityEngine;

namespace DarkestLike.InDungeon.Manager
{
    public partial class InDungeonManager
    {
        /// <summary>
        /// 배틀 시작 (예외처리 포함)
        /// </summary>
        /// <param name="enemyGroup">적 그룹</param>
        public void StartBattle(EnemyGroup enemyGroup)
        {
            partyCtrl.ActiveFreeze(true);
            cameraSubsystem.SetCameraTarget(battleSubsystem.BattleCamTrf);
            List<CharacterData> enemyDatas = enemyGroup?.Enemies;

            // 적이 없으면 경고
            if (enemyDatas == null || enemyDatas.Count == 0)
            {
                Debug.LogWarning("[InDungeonManager] 전투 타입인데 적이 없습니다. 적을 생성해야 합니다.");
                // TODO: 나중에 적 생성 로직 추가
                return;
            }

            for (int i = 0; i < enemyDatas.Count; i++)
            {
                if (unitSubsystem.AddEnemyCharacter(enemyDatas[i], battleSubsystem.EnemyPositions[i], out CharacterUnit createdUnit))
                {
                    uiSubsystem.CreateHpBar(createdUnit);
                    uiSubsystem.CreateStatusEffectBar(createdUnit);
                }
            }
            
            if (mapSubsystem.CurrentLocation == CurrentLocation.Room)
                battleSubsystem.StartBattle(unitSubsystem.PlayerUnits, unitSubsystem.EnemyUnits, Vector3.right * 2.5f);
            else if (mapSubsystem.CurrentLocation == CurrentLocation.Hallway)
                battleSubsystem.StartBattle(unitSubsystem.PlayerUnits, unitSubsystem.EnemyUnits, mapSubsystem.CurrentTile.Position);
        }


        
        public void SelectPlayerUnit(CharacterUnit playerUnit)
        {
            battleSubsystem.SetSelectedPlayerUnit(playerUnit);

            // 스킬 사용 가능 여부 판별 (BattleSubsystem에서 비즈니스 로직 처리)
            var skills = playerUnit.CharacterData.Base.skills;
            bool[] skillUsableFlags = new bool[skills.Length];
            for (int i = 0; i < skills.Length; i++)
            {
                skillUsableFlags[i] = battleSubsystem.ValidateSkillPosition(playerUnit, skills[i]);
            }

            // UI에 데이터 전달 (단방향 흐름)
            uiSubsystem.OnSelectPlayerUnit(playerUnit, skillUsableFlags);
        }
        public void SelectEnemyUnit(CharacterUnit enemyUnit)
        {
            battleSubsystem.SelectEnemy(enemyUnit);
            uiSubsystem.OnSelectEnemyUnit(enemyUnit);
        }

        public void SelectNone()
        {
            uiSubsystem.SelectedUnitBarController.SetActiveSelectedBar(false);
        }

        public void SelectSkill(SkillBase skill)
        {
            battleSubsystem.SetSelectedSkill(skill);
            uiSubsystem.OnSelectPlayerSkill(battleSubsystem.SelectedSkill);
        }
    }
}