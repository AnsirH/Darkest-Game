using DarkestLike.Character;
using DarkestLike.InDungeon.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon.BattleSystem
{
    public class BattleStage : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] CharacterUnit characterUnitPrefab;
        [SerializeField] Transform[] playerPositions;
        [SerializeField] Transform[] enemyPositions;
        [SerializeField] Transform battleCamTrf;

        // Variables
        List<CharacterUnit> enemyUnits = new();
        // Properties
        public Transform BattleCamTrf => battleCamTrf;
        
        // 플레이어 유닛 리스트와 적 데이터를 받아서 받아서 배틀 스테이지 초기화
        public void InitializeBattleStage(List<Unit.CharacterUnit> playerUnits, List<CharacterData> enemyData, Vector3 stagePosition)
        {
            transform.position = stagePosition;
            // 플레이어 유닛의 positionMaintainer의 target을 플레이어 위치로 설정
            for (int i = 0; i < playerUnits.Count; i++)
            {
                playerUnits[i].ChangePositionMaintainerTarget(playerPositions[i], 0.1f);
                playerUnits[i].AnimController.ActiveIsBattle(true);
            }

            if (enemyUnits.Count > 0)
            {
                while (enemyUnits.Count > 0)
                {
                    CharacterUnit removedUnit = enemyUnits[^1];
                    enemyUnits.Remove(removedUnit);
                    Destroy(removedUnit.gameObject);
                }
            }
            
            // 적의 정보를 기반으로 적 유닛 초기화 및 활성화
            for (int i = 0; i < enemyData.Count; i++)
            {
                CharacterUnit unit = Instantiate(characterUnitPrefab);
                unit.Initialize(enemyData[i], enemyPositions[i]);
                enemyUnits.Add(unit);
            }
        }
    }
}