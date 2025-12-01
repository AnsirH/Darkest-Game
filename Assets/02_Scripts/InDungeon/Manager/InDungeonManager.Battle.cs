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
                    uiSubsystem.CreateHpBar(createdUnit);
            }
            
            if (mapSubsystem.CurrentLocation == CurrentLocation.Room)
                battleSubsystem.StartBattle(unitSubsystem.PlayerUnits, unitSubsystem.EnemyUnits, Vector3.zero);
            else if (mapSubsystem.CurrentLocation == CurrentLocation.Hallway)
                battleSubsystem.StartBattle(unitSubsystem.PlayerUnits, unitSubsystem.EnemyUnits, mapSubsystem.CurrentTile.Position);
        }


        public void SelectPlayerUnit(CharacterUnit playerUnit)
        {
            if (playerUnit.IsEnemyUnit) return;
            
        }
        public void SelectEnemyUnit(CharacterUnit enemyUnit)
        {
            battleSubsystem.SelectEnemy(enemyUnit);
        }

        public void SelectUnit(CharacterUnit characterUnit)
        {
            if (characterUnit.IsEnemyUnit)
            {
                // 적 유닛 클릭했을 때 처리
                // 기본 배틀 상태일 때는 적 정보 표시
                // 플레이어 공격 활성화 상태일 때는 공격을 실행
            }
            else
            {
                uiSubsystem.OnSelectPlayerUnit(characterUnit);
            }
        }

        public void SelectNone()
        {
            uiSubsystem.SelectedUnitBarController.SetActivePlayerBar(false);
        }
    }
}