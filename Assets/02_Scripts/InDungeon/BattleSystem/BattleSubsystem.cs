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
        enum BattleState
        {
            Start,
            SelectUnit,
            PlayerTurn,
            Action,
            Result,
            End
        }
        [Header("References")]
        [SerializeField] Transform battleStage;
        [SerializeField] Transform[] playerPositions;
        [SerializeField] Transform[] enemyPositions;
        [SerializeField] Transform battleCamTrf;

        // Variables
        // 배틀 상태
        private readonly string characterUnitLayerName = "CharacterUnit";
        private bool isBattleActive = false;
        private BattleState battleState = BattleState.SelectUnit;
        private Queue<CharacterUnit> queuedUnits = new Queue<CharacterUnit>();
        // 선택 상태 관리
        public CharacterUnit SelectedEnemyUnit { get; private set; } = null;
        public CharacterUnit SelectedPlayerUnit { get; private set; } = null;
        public SkillBase SelectedSkill { get; private set; } = null;
        private Camera mainCamera;

        // Properties
        public bool IsBattleActive => isBattleActive;
        public Transform BattleCamTrf => battleCamTrf;
        public Transform[] EnemyPositions => enemyPositions;
        
        private void Update()
        {
            // 클릭 시 유닛을 선택하는 건지 확인
            // 적 유닛 클릭이면 적 유닛 선택
            // 플레이어 유닛 클릭이면 플레이어 유닛 선택
            if (battleState == BattleState.PlayerTurn)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    if (!Physics.Raycast(InDungeonManager.Inst.ViewCamera.ScreenPointToRay(Input.mousePosition), out hit,
                            500, LayerMask.GetMask(characterUnitLayerName)))
                        return;
                    if (!hit.collider.TryGetComponent(out CharacterUnit clickedUnit)) return;
                
                    if (!clickedUnit.IsEnemyUnit)
                    {
                        InDungeonManager.Inst.SelectPlayerUnit(clickedUnit);
                        SelectedPlayerUnit = clickedUnit;
                    }
                }
            }
        }

        public void StartBattle(List<CharacterUnit> playerUnits, List<CharacterUnit> enemyUnits, Vector3 stagePosition)
        {
            if (isBattleActive) 
            {
                Debug.LogWarning("[BattleSubsystem] 배틀이 이미 진행 중입니다.");
                return;
            }

            if (enemyUnits.Count == 0)
            {
                Debug.LogWarning("[BattleSubsystem] 적 데이터가 없습니다.");
                return;
            }

            if (playerUnits == null || playerUnits.Count == 0)
            {
                Debug.LogError("[BattleSubsystem] 플레이어 유닛이 없습니다.");
                return;
            }

            // 배틀 지점 설정
            battleStage.position = stagePosition;
            // 플레이어 유닛의 positionMaintainer의 target을 플레이어 위치로 설정
            for (int i = 0; i < playerUnits.Count; i++)
            {
                playerUnits[i].ChangePositionMaintainerTarget(playerPositions[i]);
                playerUnits[i].AnimController.ActiveIsBattle(true);
            }

            for (int i = 0; i < enemyUnits.Count; i++)
            {
                enemyUnits[i].ChangePositionMaintainerTarget(enemyPositions[i]);
                enemyUnits[i].SetPositionToTarget();
            }
        }

        private IEnumerator BattleLoop()
        {
            // 배틀 시작 연출
            yield return new WaitForSeconds(1.0f);
            List<CharacterUnit> units = new List<CharacterUnit>();
            while (true)
            {
                // 캐릭터 선택
                battleState = BattleState.SelectUnit;
                yield return new WaitForSeconds(1.0f);
                
                
            }
        }

        public void SetSelectedSkill(SkillBase skill)
        {
            SelectedSkill = skill;
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
        
        

        public void SetSelectedPlayerUnit(CharacterUnit unit)
        {
            SelectedPlayerUnit = unit;
        }

        /// <summary>
        /// 적 유닛을 선택합니다.
        /// </summary>
        /// <param name="enemyUnit">선택할 적 유닛</param>
        public void SelectEnemy(CharacterUnit enemyUnit)
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
            SelectedEnemyUnit = enemyUnit;
        }

        /// <summary>
        /// 현재 선택된 적 유닛을 해제합니다.
        /// </summary>
        public void ClearSelectedEnemy()
        {
            if (SelectedEnemyUnit != null)
            {
                // 선택 UI 비활성화
                SelectedEnemyUnit = null;
            }
        }

        protected override void OnInitialize()
        {
            mainCamera = Camera.main;
        }
    }
}
