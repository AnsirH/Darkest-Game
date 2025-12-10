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
                SelectedPlayerUnit = clickedUnit;
            }
        }

        /// <summary>
        /// 배틀 중 타겟 클릭 처리 (적 또는 아군)
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
                if (clickedUnit.IsPlayerUnit && ValidateAllyTarget(clickedUnit, SelectedSkill, SelectedPlayerUnit))
                {
                    SelectAlly(clickedUnit);
                }
                else
                {
                    Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}은(는) 타겟할 수 없습니다.");
                }
            }
            // 공격 스킬 → 적 타겟팅 (기존)
            else
            {
                if (clickedUnit.IsEnemyUnit && ValidateTarget(clickedUnit, SelectedSkill))
                {
                    InDungeonManager.Inst.SelectEnemyUnit(clickedUnit);
                }
                else
                {
                    Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}은(는) 공격할 수 없습니다.");
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

            // 배틀 시작 시 선택 바 초기화 (이전 배틀 잔여물 제거)
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.SetActiveSelectedBar(false);

            // 호버 바 초기화
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearHover();
            currentHoveredUnit = null;

            // 타겟 표시 바 초기화 (적 + 아군 모두)
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableEnemies();
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableAllies();

            // 유닛 리스트 저장
            this.playerUnits = new List<CharacterUnit>(playerUnitsList);
            this.enemyUnits = new List<CharacterUnit>(enemyUnitsList);

            // 배틀 지점 설정
            battleStage.position = stagePosition;

            // 플레이어 유닛의 positionMaintainer의 target을 플레이어 위치로 설정
            for (int i = 0; i < playerUnits.Count; i++)
            {
                playerUnits[i].SetPositionIndex(i);
                playerUnits[i].ChangePositionMaintainerTarget(playerPositions[i]);
                playerUnits[i].AnimController.ActiveIsBattle(true);
            }

            // 적 유닛 배치
            for (int i = 0; i < enemyUnits.Count; i++)
            {
                enemyUnits[i].SetPositionIndex(i);
                enemyUnits[i].ChangePositionMaintainerTarget(enemyPositions[i]);
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

            // 모든 유닛 리스트 생성
            List<CharacterUnit> allUnits = new List<CharacterUnit>();
            allUnits.AddRange(playerUnits);
            allUnits.AddRange(enemyUnits);

            List<CharacterUnit> availableUnits = new List<CharacterUnit>(allUnits);

            while (isBattleActive)
            {
                // 1. 모든 유닛이 행동했으면 새 라운드 시작
                if (availableUnits.Count == 0)
                {
                    OnRoundEnd();
                    availableUnits = new List<CharacterUnit>(allUnits.Where(u => u.IsAlive));
                    OnRoundStart();

                    // 모든 유닛이 사망한 경우 체크
                    if (availableUnits.Count == 0)
                    {
                        Debug.LogError("[BattleLoop] 모든 유닛이 사망했습니다!");
                        break;
                    }
                }

                // 2. 다음 행동 유닛 계산
                CharacterUnit currentUnit = CalculateNextUnit(availableUnits);
                if (currentUnit == null || !currentUnit.IsAlive)
                {
                    Debug.LogWarning("[BattleLoop] 선택된 유닛이 null이거나 사망 상태입니다.");
                    availableUnits.Remove(currentUnit);
                    continue;
                }

                availableUnits.Remove(currentUnit);

                // 3. 턴 시작 알림 (유닛 선택 - 기절 여부와 관계없이)
                OnTurnStart(currentUnit);

                // 4. 기절 체크
                if (currentUnit.CharacterData.HasEffect(StatusEffectType.Stun))
                {
                    Debug.Log($"[BattleLoop] {currentUnit.CharacterName}이(가) 기절 상태로 행동 불가!");
                    // TODO: 턴 스킵 이벤트 발행, 기절 UI 표시
                    yield return new WaitForSeconds(1f);

                    // 기절해도 턴 종료 처리 (선택 해제)
                    OnTurnEnd(currentUnit);
                    continue;
                }

                // 5. 행동 실행 (플레이어 or AI)
                if (currentUnit.IsPlayerUnit)
                {
                    yield return StartCoroutine(PlayerTurnCoroutine(currentUnit));
                }
                else
                {
                    yield return StartCoroutine(EnemyTurnCoroutine(currentUnit));
                }

                // 6. 턴 종료 처리
                OnTurnEnd(currentUnit);

                // 7. 승패 확인
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
        /// 라운드 종료 처리 (DOT 데미지 등)
        /// </summary>
        private void OnRoundEnd()
        {
            Debug.Log("[BattleSubsystem] 라운드 종료");

            // 모든 유닛의 상태이상 처리
            List<CharacterUnit> allUnits = new List<CharacterUnit>();
            allUnits.AddRange(playerUnits);
            allUnits.AddRange(enemyUnits);

            foreach (var unit in allUnits)
            {
                if (!unit.IsAlive) continue;

                var results = unit.CharacterData.ProcessEndOfTurn();
                foreach (var result in results)
                {
                    Debug.Log($"[DOT] {unit.CharacterName}이(가) {result.effect.effectName}(으)로 {result.damageDealt} 데미지를 받았습니다.");

                    // HP바 업데이트
                    InDungeonManager.Inst.UISubsystem.UpdateHpBar(unit);
                }
            }
        }

        /// <summary>
        /// 턴 시작 알림 및 유닛 자동 선택
        /// </summary>
        private void OnTurnStart(CharacterUnit unit)
        {
            Debug.Log($"[BattleSubsystem] {unit.CharacterName}의 턴 시작");

            // 유닛 타입에 따라 자동 선택
            if (unit.IsPlayerUnit)
            {
                InDungeonManager.Inst.SelectPlayerUnit(unit);
            }
            else
            {
                InDungeonManager.Inst.SelectEnemyUnit(unit);
            }

            // TODO: 턴 시작 이벤트 발행
        }

        /// <summary>
        /// 턴 종료 처리 및 선택 해제
        /// </summary>
        private void OnTurnEnd(CharacterUnit unit)
        {
            Debug.Log($"[BattleSubsystem] {unit.CharacterName}의 턴 종료");

            // 턴 종료 시 선택 바 비활성화
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.SetActiveSelectedBar(false);

            // 타겟 표시 바 정리 (적 + 아군 모두)
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableEnemies();
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableAllies();

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
                if (IsSupportiveSkill(SelectedSkill))
                {
                    // 아군 표시
                    var targetableAllies = GetTargetableAllies(SelectedSkill, playerUnit);
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowTargetableAllies(targetableAllies);
                }
                else
                {
                    // 적 표시 (기존)
                    var targetableEnemies = GetTargetableEnemies(SelectedSkill);
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowTargetableEnemies(targetableEnemies);
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

            // 최종 타겟 결정
            CharacterUnit finalTarget = IsSupportiveSkill(SelectedSkill) ? SelectedAllyUnit : SelectedEnemyUnit;
            Debug.Log($"[PlayerTurn] 행동 선택 완료: {SelectedSkill.description} → {finalTarget.CharacterName}");

            // 스킬 실행
            yield return StartCoroutine(ExecuteSkill(playerUnit, SelectedSkill, finalTarget));

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

            // AI 결정
            AIDecision decision = EnemyAI.MakeDecision(enemyUnit, playerUnits);

            if (decision != null && decision.target != null)
            {
                Debug.Log($"[AI] {enemyUnit.CharacterName}이(가) {decision.selectedSkill.description}(을)를 {decision.target.CharacterName}에게 사용");
                yield return StartCoroutine(ExecuteSkill(enemyUnit, decision.selectedSkill, decision.target));
            }
            else
            {
                Debug.LogWarning($"[AI] {enemyUnit.CharacterName}의 AI가 결정을 내리지 못했습니다.");
            }

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
        /// 스킬 포지션 검증
        /// </summary>
        private bool ValidateSkillPosition(CharacterUnit caster, SkillBase skill)
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

            // 유닛 리스트에서 제거
            if (unit.IsPlayerUnit)
            {
                playerUnits.Remove(unit);
            }
            else
            {
                enemyUnits.Remove(unit);
            }

            // 오브젝트 비활성화
            unit.gameObject.SetActive(false);

            Debug.Log($"[Death] {unit.CharacterName} 제거 완료");
        }

        /// <summary>
        /// 전투 종료 조건 체크
        /// </summary>
        private bool CheckBattleEnd()
        {
            // 아군 전멸 체크
            bool allPlayersDead = playerUnits.TrueForAll(u => u.IsDead);
            if (allPlayersDead)
            {
                battleEndType = BattleEndType.Defeat;
                Debug.Log("[BattleEnd] 패배: 모든 아군이 사망했습니다.");
                return true;
            }

            // 적 전멸 체크
            bool allEnemiesDead = enemyUnits.TrueForAll(u => u.IsDead);
            if (allEnemiesDead)
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
                if (IsSupportiveSkill(skill))
                {
                    // 아군 타겟팅
                    var targetableAllies = GetTargetableAllies(skill, SelectedPlayerUnit);
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowTargetableAllies(targetableAllies);
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableEnemies();
                }
                else
                {
                    // 적 타겟팅 (기존)
                    var targetableEnemies = GetTargetableEnemies(skill);
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowTargetableEnemies(targetableEnemies);
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableAllies();
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
        /// 선택된 스킬로 타겟 가능한 아군 유닛 리스트 반환
        /// </summary>
        private List<CharacterUnit> GetTargetableAllies(SkillBase skill, CharacterUnit caster)
        {
            List<CharacterUnit> targetable = new List<CharacterUnit>();

            if (skill == null || playerUnits == null) return targetable;

            foreach (var ally in playerUnits)
            {
                if (ally == null || !ally.IsAlive) continue;

                // 자기 자신 타겟팅 체크
                if (ally == caster && !skill.canTargetSelf) continue;

                // 포지션 기반 타겟팅 검증
                if (ally.Position == UnitPosition.Front && skill.canTargetFront)
                    targetable.Add(ally);
                else if (ally.Position == UnitPosition.Back && skill.canTargetBack)
                    targetable.Add(ally);
            }

            return targetable;
        }

        /// <summary>
        /// 아군 타겟 검증
        /// </summary>
        private bool ValidateAllyTarget(CharacterUnit target, SkillBase skill, CharacterUnit caster)
        {
            if (!target.IsAlive) return false;
            if (!target.IsPlayerUnit) return false;

            // 자기 자신 타겟팅 체크
            if (target == caster && !skill.canTargetSelf) return false;

            // 포지션 검증
            if (target.Position == UnitPosition.Front && !skill.canTargetFront) return false;
            if (target.Position == UnitPosition.Back && !skill.canTargetBack) return false;

            return true;
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

            // 타겟 표시 바 정리 (적 + 아군 모두)
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableEnemies();
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableAllies();

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

            // 플레이어 유닛 전투 모드 해제
            foreach (var playerUnit in playerUnits)
            {
                if (playerUnit != null && playerUnit.gameObject.activeSelf)
                {
                    playerUnit.AnimController.ActiveIsBattle(false);
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

            // 플레이어 유닛 전투 모드 해제
            foreach (var playerUnit in playerUnits)
            {
                if (playerUnit != null && playerUnit.gameObject.activeSelf)
                {
                    playerUnit.AnimController.ActiveIsBattle(false);
                }
            }

            // 탐험 모드로 복귀
            InDungeonManager.Inst.PartyCtrl.ActiveFreeze(false);

            Debug.Log("[BattleEnd] 탐험 모드로 복귀 (같은 방)");
        }

        /// <summary>
        /// 적 유닛 정리
        /// </summary>
        private void CleanupEnemyUnits()
        {
            foreach (var enemyUnit in enemyUnits)
            {
                if (enemyUnit != null)
                {
                    Destroy(enemyUnit.gameObject);
                }
            }
            enemyUnits.Clear();
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
