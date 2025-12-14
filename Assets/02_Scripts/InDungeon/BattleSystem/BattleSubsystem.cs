using DarkestLike.Character;
using DarkestLike.InDungeon.Unit;
using DarkestLike.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public enum BattleEndType
        {
            Victory,    // 승리
            Defeat,     // 패배
            Fled        // 도망
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
        private BattleState battleState = BattleState.Start;
        private BattleEndType battleEndType;
        private Queue<CharacterUnit> queuedUnits = new Queue<CharacterUnit>();

        // 유닛 리스트
        private List<CharacterUnit> playerUnits = new List<CharacterUnit>();
        private List<CharacterUnit> enemyUnits = new List<CharacterUnit>();
        private List<CharacterUnit> allUnits = new List<CharacterUnit>(); // 전체 유닛 리스트 (배틀 중 관리)

        // 선택 상태 관리
        public CharacterUnit SelectedEnemyUnit { get; private set; } = null;
        public CharacterUnit SelectedAllyUnit { get; private set; } = null;
        public CharacterUnit SelectedPlayerUnit { get; private set; } = null;
        public SkillBase SelectedSkill { get; private set; } = null;
        private CharacterUnit currentHoveredUnit = null;
        private Camera mainCamera;

        // Properties
        public bool IsBattleActive => isBattleActive;
        public Transform BattleCamTrf => battleCamTrf;
        public Transform[] EnemyPositions => enemyPositions;
        
        private void Update()
        {
            // 배틀 전: 유닛 클릭 선택
            if (!isBattleActive)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    HandleUnitClick();
                }
            }
            // 배틀 중: 적 유닛 호버 감지 및 클릭 처리
            else
            {
                HandleUnitHover();

                // 플레이어 턴 중 타겟 클릭
                if (Input.GetMouseButtonDown(0) && battleState == BattleState.PlayerTurn)
                {
                    HandleTargetClick();
                }
            }
        }

        /// <summary>
        /// 마우스 위치의 캐릭터 유닛을 찾습니다.
        /// </summary>
        /// <param name="unit">찾은 유닛 (없으면 null)</param>
        /// <returns>유닛을 찾았는지 여부</returns>
        private bool TryGetCharacterUnitUnderMouse(out CharacterUnit unit)
        {
            unit = null;
            RaycastHit hit;

            if (!Physics.Raycast(InDungeonManager.Inst.ViewCamera.ScreenPointToRay(Input.mousePosition),
                out hit, 500, LayerMask.GetMask(characterUnitLayerName)))
            {
                return false;
            }

            return hit.collider.TryGetComponent(out unit);
        }

        /// <summary>
        /// 마우스 호버 시 적 유닛 감지 및 호버 바 표시 (적 유닛 전용)
        /// </summary>
        private void HandleUnitHover()
        {
            CharacterUnit hoveredUnit = null;

            // 마우스 아래 유닛 감지
            TryGetCharacterUnitUnderMouse(out hoveredUnit);

            // 호버 상태 변경 확인 (최적화: 변경 시에만 UI 업데이트)
            if (hoveredUnit != currentHoveredUnit)
            {
                // 이전 호버 해제
                if (currentHoveredUnit != null)
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearHover();
                }

                // 새 유닛 호버 (적 유닛만)
                if (hoveredUnit != null && hoveredUnit.IsAlive && hoveredUnit.IsEnemyUnit)
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.HoverEnemyUnit(hoveredUnit.transform);
                }

                currentHoveredUnit = hoveredUnit;
            }
        }

        /// <summary>
        /// 유닛 클릭 처리 (배틀 전 - 플레이어 유닛 선택)
        /// </summary>
        private void HandleUnitClick()
        {
            if (!TryGetCharacterUnitUnderMouse(out CharacterUnit clickedUnit))
                return;

            if (!clickedUnit.IsEnemyUnit)
            {
                InDungeonManager.Inst.SelectPlayerUnit(clickedUnit);
            }
        }

        /// <summary>
        /// 배틀 중 타겟 클릭 처리 (적 또는 아군) - Multi 타겟 지원
        /// </summary>
        private void HandleTargetClick()
        {
            if (!TryGetCharacterUnitUnderMouse(out CharacterUnit clickedUnit))
                return;

            if (!clickedUnit.IsAlive || SelectedSkill == null)
                return;

            // 지원 스킬 → 아군 타겟팅
            if (IsSupportiveSkill(SelectedSkill))
            {
                if (!clickedUnit.IsPlayerUnit)
                {
                    Debug.Log($"[BattleSubsystem] 지원 스킬은 아군만 타겟할 수 있습니다.");
                    return;
                }

                // Multi 타겟 스킬
                if (IsMultiTargetSkill(SelectedSkill))
                {
                    // 클릭한 아군의 영역 검증
                    if (!ValidateAllyTarget(clickedUnit, SelectedSkill, SelectedPlayerUnit))
                    {
                        Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}이(가) 속한 영역을 타겟할 수 없습니다.");
                        return;
                    }

                    // 영역 내 모든 아군 선택
                    List<CharacterUnit> targetedAllies = GetTargetedUnitsInArea(clickedUnit, SelectedSkill, false);

                    if (targetedAllies.Count > 0)
                    {
                        // 다중 아군 선택 (첫 번째 유닛을 대표로 설정)
                        SelectedAllyUnit = targetedAllies[0];
                        Debug.Log($"[BattleSubsystem] 아군 영역 선택: {targetedAllies.Count}명");
                    }
                }
                // Single 타겟 스킬 (기존 로직)
                else
                {
                    if (ValidateAllyTarget(clickedUnit, SelectedSkill, SelectedPlayerUnit))
                    {
                        SelectAlly(clickedUnit);
                    }
                    else
                    {
                        Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}은(는) 타겟할 수 없습니다.");
                    }
                }
            }
            // 공격 스킬 → 적 타겟팅
            else
            {
                if (!clickedUnit.IsEnemyUnit)
                {
                    Debug.Log($"[BattleSubsystem] 공격 스킬은 적만 타겟할 수 있습니다.");
                    return;
                }

                // Multi 타겟 스킬
                if (IsMultiTargetSkill(SelectedSkill))
                {
                    // 클릭한 적의 영역 검증
                    if (!ValidateTarget(clickedUnit, SelectedSkill))
                    {
                        Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}이(가) 속한 영역을 공격할 수 없습니다.");
                        return;
                    }

                    // 영역 내 모든 적 선택
                    List<CharacterUnit> targetedEnemies = GetTargetedUnitsInArea(clickedUnit, SelectedSkill, true);

                    if (targetedEnemies.Count > 0)
                    {
                        // 다중 적 선택 (첫 번째 유닛을 대표로 설정)
                        InDungeonManager.Inst.SelectEnemyUnit(targetedEnemies[0]);
                        Debug.Log($"[BattleSubsystem] 적 영역 선택: {targetedEnemies.Count}명");
                    }
                }
                // Single 타겟 스킬 (기존 로직)
                else
                {
                    if (ValidateTarget(clickedUnit, SelectedSkill))
                    {
                        InDungeonManager.Inst.SelectEnemyUnit(clickedUnit);
                    }
                    else
                    {
                        Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}은(는) 공격할 수 없습니다.");
                    }
                }
            }
        }

        public void StartBattle(List<CharacterUnit> playerUnitsList, List<CharacterUnit> enemyUnitsList, Vector3 stagePosition)
        {
            if (isBattleActive)
            {
                Debug.LogWarning("[BattleSubsystem] 배틀이 이미 진행 중입니다.");
                return;
            }

            if (enemyUnitsList == null || enemyUnitsList.Count == 0)
            {
                Debug.LogWarning("[BattleSubsystem] 적 데이터가 없습니다.");
                return;
            }

            if (playerUnitsList == null || playerUnitsList.Count == 0)
            {
                Debug.LogError("[BattleSubsystem] 플레이어 유닛이 없습니다.");
                return;
            }

            // 적 유닛 수가 배치 가능한 위치보다 많으면 오류
            if (enemyUnitsList.Count > enemyPositions.Length)
            {
                Debug.LogError($"[BattleSubsystem] 적 유닛 수({enemyUnitsList.Count})가 배치 가능한 위치({enemyPositions.Length})보다 많습니다!");
                return;
            }

            // 배틀 시작 시 선택 바 초기화 (이전 배틀 잔여물 제거)
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.SetActiveSelectedBar(false);

            // 호버 바 초기화
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearHover();
            currentHoveredUnit = null;

            // 타겟 표시 바 초기화 (통합 Clear)
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableUnits();

            // 유닛 리스트 저장
            this.playerUnits = new List<CharacterUnit>(playerUnitsList);
            this.enemyUnits = new List<CharacterUnit>(enemyUnitsList);

            // 배틀 지점 설정
            battleStage.position = stagePosition;
            InDungeonManager.Inst.PartyCtrl.transform.position = battleStage.position - Vector3.right * 2.5f;

            // 플레이어 유닛의 positionMaintainer의 target을 플레이어 위치로 설정
            for (int i = 0; i < playerUnits.Count; i++)
            {
                playerUnits[i].SetPositionIndex(i);
                playerUnits[i].ChangePositionMaintainerTarget(playerPositions[i], 0.2f);
                playerUnits[i].AnimController.ActiveIsBattle(true);
            }

            // 적 유닛 배치
            for (int i = 0; i < enemyUnits.Count; i++)
            {
                enemyUnits[i].SetPositionIndex(i);
                enemyUnits[i].SetMoveTime(0.2f);
                enemyUnits[i].ChangePositionMaintainerTarget(enemyPositions[i], 0.2f);
                enemyUnits[i].SetPositionToTarget();
            }

            // 배틀 시작
            isBattleActive = true;
            battleState = BattleState.Start;
            StartCoroutine(BattleLoop());

            Debug.Log($"[BattleSubsystem] 배틀 시작: 플레이어 {playerUnits.Count}명 vs 적 {enemyUnits.Count}명");
        }

        private IEnumerator BattleLoop()
        {
            // 배틀 시작 연출
            yield return new WaitForSeconds(1.0f);

            // 모든 유닛 리스트 초기화
            allUnits.Clear();
            allUnits.AddRange(playerUnits);
            allUnits.AddRange(enemyUnits);

            List<CharacterUnit> availableUnits = new List<CharacterUnit>(allUnits);

            while (isBattleActive)
            {
                // 1. 모든 유닛이 행동했으면 새 라운드 시작
                if (availableUnits.Count == 0)
                {
                    OnRoundEnd();
                    availableUnits = new List<CharacterUnit>(allUnits); // allUnits는 이미 사망한 유닛이 제거됨
                    OnRoundStart();

                    // 모든 유닛이 사망한 경우 체크 (이론상 CheckBattleEnd에서 먼저 감지되어야 함)
                    if (availableUnits.Count == 0)
                    {
                        Debug.LogError("[BattleLoop] 모든 유닛이 사망했습니다!");
                        break;
                    }
                }

                // 2. 다음 행동 유닛 계산
                CharacterUnit currentUnit = CalculateNextUnit(availableUnits);
                if (currentUnit == null)
                {
                    // 살아있는 유닛이 없으면 availableUnits에서 죽은 유닛들 정리
                    Debug.LogWarning("[BattleLoop] CalculateNextUnit이 null을 반환했습니다. availableUnits 정리 중...");
                    availableUnits.RemoveAll(u => u == null || !u.IsAlive);

                    // 정리 후에도 유닛이 없으면 라운드 종료
                    if (availableUnits.Count == 0)
                    {
                        Debug.Log("[BattleLoop] 모든 유닛이 행동 완료, 라운드 종료");
                    }
                    continue;
                }

                if (!currentUnit.IsAlive)
                {
                    Debug.LogWarning("[BattleLoop] 선택된 유닛이 사망 상태입니다.");
                    availableUnits.Remove(currentUnit);
                    continue;
                }

                availableUnits.Remove(currentUnit);

                // 3. 유닛 선택 (턴 시작 전 선택 UI 표시)
                if (currentUnit.IsPlayerUnit)
                {
                    InDungeonManager.Inst.SelectPlayerUnit(currentUnit);
                }
                else
                {
                    InDungeonManager.Inst.SelectEnemyUnit(currentUnit);
                }

                Debug.Log($"[BattleSubsystem] {currentUnit.CharacterName}의 턴 시작");

                // 4. 기절 체크 (ProcessStartOfTurn 호출 전에 체크)
                bool isStunned = currentUnit.CharacterData.HasEffect(StatusEffectType.Stun);

                // 5. DOT 효과 처리 (턴 시작 시 - 기절 여부와 무관하게 처리)
                if (currentUnit.IsAlive)
                {
                    var results = currentUnit.CharacterData.ProcessStartOfTurn();
                    foreach (var result in results)
                    {
                        Debug.Log($"[DOT] {currentUnit.CharacterName}이(가) {result.effect.effectName}(으)로 {result.damageDealt} 데미지를 받았습니다.");
                        InDungeonManager.Inst.UISubsystem.UpdateHpBar(currentUnit);
                    }

                    // DOT로 사망 체크
                    if (currentUnit.IsDead)
                    {
                        yield return StartCoroutine(HandleUnitDeath(currentUnit));

                        // 승패 확인
                        if (CheckBattleEnd())
                        {
                            EndBattle();
                            break;
                        }

                        continue;
                    }
                }

                // 6. 기절 상태면 행동 스킵
                if (isStunned)
                {
                    Debug.Log($"[BattleLoop] {currentUnit.CharacterName}이(가) 기절 상태로 행동 불가!");
                    yield return new WaitForSeconds(1f);

                    // 턴 종료 처리
                    OnTurnEnd(currentUnit);
                    continue;
                }

                // 7. 행동 실행 (플레이어 or AI)
                if (currentUnit.IsPlayerUnit)
                {
                    yield return StartCoroutine(PlayerTurnCoroutine(currentUnit));
                }
                else
                {
                    yield return StartCoroutine(EnemyTurnCoroutine(currentUnit));
                }

                // 8. 턴 종료 처리
                OnTurnEnd(currentUnit);

                // 9. 승패 확인
                if (CheckBattleEnd())
                {
                    EndBattle();
                    break;
                }

                yield return new WaitForSeconds(0.3f);
            }
        }

        /// <summary>
        /// 다음 행동 유닛을 계산합니다 (Speed + Random(1-8))
        /// </summary>
        private CharacterUnit CalculateNextUnit(List<CharacterUnit> availableUnits)
        {
            if (availableUnits == null || availableUnits.Count == 0)
                return null;

            CharacterUnit selectedUnit = null;
            int highestValue = -1;

            foreach (var unit in availableUnits)
            {
                if (!unit.IsAlive) continue;

                int speedValue = unit.CharacterData.Speed + Random.Range(1, 9);

                if (speedValue > highestValue)
                {
                    highestValue = speedValue;
                    selectedUnit = unit;
                }
            }

            return selectedUnit;
        }

        /// <summary>
        /// 라운드 시작 처리
        /// </summary>
        private void OnRoundStart()
        {
            Debug.Log("[BattleSubsystem] 새 라운드 시작");
            // TODO: 라운드 시작 이벤트 발행
        }

        /// <summary>
        /// 라운드 종료 처리
        /// </summary>
        private void OnRoundEnd()
        {
            Debug.Log("[BattleSubsystem] 라운드 종료");
            // TODO: 라운드 종료 이벤트 발행
        }


        /// <summary>
        /// 턴 종료 처리 및 선택 해제
        /// </summary>
        private void OnTurnEnd(CharacterUnit unit)
        {
            Debug.Log($"[BattleSubsystem] {unit.CharacterName}의 턴 종료");

            // 턴 종료 시 선택 바 비활성화
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.SetActiveSelectedBar(false);

            // 타겟 표시 바 정리 (통합 Clear)
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableUnits();

            unit.UpdateTurn();
        }

        /// <summary>
        /// 플레이어 턴 처리
        /// </summary>
        private IEnumerator PlayerTurnCoroutine(CharacterUnit playerUnit)
        {
            battleState = BattleState.PlayerTurn;

            Debug.Log($"[PlayerTurn] {playerUnit.CharacterName}의 턴");
            // TODO: 턴 시작 UI 표시 이벤트

            // 선택 초기화 (스킬은 OnTurnStart에서 이미 선택됨)
            ClearSelectedEnemy();
            ClearSelectedAlly();

            // 타겟 가능한 유닛 표시 (스킬이 이미 선택되어 있으므로)
            if (SelectedSkill != null)
            {
                bool isSupportive = IsSupportiveSkill(SelectedSkill);
                var targetableUnits = isSupportive
                    ? GetTargetableAllies(SelectedSkill, playerUnit)
                    : GetTargetableEnemies(SelectedSkill);

                if (IsMultiTargetSkill(SelectedSkill))
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowConnectionIndicators(
                        targetableUnits,
                        SelectedSkill.canTargetFront,
                        SelectedSkill.canTargetBack,
                        isSupportive);
                }
                else
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowTargetableUnits(
                        targetableUnits,
                        isSupportive);
                }
            }

            // 플레이어 입력 대기
            bool actionSelected = false;
            while (!actionSelected)
            {
                // 지원 스킬: 아군 선택 대기
                if (IsSupportiveSkill(SelectedSkill))
                {
                    if (SelectedSkill != null && SelectedAllyUnit != null)
                        actionSelected = true;
                }
                // 공격 스킬: 적 선택 대기
                else
                {
                    if (SelectedSkill != null && SelectedEnemyUnit != null)
                        actionSelected = true;
                }
                yield return null;
            }

            // Multi 타겟 스킬 실행
            if (IsMultiTargetSkill(SelectedSkill))
            {
                // 영역 내 모든 유닛 가져오기
                bool isEnemyTarget = !IsSupportiveSkill(SelectedSkill);
                CharacterUnit clickedUnit = isEnemyTarget ? SelectedEnemyUnit : SelectedAllyUnit;
                List<CharacterUnit> targets = GetTargetedUnitsInArea(clickedUnit, SelectedSkill, isEnemyTarget);

                Debug.Log($"[PlayerTurn] Multi 스킬 실행: {SelectedSkill.description} → {targets.Count}명");

                yield return StartCoroutine(ExecuteSkillOnMultipleTargets(playerUnit, SelectedSkill, targets));
            }
            // Single 타겟 스킬 실행
            else
            {
                CharacterUnit finalTarget = IsSupportiveSkill(SelectedSkill) ? SelectedAllyUnit : SelectedEnemyUnit;
                Debug.Log($"[PlayerTurn] Single 스킬 실행: {SelectedSkill.description} → {finalTarget.CharacterName}");

                yield return StartCoroutine(ExecuteSkill(playerUnit, SelectedSkill, finalTarget));
            }

            // 선택 초기화
            SelectedSkill = null;
            ClearSelectedEnemy();
            ClearSelectedAlly();
        }

        /// <summary>
        /// 적 턴 처리
        /// </summary>
        private IEnumerator EnemyTurnCoroutine(CharacterUnit enemyUnit)
        {
            battleState = BattleState.Action;

            Debug.Log($"[EnemyTurn] {enemyUnit.CharacterName}의 턴");
            // TODO: 적 턴 시작 이벤트

            yield return new WaitForSeconds(0.5f);

            // AI 결정 (공격 스킬은 playerUnits, 버프/힐 스킬은 enemyUnits를 타겟으로)
            AIDecision decision = EnemyAI.MakeDecision(enemyUnit, playerUnits, enemyUnits);

            // 결정을 못 내린 경우: 치명적인 버그
            if (decision == null || decision.target == null)
            {
                Debug.LogError($"[CRITICAL BUG] {enemyUnit.CharacterName}의 AI가 결정을 내리지 못했습니다!");
                Debug.LogError($"[BattleState] 적 유닛 수: {enemyUnits.Count}, 플레이어 유닛 수: {playerUnits.Count}");
                Debug.LogError($"[BattleState] {enemyUnit.CharacterName} 위치: {enemyUnit.Position}, 스킬 수: {enemyUnit.CharacterData.Base.skills.Length}");

                // 이 경우는 발생하면 안 되는 버그입니다
                // 게임 디자인상 적은 항상 행동할 수 있어야 합니다
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            bool isSupportive = IsSupportiveSkill(decision.selectedSkill);

            // Multi 타겟 스킬
            if (IsMultiTargetSkill(decision.selectedSkill))
            {
                // AI가 선택한 타겟이 속한 영역의 모든 유닛 가져오기
                // 적의 관점: 공격 스킬이면 플레이어(enemyUnits=false), 회복이면 적(enemyUnits=true)
                bool isEnemyTarget = isSupportive;
                List<CharacterUnit> targets = GetTargetedUnitsInArea(decision.target, decision.selectedSkill, isEnemyTarget);

                // 목표 유닛 표시 (플레이어 턴과 동일)
                InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowConnectionIndicators(
                    targets,
                    decision.selectedSkill.canTargetFront,
                    decision.selectedSkill.canTargetBack,
                    isSupportive);

                yield return new WaitForSeconds(0.5f); // 표시 후 잠시 대기

                Debug.Log($"[AI] {enemyUnit.CharacterName}이(가) Multi 스킬 {decision.selectedSkill.description}(을)를 {targets.Count}명에게 사용");
                yield return StartCoroutine(ExecuteSkillOnMultipleTargets(enemyUnit, decision.selectedSkill, targets));
            }
            // Single 타겟 스킬
            else
            {
                // 목표 유닛 표시 (플레이어 턴과 동일)
                List<CharacterUnit> targetList = new List<CharacterUnit> { decision.target };
                InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowTargetableUnits(
                    targetList,
                    isSupportive);

                yield return new WaitForSeconds(0.5f); // 표시 후 잠시 대기

                Debug.Log($"[AI] {enemyUnit.CharacterName}이(가) {decision.selectedSkill.description}(을)를 {decision.target.CharacterName}에게 사용");
                yield return StartCoroutine(ExecuteSkill(enemyUnit, decision.selectedSkill, decision.target));
            }

            // 목표 표시 제거
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableUnits();

            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 스킬 실행
        /// </summary>
        private IEnumerator ExecuteSkill(CharacterUnit caster, SkillBase skill, CharacterUnit target)
        {
            battleState = BattleState.Action;

            // 1. 포지션 검증
            if (!ValidateSkillPosition(caster, skill))
            {
                Debug.LogWarning($"[ExecuteSkill] {caster.CharacterName}의 포지션에서 {skill.description}를 사용할 수 없습니다.");
                yield break;
            }

            // 2. 타겟 검증 (지원 스킬이면 아군 검증)
            bool isValidTarget = IsSupportiveSkill(skill)
                ? ValidateAllyTarget(target, skill, caster)
                : ValidateTarget(target, skill);

            if (!isValidTarget)
            {
                Debug.LogWarning($"[ExecuteSkill] {target.CharacterName}은(는) 유효한 타겟이 아닙니다.");
                yield break;
            }

            // 3. 애니메이션 재생 (TODO: Timeline 통합)
            if (skill.timelineAsset != null)
            {
                // yield return StartCoroutine(PlaySkillAnimation(caster, skill));
                Debug.Log($"[Skill] 애니메이션 재생: {skill.description}");
            }

            // 4. 지원 스킬 처리
            if (IsSupportiveSkill(skill))
            {
                // 4a. 힐링 처리 (attackRatio 사용)
                if (skill.isHealing)
                {
                    // attackRatio를 사용하여 회복량 계산 (데미지와 동일한 방식)
                    int healAmount = (int)(caster.CharacterData.Attack * (skill.attackRatio / 100f));
                    int healedAmount = target.CharacterData.Heal(healAmount);

                    Debug.Log($"[Heal] {caster.CharacterName}이(가) {target.CharacterName}을(를) {healedAmount} 회복!");
                    Debug.Log($"[Heal] {target.CharacterName} HP: {target.CharacterData.CurrentHealth}/{target.CharacterData.MaxHealth}");

                    // HP바 업데이트
                    InDungeonManager.Inst.UISubsystem.UpdateHpBar(target);
                    // TODO: 회복 숫자 표시 이벤트 (초록색)
                }

                // 4b. 버프 적용
                if (skill.appliesStatusEffect && skill.statusEffectType == StatusEffectType.Buff)
                {
                    int roll = Random.Range(0, 100);
                    if (roll < skill.statusEffectChance)
                    {
                        var effect = new StatusEffect(
                            skill.statusEffectType,
                            skill.statusEffectDuration,
                            skill.statusEffectValue,
                            skill.description
                        );
                        target.CharacterData.AddStatusEffect(effect);

                        Debug.Log($"[Buff] {target.CharacterName}에게 {effect.type} 효과 적용!");
                        // TODO: 버프 적용 이벤트
                    }
                }

                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // 5. 공격 스킬 처리 (기존 로직)
            DamageResult result = DamageCalculator.CalculateDamage(caster, target, skill);

            if (result.isMiss)
            {
                Debug.Log($"[Combat] {caster.CharacterName}의 공격이 빗나갔습니다!");
                // TODO: Miss UI 표시 이벤트
            }
            else
            {
                // 데미지 적용
                target.TakeDamage(result.damage);

                Debug.Log($"[Combat] {caster.CharacterName}이(가) {target.CharacterName}에게 {result.damage} 데미지!");
                Debug.Log($"[Combat] {target.CharacterName} HP: {target.CharacterData.CurrentHealth}/{target.CharacterData.MaxHealth}");

                // HP바 업데이트
                InDungeonManager.Inst.UISubsystem.UpdateHpBar(target);

                // TODO: 데미지 숫자 표시 이벤트

                // 상태이상 적용
                if (skill.appliesStatusEffect)
                {
                    int roll = Random.Range(0, 100);
                    if (roll < skill.statusEffectChance)
                    {
                        var effect = new StatusEffect(
                            skill.statusEffectType,
                            skill.statusEffectDuration,
                            skill.statusEffectValue,
                            skill.description
                        );
                        target.CharacterData.AddStatusEffect(effect);

                        Debug.Log($"[StatusEffect] {target.CharacterName}에게 {effect.type} 효과 적용!");
                        // TODO: 상태이상 적용 이벤트
                    }
                }

                // 사망 체크
                if (target.IsDead)
                {
                    yield return StartCoroutine(HandleUnitDeath(target));
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 다중 타겟에게 스킬 실행 (Multi 타겟용)
        /// </summary>
        private IEnumerator ExecuteSkillOnMultipleTargets(CharacterUnit caster, SkillBase skill, List<CharacterUnit> targets)
        {
            battleState = BattleState.Action;

            // 1. 포지션 검증
            if (!ValidateSkillPosition(caster, skill))
            {
                Debug.LogWarning($"[ExecuteSkill] {caster.CharacterName}의 포지션에서 {skill.description}를 사용할 수 없습니다.");
                yield break;
            }

            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning($"[ExecuteSkillOnMultipleTargets] 타겟이 없습니다.");
                yield break;
            }

            Debug.Log($"[ExecuteSkillOnMultipleTargets] {caster.CharacterName}이(가) {targets.Count}명에게 {skill.description} 사용");

            // 2. 애니메이션 재생
            if (skill.timelineAsset != null)
            {
                // yield return StartCoroutine(PlaySkillAnimation(caster, skill));
                Debug.Log($"[Skill] 애니메이션 재생: {skill.description}");
            }

            // 3. 지원 스킬 처리 (회복/버프)
            if (IsSupportiveSkill(skill))
            {
                foreach (var target in targets)
                {
                    if (target == null || !target.IsAlive) continue;

                    // 힐링 처리
                    if (skill.isHealing)
                    {
                        int healAmount = (int)(caster.CharacterData.Attack * (skill.attackRatio / 100f));
                        int healedAmount = target.CharacterData.Heal(healAmount);

                        Debug.Log($"[Heal] {caster.CharacterName}이(가) {target.CharacterName}을(를) {healedAmount} 회복!");
                        Debug.Log($"[Heal] {target.CharacterName} HP: {target.CharacterData.CurrentHealth}/{target.CharacterData.MaxHealth}");

                        // HP바 업데이트 (동시 적용)
                        InDungeonManager.Inst.UISubsystem.UpdateHpBar(target);
                    }

                    // 버프 적용
                    if (skill.appliesStatusEffect && skill.statusEffectType == StatusEffectType.Buff)
                    {
                        int roll = Random.Range(0, 100);
                        if (roll < skill.statusEffectChance)
                        {
                            var effect = new StatusEffect(
                                skill.statusEffectType,
                                skill.statusEffectDuration,
                                skill.statusEffectValue,
                                skill.description
                            );
                            target.CharacterData.AddStatusEffect(effect);

                            Debug.Log($"[Buff] {target.CharacterName}에게 {effect.type} 효과 적용!");
                        }
                    }
                }

                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // 4. 공격 스킬 처리 (모든 타겟에 동시 적용)
            List<CharacterUnit> deadUnits = new List<CharacterUnit>();

            foreach (var target in targets)
            {
                if (target == null || !target.IsAlive) continue;

                // 데미지 계산 및 적용
                DamageResult result = DamageCalculator.CalculateDamage(caster, target, skill);

                if (result.isMiss)
                {
                    Debug.Log($"[Combat] {caster.CharacterName}의 {target.CharacterName}에 대한 공격이 빗나갔습니다!");
                }
                else
                {
                    // 데미지 적용
                    target.TakeDamage(result.damage);

                    Debug.Log($"[Combat] {caster.CharacterName}이(가) {target.CharacterName}에게 {result.damage} 데미지!");
                    Debug.Log($"[Combat] {target.CharacterName} HP: {target.CharacterData.CurrentHealth}/{target.CharacterData.MaxHealth}");

                    // HP바 업데이트 (동시 적용)
                    InDungeonManager.Inst.UISubsystem.UpdateHpBar(target);

                    // 상태이상 적용
                    if (skill.appliesStatusEffect)
                    {
                        int roll = Random.Range(0, 100);
                        if (roll < skill.statusEffectChance)
                        {
                            var effect = new StatusEffect(
                                skill.statusEffectType,
                                skill.statusEffectDuration,
                                skill.statusEffectValue,
                                skill.description
                            );
                            target.CharacterData.AddStatusEffect(effect);

                            Debug.Log($"[StatusEffect] {target.CharacterName}에게 {effect.type} 효과 적용!");
                        }
                    }

                    // 사망 체크 (나중에 일괄 처리)
                    if (target.IsDead && !deadUnits.Contains(target))
                    {
                        deadUnits.Add(target);
                    }
                }
            }

            // 5. 사망 처리 (일괄)
            foreach (var deadUnit in deadUnits)
            {
                yield return StartCoroutine(HandleUnitDeath(deadUnit));
            }

            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 스킬 포지션 검증 (UI에서도 접근 가능하도록 public)
        /// </summary>
        public bool ValidateSkillPosition(CharacterUnit caster, SkillBase skill)
        {
            if (caster.Position == UnitPosition.Front && !skill.canUseFromFront) return false;
            if (caster.Position == UnitPosition.Back && !skill.canUseFromBack) return false;
            return true;
        }

        /// <summary>
        /// 타겟 검증
        /// </summary>
        private bool ValidateTarget(CharacterUnit target, SkillBase skill)
        {
            if (!target.IsAlive) return false;
            if (target.Position == UnitPosition.Front && !skill.canTargetFront) return false;
            if (target.Position == UnitPosition.Back && !skill.canTargetBack) return false;
            return true;
        }

        /// <summary>
        /// 유닛 사망 처리
        /// </summary>
        private IEnumerator HandleUnitDeath(CharacterUnit unit)
        {
            Debug.Log($"[Death] {unit.CharacterName}이(가) 사망했습니다.");

            // 사망한 유닛의 선택 바 즉시 비활성화
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.SetActiveSelectedBar(false);

            // TODO: 사망 애니메이션
            // unit.AnimController.PlayDeath();

            yield return new WaitForSeconds(1f);

            // 죽은 유닛의 위치 정보 저장 (리스트에서 제거하기 전에)
            int deadUnitIndex = unit.PositionIndex;
            bool isPlayer = unit.IsPlayerUnit;

            // 유닛 리스트에서 제거
            if (isPlayer)
            {
                playerUnits.Remove(unit);
            }
            else
            {
                enemyUnits.Remove(unit);
            }

            // 전체 유닛 리스트에서도 제거
            allUnits.Remove(unit);

            // HP바 및 상태 이상 바 제거
            InDungeonManager.Inst.UISubsystem.RemoveHpBar(unit);
            InDungeonManager.Inst.UISubsystem.RemoveStatusEffectBar(unit);
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableUnits();

            // 오브젝트 비활성화
            unit.gameObject.SetActive(false);

            // 뒤에 있는 유닛들을 앞으로 당기기
            PullUnitsForward(deadUnitIndex, isPlayer);

            Debug.Log($"[Death] {unit.CharacterName} 제거 완료, 뒤 유닛들 재배치 완료");
        }

        /// <summary>
        /// 유닛 사망 시 뒤에 있는 유닛들을 앞으로 당깁니다.
        /// </summary>
        /// <param name="deadUnitIndex">죽은 유닛의 위치 인덱스</param>
        /// <param name="isPlayerUnit">플레이어 유닛 여부</param>
        private void PullUnitsForward(int deadUnitIndex, bool isPlayerUnit)
        {
            // 해당 진영의 유닛 리스트와 위치 배열 선택
            List<CharacterUnit> units = isPlayerUnit ? playerUnits : enemyUnits;
            Transform[] positions = isPlayerUnit ? playerPositions : enemyPositions;

            // 죽은 유닛보다 뒤에 있는 유닛들을 찾아서 앞으로 당기기
            foreach (var unit in units)
            {
                if (unit.PositionIndex > deadUnitIndex)
                {
                    int oldIndex = unit.PositionIndex;
                    int newIndex = oldIndex - 1;
                    unit.SetPositionIndex(newIndex);
                    unit.SetTarget(positions[newIndex]); // 배틀 시작 시 설정한 속도 유지

                    Debug.Log($"[Death] {unit.CharacterName} 위치 이동: {oldIndex} → {newIndex}");
                }
            }
        }

        /// <summary>
        /// 전투 종료 조건 체크
        /// </summary>
        private bool CheckBattleEnd()
        {
            // 아군 전멸 체크 (사망한 유닛은 리스트에서 제거되므로 Count == 0 체크)
            if (playerUnits.Count == 0)
            {
                battleEndType = BattleEndType.Defeat;
                Debug.Log("[BattleEnd] 패배: 모든 아군이 사망했습니다.");
                return true;
            }

            // 적 전멸 체크 (사망한 유닛은 리스트에서 제거되므로 Count == 0 체크)
            if (enemyUnits.Count == 0)
            {
                battleEndType = BattleEndType.Victory;
                Debug.Log("[BattleEnd] 승리: 모든 적을 처치했습니다!");
                return true;
            }

            return false;
        }

        public void SetSelectedSkill(SkillBase skill)
        {
            SelectedSkill = skill;

            // 스킬 선택 시 타겟 가능한 유닛 표시
            if (skill != null && isBattleActive && battleState == BattleState.PlayerTurn)
            {
                bool isSupportive = IsSupportiveSkill(skill);
                var targetableUnits = isSupportive
                    ? GetTargetableAllies(skill, SelectedPlayerUnit)
                    : GetTargetableEnemies(skill);

                if (IsMultiTargetSkill(skill))
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowConnectionIndicators(
                        targetableUnits,
                        skill.canTargetFront,
                        skill.canTargetBack,
                        isSupportive);
                }
                else
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowTargetableUnits(
                        targetableUnits,
                        isSupportive);
                }
            }
        }

        /// <summary>
        /// 선택된 스킬로 공격 가능한 적 유닛 리스트를 반환합니다.
        /// </summary>
        private List<CharacterUnit> GetTargetableEnemies(SkillBase skill)
        {
            List<CharacterUnit> targetable = new List<CharacterUnit>();

            if (skill == null || enemyUnits == null) return targetable;

            foreach (var enemy in enemyUnits)
            {
                if (enemy == null || !enemy.IsAlive) continue;

                // 포지션 기반 타겟팅 검증
                if (enemy.Position == UnitPosition.Front && skill.canTargetFront)
                {
                    targetable.Add(enemy);
                }
                else if (enemy.Position == UnitPosition.Back && skill.canTargetBack)
                {
                    targetable.Add(enemy);
                }
            }

            return targetable;
        }

        /// <summary>
        /// 스킬이 아군 대상 지원 스킬인지 확인
        /// </summary>
        private bool IsSupportiveSkill(SkillBase skill)
        {
            if (skill == null) return false;
            return skill.isHealing ||
                   (skill.appliesStatusEffect && skill.statusEffectType == StatusEffectType.Buff);
        }

        /// <summary>
        /// 스킬이 Multi 타겟 스킬인지 확인
        /// </summary>
        private bool IsMultiTargetSkill(SkillBase skill)
        {
            if (skill == null) return false;
            return skill.targetType == TargetType.Multi;
        }

        /// <summary>
        /// 클릭한 유닛의 영역에 속한 모든 타겟 유닛 반환
        /// </summary>
        /// <param name="clickedUnit">클릭한 유닛</param>
        /// <param name="skill">선택된 스킬</param>
        /// <param name="isEnemyTarget">적 타겟팅 여부 (true: 적, false: 아군)</param>
        /// <returns>영역에 속한 모든 유닛 리스트</returns>
        private List<CharacterUnit> GetTargetedUnitsInArea(CharacterUnit clickedUnit, SkillBase skill, bool isEnemyTarget)
        {
            List<CharacterUnit> result = new List<CharacterUnit>();

            if (clickedUnit == null || skill == null) return result;

            // 타겟 유닛 리스트 선택
            List<CharacterUnit> targetList = isEnemyTarget ? enemyUnits : playerUnits;

            // 전열 + 후열 모두 타겟 가능 → 전체 공격
            if (skill.canTargetFront && skill.canTargetBack)
            {
                foreach (var unit in targetList)
                {
                    if (unit != null && unit.IsAlive)
                        result.Add(unit);
                }
            }
            // 전열만 타겟 가능 → 클릭한 유닛이 전열이면 전열 전체
            else if (skill.canTargetFront && !skill.canTargetBack)
            {
                if (clickedUnit.Position == UnitPosition.Front)
                {
                    foreach (var unit in targetList)
                    {
                        if (unit != null && unit.IsAlive && unit.Position == UnitPosition.Front)
                            result.Add(unit);
                    }
                }
            }
            // 후열만 타겟 가능 → 클릭한 유닛이 후열이면 후열 전체
            else if (!skill.canTargetFront && skill.canTargetBack)
            {
                if (clickedUnit.Position == UnitPosition.Back)
                {
                    foreach (var unit in targetList)
                    {
                        if (unit != null && unit.IsAlive && unit.Position == UnitPosition.Back)
                            result.Add(unit);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 선택된 스킬로 타겟 가능한 아군 유닛 리스트 반환
        /// </summary>
        private List<CharacterUnit> GetTargetableAllies(SkillBase skill, CharacterUnit caster)
        {
            List<CharacterUnit> targetable = new List<CharacterUnit>();

            if (!skill || playerUnits == null) return targetable;

            foreach (var ally in playerUnits)
            {
                if (!ally || !ally.IsAlive) continue;

                // 자기 자신 타겟팅 - canTargetSelf가 true면 포지션 무관하게 추가
                if (ally == caster)
                {
                    if (skill.canTargetSelf)
                        targetable.Add(ally);
                    continue;
                }

                // 다른 아군 타겟팅 - 포지션 기반 검증
                if (ally.Position == UnitPosition.Front && skill.canTargetFront)
                    targetable.Add(ally);
                else if (ally.Position == UnitPosition.Back && skill.canTargetBack)
                    targetable.Add(ally);
            }

            return targetable;
        }

        /// <summary>
        /// 아군 타겟 검증 (같은 진영인지 확인)
        /// </summary>
        private bool ValidateAllyTarget(CharacterUnit target, SkillBase skill, CharacterUnit caster)
        {
            if (!target.IsAlive) return false;

            // 같은 진영인지 확인 (플레이어끼리 또는 적끼리)
            bool isSameFaction = (target.IsPlayerUnit && caster.IsPlayerUnit) ||
                                 (target.IsEnemyUnit && caster.IsEnemyUnit);

            if (!isSameFaction) return false;

            // 자기 자신 타겟팅 - canTargetSelf가 true면 포지션 무관하게 허용
            if (target == caster)
                return skill.canTargetSelf;

            // 다른 아군 타겟팅 - 포지션 검증
            if (target.Position == UnitPosition.Front && !skill.canTargetFront) return false;
            if (target.Position == UnitPosition.Back && !skill.canTargetBack) return false;

            return true;
        }

        /// <summary>
        /// 현재 턴 플레이어 유닛 설정
        /// </summary>
        public void SetSelectedPlayerUnit(CharacterUnit playerUnit)
        {
            SelectedPlayerUnit = playerUnit;
        }

        /// <summary>
        /// 아군 유닛 선택
        /// </summary>
        public void SelectAlly(CharacterUnit allyUnit)
        {
            if (!isBattleActive || !allyUnit.IsPlayerUnit) return;

            ClearSelectedAlly();
            SelectedAllyUnit = allyUnit;
            Debug.Log($"[BattleSubsystem] 아군 선택: {allyUnit.CharacterName}");
        }

        /// <summary>
        /// 선택된 아군 초기화
        /// </summary>
        public void ClearSelectedAlly()
        {
            if (SelectedAllyUnit != null)
                SelectedAllyUnit = null;
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

            isBattleActive = false;
            battleState = BattleState.End;

            StopAllCoroutines(); // BattleLoop 중단

            // 선택 상태 초기화
            ClearSelectedEnemy();
            ClearSelectedAlly();
            SelectedSkill = null;
            SelectedPlayerUnit = null;

            // UI 바 비활성화
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.SetActiveSelectedBar(false);

            // 호버 바도 비활성화
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearHover();
            currentHoveredUnit = null;

            // 타겟 표시 바 정리 (통합 Clear)
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableUnits();

            // allUnits 리스트 정리 (배틀 종료 시 초기화)
            allUnits.Clear();

            // 전투 결과에 따른 처리
            switch (battleEndType)
            {
                case BattleEndType.Victory:
                    StartCoroutine(HandleVictory());
                    break;
                case BattleEndType.Defeat:
                    StartCoroutine(HandleDefeat());
                    break;
                case BattleEndType.Fled:
                    StartCoroutine(HandleFlee());
                    break;
            }
        }

        /// <summary>
        /// 승리 처리
        /// </summary>
        private IEnumerator HandleVictory()
        {
            Debug.Log("[BattleEnd] === 승리 ===");
            // TODO: 승리 UI 표시 이벤트

            yield return new WaitForSeconds(2f);

            // TODO: 보상 시스템 (골드, 아이템 등)

            // 적 유닛 정리
            CleanupEnemyUnits();

            // 플레이어 유닛 전투 모드 해제 및 이동 속도 원래대로 복구
            for (int i = 0; i < playerUnits.Count; i++)
            {
                var playerUnit = playerUnits[i];
                if (playerUnit != null && playerUnit.gameObject.activeSelf)
                {
                    playerUnit.AnimController.ActiveIsBattle(false);
                    playerUnit.SetMoveTime(0.5f); // 원래 속도로 복구

                    // PositionMaintainer 타겟을 PartyCtrl의 위치로 복구
                    playerUnit.ChangePositionMaintainerTarget(
                        InDungeonManager.Inst.PartyCtrl.PositionTargets[playerUnit.PositionIndex]);
                }
            }

            if (InDungeonManager.Inst.CurrentLocation == CurrentLocation.Hallway)
                InDungeonManager.Inst.CameraSubsystem.SetToPartyTarget(false);
            else
                InDungeonManager.Inst.CameraSubsystem.SetToRoomTarget(false);

            // 배틀 클리어한 방/타일 데이터 초기화
            ClearBattleLocationData();

            // 방에서 배틀이 끝났을 때 인접 방 하이라이트
            if (InDungeonManager.Inst.CurrentLocation == CurrentLocation.Room)
            {
                var currentRoom = InDungeonManager.Inst.CurrentRoom;
                if (currentRoom != null)
                {
                    InDungeonManager.Inst.UISubsystem.MapDrawer.HighlightNearRooms(currentRoom);
                }
            }

            // 탐험 모드로 복귀
            InDungeonManager.Inst.PartyCtrl.ActiveFreeze(false);

            Debug.Log("[BattleEnd] 탐험 모드로 복귀");
        }

        /// <summary>
        /// 패배 처리
        /// </summary>
        private IEnumerator HandleDefeat()
        {
            Debug.Log("[BattleEnd] === 패배 ===");
            // TODO: 패배 UI 표시 이벤트

            yield return new WaitForSeconds(2f);

            // TODO: 패배 처리 (게임오버, 마을로 복귀 등)
            Debug.LogWarning("[BattleEnd] 패배 처리 미구현 - 게임오버 또는 마을로 복귀 필요");

            // 적 유닛 정리
            CleanupEnemyUnits();
        }

        /// <summary>
        /// 도망 성공 처리
        /// </summary>
        private IEnumerator HandleFlee()
        {
            Debug.Log("[BattleEnd] === 도망 성공 ===");
            // TODO: 도망 성공 메시지 이벤트

            yield return new WaitForSeconds(1f);

            // 적 유닛 정리
            CleanupEnemyUnits();

            // 플레이어 유닛 전투 모드 해제 및 이동 속도 원래대로 복구
            for (int i = 0; i < playerUnits.Count; i++)
            {
                var playerUnit = playerUnits[i];
                if (playerUnit != null && playerUnit.gameObject.activeSelf)
                {
                    playerUnit.AnimController.ActiveIsBattle(false);
                    playerUnit.SetMoveTime(0.5f); // 원래 속도로 복구

                    // PositionMaintainer 타겟을 PartyCtrl의 위치로 복구
                    playerUnit.ChangePositionMaintainerTarget(
                        InDungeonManager.Inst.PartyCtrl.PositionTargets[playerUnit.PositionIndex]);
                }
            }

            // 카메라를 Room 타겟으로 복구
            InDungeonManager.Inst.CameraSubsystem.SetToPartyTarget();

            // 배틀 클리어한 방/타일 데이터 초기화
            ClearBattleLocationData();

            // 방에서 배틀이 끝났을 때 인접 방 하이라이트
            if (InDungeonManager.Inst.CurrentLocation == CurrentLocation.Room)
            {
                var currentRoom = InDungeonManager.Inst.CurrentRoom;
                if (currentRoom != null)
                {
                    InDungeonManager.Inst.UISubsystem.MapDrawer.HighlightNearRooms(currentRoom);
                }
            }

            // 탐험 모드로 복귀
            InDungeonManager.Inst.PartyCtrl.ActiveFreeze(false);

            Debug.Log("[BattleEnd] 탐험 모드로 복귀 (같은 방)");
        }

        /// <summary>
        /// 적 유닛 정리 (GameObject, UI, Dictionary 모두 정리)
        /// </summary>
        private void CleanupEnemyUnits()
        {
            foreach (var enemyUnit in enemyUnits)
            {
                if (enemyUnit != null)
                {
                    // HP 바 및 상태 이상 바 제거
                    InDungeonManager.Inst.UISubsystem.RemoveHpBar(enemyUnit);
                    InDungeonManager.Inst.UISubsystem.RemoveStatusEffectBar(enemyUnit);

                    // GameObject 제거
                    Destroy(enemyUnit.gameObject);
                }
            }
            enemyUnits.Clear();

            // UnitSubsystem의 적 캐릭터 Dictionary도 정리
            InDungeonManager.Inst.UnitSubsystem.ClearEnemyCharacters();

            Debug.Log("[BattleEnd] 적 유닛 정리 완료");
        }

        /// <summary>
        /// 배틀 클리어한 방/타일의 데이터 초기화 및 맵 UI 업데이트
        /// </summary>
        private void ClearBattleLocationData()
        {
            var currentLocation = InDungeonManager.Inst.CurrentLocation;

            if (currentLocation == CurrentLocation.Room)
            {
                // 방 데이터 초기화
                var currentRoom = InDungeonManager.Inst.CurrentRoom;
                if (currentRoom != null)
                {
                    currentRoom.ClearBattleData();
                    Debug.Log($"[BattleEnd] Room 데이터 초기화 완료: {currentRoom.Position}");

                    // 맵 UI 업데이트 (빈 방으로 표시)
                    InDungeonManager.Inst.UISubsystem.MapDrawer.UpdateRoomUI(currentRoom);
                }
            }
            else if (currentLocation == CurrentLocation.Hallway)
            {
                // 타일 데이터 초기화
                var currentTile = InDungeonManager.Inst.CurrentTile;
                if (currentTile != null)
                {
                    currentTile.ClearBattleData();
                    Debug.Log($"[BattleEnd] Tile 데이터 초기화 완료: {currentTile.Position}");

                    // 맵 UI 업데이트 (빈 타일로 표시)
                    InDungeonManager.Inst.UISubsystem.MapDrawer.UpdateTileUI(currentTile);
                }
            }
        }

        /// <summary>
        /// 도망 시도
        /// </summary>
        public void AttemptFlee()
        {
            if (!isBattleActive)
            {
                Debug.LogWarning("[Flee] 배틀이 진행 중이 아닙니다.");
                return;
            }

            // 도망 성공 확률 계산 (50% 기본)
            int fleeChance = 50;
            int roll = Random.Range(0, 100);

            if (roll < fleeChance)
            {
                // 도망 성공
                Debug.Log($"[Flee] 도망 성공! (roll: {roll}, chance: {fleeChance})");
                battleEndType = BattleEndType.Fled;
                EndBattle();
            }
            else
            {
                // 도망 실패
                Debug.Log($"[Flee] 도망 실패! (roll: {roll}, chance: {fleeChance})");
                // TODO: 도망 실패 이벤트
            }
        }

        /// <summary>
        /// 두 유닛의 포지션을 교환합니다.
        /// </summary>
        public void SwapPositions(CharacterUnit unit1, CharacterUnit unit2)
        {
            if (unit1.IsPlayerUnit != unit2.IsPlayerUnit)
            {
                Debug.LogWarning("[Swap] 플레이어와 적의 포지션은 교환할 수 없습니다.");
                return;
            }

            int tempIndex = unit1.PositionIndex;
            unit1.SetPositionIndex(unit2.PositionIndex);
            unit2.SetPositionIndex(tempIndex);

            // 물리적 위치 이동
            Transform[] positions = unit1.IsPlayerUnit ? playerPositions : enemyPositions;
            unit1.SetTarget(positions[unit1.PositionIndex]);
            unit2.SetTarget(positions[unit2.PositionIndex]);

            Debug.Log($"[Swap] {unit1.CharacterName}과(와) {unit2.CharacterName}의 포지션 교환");
        }

        /// <summary>
        /// 유닛을 밀거나 당깁니다.
        /// </summary>
        public void PushUnit(CharacterUnit unit, int direction)
        {
            int newIndex = unit.PositionIndex + direction;
            if (newIndex < 0 || newIndex >= 4)
            {
                Debug.LogWarning($"[Push] {unit.CharacterName}을(를) 범위 밖으로 밀 수 없습니다.");
                return;
            }

            // 해당 위치의 유닛과 교환
            var units = unit.IsPlayerUnit ? playerUnits : enemyUnits;
            var targetUnit = units.Find(u => u.PositionIndex == newIndex);

            if (targetUnit != null)
            {
                SwapPositions(unit, targetUnit);
            }
            else
            {
                // 빈 칸으로 이동
                unit.SetPositionIndex(newIndex);
                Transform[] positions = unit.IsPlayerUnit ? playerPositions : enemyPositions;
                unit.SetTarget(positions[newIndex]);

                Debug.Log($"[Push] {unit.CharacterName}을(를) 포지션 {newIndex}(으)로 이동");
            }
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
