# 배틀 시스템 구현 체크리스트

## 진행 상황

- **시작일**: 2025-12-06
- **현재 단계**: Phase 1 완료! 🎉
- **완성도**: 70% → 목표 100%
- **상태**: 핵심 전투 시스템 완성, 테스트 준비 완료

---

## Phase 1: 기본 데이터 구조 생성

### 1.1 새 파일 생성
- [x] `StatusEffect.cs` 생성 - 상태이상 시스템
- [x] `DamageCalculator.cs` 생성 - 데미지 계산 로직
- [x] `EnemyAI.cs` 생성 - 적 AI 로직
- [ ] `SkillAnimationPlayer.cs` 생성 (선택) - 애니메이션 재생
- [ ] `TorchSystem.cs` 생성 (선택) - 횃불 시스템

### 1.2 기존 파일 확장

#### CharacterData.cs
- [ ] `TakeDamage(int damage)` 메서드 추가
- [ ] `IsDead` 프로퍼티 추가
- [ ] `activeEffects` 리스트 추가
- [ ] `AddStatusEffect()` 메서드 추가
- [ ] `RemoveStatusEffect()` 메서드 추가
- [ ] `ProcessEndOfTurn()` 메서드 추가 (DOT 처리)
- [ ] `HasEffect()` 메서드 추가
- [ ] `ApplyEffectImmediate()` 메서드 추가

#### CharacterUnit.cs
- [ ] `UnitPosition` enum 추가
- [ ] `positionIndex` 필드 추가
- [ ] `Position` 프로퍼티 추가
- [ ] `SetPositionIndex()` 메서드 추가
- [ ] `IsPlayerUnit` 프로퍼티 추가
- [ ] `IsDead` 프로퍼티 추가
- [ ] `TakeDamage()` 메서드 추가
- [ ] `SetTarget()` 메서드 추가 (포지션 변경용)

#### SkillBase.cs
- [ ] `TargetType` enum 추가
- [ ] Position Requirements 필드 추가
  - [ ] `canUseFromFront`
  - [ ] `canUseFromBack`
- [ ] Targeting 필드 추가
  - [ ] `targetType`
  - [ ] `targetCount`
  - [ ] `canTargetFront`
  - [ ] `canTargetBack`
- [ ] Status Effects 필드 추가
  - [ ] `appliesStatusEffect`
  - [ ] `statusEffectType`
  - [ ] `statusEffectDuration`
  - [ ] `statusEffectValue`
  - [ ] `statusEffectChance`

---

## Phase 2: BattleSubsystem 턴 시스템 구현

### 2.1 필드 및 Enum 추가
- [ ] `BattleEndType` enum 추가 (Victory, Defeat, Fled)
- [ ] `battleEndType` 필드 추가

### 2.2 턴 계산 메서드
- [ ] `CalculateNextUnit()` 메서드 추가
- [ ] `OnRoundStart()` 메서드 추가
- [ ] `OnRoundEnd()` 메서드 추가
- [ ] `OnTurnStart()` 메서드 추가
- [ ] `OnTurnEnd()` 메서드 추가

### 2.3 BattleLoop 완성
- [ ] BattleLoop 코루틴 구현
  - [ ] 유닛 리스트 초기화
  - [ ] 라운드 시작/종료 처리
  - [ ] 턴 순서 계산
  - [ ] 플레이어/적 턴 분기
  - [ ] 승패 확인

### 2.4 플레이어 턴 처리
- [ ] `PlayerTurnCoroutine()` 구현
  - [ ] 턴 시작 이벤트 발행
  - [ ] 스킬 선택 대기
  - [ ] 타겟 선택 대기
  - [ ] 스킬 실행
  - [ ] 선택 초기화

### 2.5 적 턴 처리
- [ ] `EnemyTurnCoroutine()` 구현
  - [ ] 턴 시작 이벤트 발행
  - [ ] AI 결정 호출
  - [ ] 스킬 실행

### 2.6 Update() 수정
- [ ] 유닛 클릭 로직을 `battleState == BattleState.PlayerTurn`일 때만 작동하도록 수정

---

## Phase 3: 스킬 실행 시스템

### 3.1 스킬 실행 코루틴
- [ ] `ExecuteSkill()` 코루틴 구현
  - [ ] 포지션 검증
  - [ ] 타겟 검증
  - [ ] 애니메이션 재생
  - [ ] 데미지 계산
  - [ ] 데미지 적용
  - [ ] HP바 업데이트 이벤트
  - [ ] 데미지 숫자 표시 이벤트
  - [ ] 사망 처리
  - [ ] 상태이상 적용

### 3.2 검증 메서드
- [ ] `ValidateSkillPosition()` 구현
- [ ] `ValidateTarget()` 구현

### 3.3 사망 처리
- [ ] `HandleUnitDeath()` 코루틴 구현
  - [ ] 사망 애니메이션
  - [ ] 유닛 리스트에서 제거
  - [ ] 오브젝트 비활성화

### 3.4 애니메이션 (선택)
- [ ] `PlaySkillAnimation()` 코루틴 구현
- [ ] Timeline 재생 대기

### 3.5 포지션 시스템
- [ ] `SwapPositions()` 메서드 구현
- [ ] `PushUnit()` 메서드 구현

---

## Phase 4: 전투 종료 처리

