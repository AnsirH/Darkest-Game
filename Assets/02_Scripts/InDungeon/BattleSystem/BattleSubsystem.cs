using DarkestLike.Character;
using DarkestLike.InDungeon.Unit;
using DarkestLike.Map;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using UnityEngine;

namespace DarkestLike.InDungeon.BattleSystem
{
    // Battle Subsystem: 배틀 시스템을 실행하고 관리하는 객체.
    // 배틀 시작
    // 배틀 프로세스
    // 배틀 종료
    public class BattleSubsystem : InDungeonSubsystem
    {
        [Header("References")]
        [SerializeField] BattleStage battleStage;

        // Variables
        // 배틀 상태
        private bool isBattleActive = false;
        // 선택 상태 관리
        private CharacterUnit selectedEnemyUnit = null;
        private Camera mainCamera;

        // Properties
        public bool IsBattleActive => isBattleActive;
        public Transform BattleCamTrf => battleStage.BattleCamTrf;
        public CharacterUnit SelectedEnemyUnit => selectedEnemyUnit;
        public List<CharacterUnit> EnemyUnits => battleStage.EnemyUnits;

        // private void Update()
        // {
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         RaycastHit hit;
        //         if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("CharacterUnit")))
        //         {
        //             SelectEnemy(hit.collider.GetComponent<Unit.CharacterUnit>());
        //         }
        //         else
        //         {
        //             ClearSelectedEnemy();
        //         }
        //     }
        // }

        public void NextTurn()
        {
            //foreach (var character in UnitSubsystem.Inst.CharacterUnits)
            //{
            //    character.UpdateTurn();
            //}
        }

        /// <summary>
        /// 배틀을 시작합니다.
        /// </summary>
        /// <param name="enemyData">적 데이터 리스트</param>
        public void StartBattle(List<CharacterUnit> playerUnits, List<CharacterData> enemyData, Vector3 stagePosition)
        {
            if (isBattleActive) 
            {
                Debug.LogWarning("[BattleSubsystem] 배틀이 이미 진행 중입니다.");
                return;
            }

            if (enemyData == null || enemyData.Count == 0)
            {
                Debug.LogWarning("[BattleSubsystem] 적 데이터가 없습니다.");
                return;
            }

            if (playerUnits == null || playerUnits.Count == 0)
            {
                Debug.LogError("[BattleSubsystem] 플레이어 유닛이 없습니다.");
                return;
            }

            // BattleStage 초기화
            if (battleStage is not null)
            {
                battleStage.InitializeBattleStage(playerUnits, enemyData, stagePosition);
                isBattleActive = true;
                Debug.Log($"[BattleSubsystem] 배틀 시작! 플레이어 {playerUnits.Count}명 vs 적 {enemyData.Count}명");
            }
            else
            {
                Debug.LogError("[BattleSubsystem] BattleStage가 설정되지 않았습니다.");
            }
        }

        /// <summary>
        /// 배틀을 종료합니다.
        /// </summary>
        public void EndBattle()
        {
            if (!isBattleActive)
            {
                Debug.LogWarning("[BattleSubsystem] 배틀이 진행 중이 아닙니다.");
                return;
            }

            // 선택 상태 초기화
            ClearSelectedEnemy();
            
            isBattleActive = false;
            Debug.Log("[BattleSubsystem] 배틀 종료");
            
            // TODO: 나중에 적 유닛 비활성화 등 추가
        }

        /// <summary>
        /// 적 유닛을 선택합니다.
        /// </summary>
        /// <param name="enemyUnit">선택할 적 유닛</param>
        public void SelectEnemy(Unit.CharacterUnit enemyUnit)
        {
            if (!isBattleActive)
            {
                Debug.LogWarning("[BattleSubsystem] 배틀이 진행 중이 아닙니다.");
                return;
            }
            if (!enemyUnit.IsEnemyUnit)
                return;
            // 기존 선택 해제
            ClearSelectedEnemy();

            // 새 유닛 선택
            selectedEnemyUnit = enemyUnit;
            if (selectedEnemyUnit != null)
            {
            }
        }

        /// <summary>
        /// 현재 선택된 적 유닛을 해제합니다.
        /// </summary>
        public void ClearSelectedEnemy()
        {
            if (selectedEnemyUnit != null)
            {
                // 선택 UI 비활성화
                selectedEnemyUnit = null;
            }
        }

        protected override void OnInitialize()
        {
            mainCamera = Camera.main;
        }
    }
}
