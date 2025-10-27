using DarkestLike.Character;
using DarkestLike.InDungeon.CharacterUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon.BattleSystem
{
    public class BattleStage : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform[] playerPositions;
        [SerializeField] Transform[] enemyPositions;
        [SerializeField] CharacterUnit.CharacterUnit[] enemyUnits;
        [SerializeField] Transform battleCamTrf;

        // Properties
        public Transform BattleCamTrf => battleCamTrf;
        
        // 플레이어 유닛 리스트와 적 데이터를 받아서 받아서 배틀 스테이지 초기화
        public void InitializeBattleStage(List<CharacterUnit.CharacterUnit> playerUnits, List<CharacterData> enemyData, Vector3 stagePosition)
        {
            transform.position = stagePosition;
            // 플레이어 유닛의 positionMaintainer의 target을 플레이어 위치로 설정
            for (int i = 0; i < playerUnits.Count; i++)
            {
                playerUnits[i].ChangePositionMaintainerTarget(playerPositions[i], 0.1f);
                playerUnits[i].AnimController.ActiveIsBattle(true);
            }
            
            // 적의 정보를 기반으로 적 유닛 초기화 및 활성화
            for (int i = 0; i < enemyData.Count; i++)
            {
                enemyUnits[i].Initialize(enemyData[i]);
                enemyUnits[i].gameObject.SetActive(true);
            }
        }
    }
}