### 4.1 승패 확인
- [ ] `CheckBattleEnd()` 메서드 구현
  - [ ] 아군 전멸 체크
  - [ ] 적 전멸 체크

### 4.2 도망 시스템
- [ ] `AttemptFlee()` 메서드 구현
  - [ ] 도망 확률 계산
  - [ ] 성공/실패 처리

### 4.3 EndBattle 개선
- [ ] `EndBattle()` 수정
  - [ ] 코루틴 중단
  - [ ] 결과별 분기
- [ ] `HandleVictory()` 구현
- [ ] `HandleDefeat()` 구현
- [ ] `HandleFlee()` 구현
- [ ] `CleanupEnemyUnits()` 구현

### 4.4 InDungeonManager 복귀
- [ ] `InDungeonManager.Battle.cs`에 `ReturnToExploration()` 추가
  - [ ] 파티 이동 해제
  - [ ] 카메라 복귀
  - [ ] 맵 UI 업데이트
  - [ ] 방 클리어 표시

---

## Phase 5: 이벤트 시스템 확장

### 5.1 DungeonEventBus 수정
- [ ] `DungeonEventType`에 전투 이벤트 추가
  - [ ] `PlayerTurnStart`
  - [ ] `EnemyTurnStart`
  - [ ] `TurnSkipped`
  - [ ] `AttackMiss`
  - [ ] `HealthChanged`
  - [ ] `DamageDealt`
  - [ ] `StatusEffectApplied`
  - [ ] `BattleVictory`
  - [ ] `BattleDefeat`
  - [ ] `FleeSuccess`
  - [ ] `FleeFailed`

---

## Phase 6: UI 업데이트 (선택)

### 6.1 HP바 실시간 업데이트
- [ ] `HpBarController.cs` 수정
  - [ ] `HealthChanged` 이벤트 구독
  - [ ] `OnHealthChanged()` 핸들러 추가

### 6.2 턴 표시 UI (선택)
- [ ] `TurnIndicatorUI.cs` 생성
  - [ ] 턴 시작 이벤트 구독
  - [ ] 턴 표시 애니메이션

### 6.3 데미지 숫자 표시 (선택)
- [ ] `DamageNumberUI.cs` 생성
  - [ ] 데미지 이벤트 구독
  - [ ] 데미지 숫자 애니메이션

### 6.4 상태이상 아이콘 (선택)
- [ ] `StatusEffectIconUI.cs` 생성
  - [ ] 상태이상 이벤트 구독
  - [ ] 아이콘 표시/제거

---

## 테스트 계획

### 단위 테스트
- [ ] 턴 순서 계산 테스트 (Speed + Random)
- [ ] 데미지 계산 테스트
- [ ] 포지션 검증 테스트
- [ ] 상태이상 적용/제거 테스트
- [ ] HP 증감 테스트
- [ ] 적 AI 결정 테스트

### 통합 테스트
- [ ] 기본 전투 (플레이어 4 vs 적 2, 전멸까지)
- [ ] 포지션 제한 테스트
- [ ] 상태이상 지속 테스트
- [ ] 도망 테스트
- [ ] 패배 테스트

---

## 주의사항

⚠️ **구현 시 주의점**:
1. BattleLoop는 코루틴 - `yield return` 필수
2. 이벤트 발행은 데이터 변경 후
3. 리스트 제거는 역순 순회
4. Null 체크 필수 (유닛이 사망했을 수 있음)
5. Timeline 재생은 비동기 처리

---

## 다음 단계

현재 작업 중: **Phase 1 - 기본 데이터 구조 생성**

다음 작업:
1. CharacterData.cs 확장
2. CharacterUnit.cs 확장
3. SkillBase.cs 확장
4. BattleSubsystem 턴 시스템 구현

---

## 파일 수정 목록

### 수정할 기존 파일
- `CharacterData.cs` - HP, 상태이상 관리
- `CharacterUnit.cs` - 포지션 정보
- `SkillBase.cs` - 스킬 확장 데이터
- `BattleSubsystem.cs` - 메인 전투 로직
- `InDungeonManager.Battle.cs` - 전투 복귀
- `DungeonEventBus.cs` - 이벤트 타입

### 생성한 새 파일
- ✅ `StatusEffect.cs` - 상태이상 시스템
- ✅ `DamageCalculator.cs` - 데미지 계산 로직
- ✅ `EnemyAI.cs` - 적 AI 로직
- ⏸️ `SkillAnimationPlayer.cs` (선택, 추후 구현)
- ⏸️ `TurnIndicatorUI.cs` (선택, 추후 구현)
- ⏸️ `DamageNumberUI.cs` (선택, 추후 구현)
- ⏸️ `StatusEffectIconUI.cs` (선택, 추후 구현)
- ⏸️ `TorchSystem.cs` (선택, 추후 구현)

### 수정한 기존 파일
- ✅ `CharacterData.cs` - TakeDamage, IsDead, 상태이상 관리 추가
- ✅ `CharacterUnit.cs` - 포지션 시스템, IsDead, IsPlayerUnit 추가
- ✅ `SkillBase.cs` - 포지션 제한, 타겟팅, 상태이상 필드 추가
- ✅ `BattleSubsystem.cs` - 완전 재구성 (500+ 라인 추가)
- ✅ `DungeonEventBus.cs` - 전투 관련 이벤트 타입 추가
