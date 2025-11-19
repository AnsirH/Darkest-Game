using DarkestLike.Character;
using DarkestLike.InDungeon.BattleSystem;
using DarkestLike.Map;
using System.Collections;
using System.Collections.Generic;
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
            List<CharacterData> enemyData = enemyGroup?.Enemies;

            // 적이 없으면 경고
            if (enemyData == null || enemyData.Count == 0)
            {
                Debug.LogWarning("[InDungeonManager] 전투 타입인데 적이 없습니다. 적을 생성해야 합니다.");
                // TODO: 나중에 적 생성 로직 추가
                return;
            }
            if (mapSubsystem.CurrentLocation == CurrentLocation.Room)
                battleSubsystem.StartBattle(partyCtrl.CharacterUnits, enemyData, Vector3.zero);
            else if (mapSubsystem.CurrentLocation == CurrentLocation.Hallway)
                battleSubsystem.StartBattle(partyCtrl.CharacterUnits, enemyData, mapSubsystem.CurrentTile.Position);
        }

        public void SelectEnemyUnit(Unit.CharacterUnit enemyUnit)
        {
            battleSubsystem.SelectEnemy(enemyUnit);
        }
    }
}