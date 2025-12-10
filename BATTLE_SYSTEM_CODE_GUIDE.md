# 배틀 시스템 코드 확인 가이드

구현한 배틀 시스템을 체계적으로 확인할 수 있도록 순서를 안내합니다.

---

## 📚 코드 확인 가이드 (추천 순서)

### **Step 1: 새로 생성한 기본 구조 파일들** (쉬움 → 어려움)

#### 1-1. StatusEffect.cs ⭐ 가장 간단
**위치**: `Assets/02_Scripts/InDungeon/BattleSystem/StatusEffect.cs`

**확인 포인트**:
- `StatusEffectType` enum (독, 출혈, 기절, 버프, 디버프)
- `StatusEffect` 클래스 - 지속시간, 수치, 이름
- `DecreaseDuration()` - 턴마다 감소
- `IsExpired` - 만료 체크

**왜 중요한가**: 상태이상 시스템의 기초 데이터 구조

---

#### 1-2. DamageCalculator.cs ⭐⭐ 중요
**위치**: `Assets/02_Scripts/InDungeon/BattleSystem/DamageCalculator.cs`

**확인 포인트**:
```csharp
public static DamageResult CalculateDamage(CharacterUnit attacker, CharacterUnit target, SkillBase skill)
```
- **명중 판정**: `skill.accuracy` vs `Random.Range(0, 100)`
- **데미지 계산**: `Attack * attackRatio - Defense`
- **랜덤 변동**: ±15%
- 반환: `DamageResult` (damage, isMiss, isCrit)

**왜 중요한가**: 모든 전투 데미지는 여기서 계산됩니다.

---

#### 1-3. EnemyAI.cs ⭐⭐ 중요
**위치**: `Assets/02_Scripts/InDungeon/BattleSystem/EnemyAI.cs`

**확인 포인트**:
```csharp
public static AIDecision MakeDecision(CharacterUnit enemy, List<CharacterUnit> playerUnits)
```
- **스킬 선택**: 사용 가능한 스킬 중 랜덤
- **타겟 선택**: HP가 가장 낮은 플레이어 우선
- `ValidateSkillPosition()` - 포지션 검증
- `ValidateTarget()` - 타겟 검증

**왜 중요한가**: 적의 행동을 결정하는 AI 로직

---

### **Step 2: 확장된 데이터 클래스들**

#### 2-1. CharacterData.cs ⭐⭐⭐ 매우 중요
**위치**: `Assets/02_Scripts/Character/CharacterData.cs`

**확인할 새 코드**:
1. **Line 5**: `using DarkestLike.InDungeon.BattleSystem;` 추가
2. **Line 15**: `private List<StatusEffect> activeEffects` 추가
3. **Line 35**: `public bool IsDead` 프로퍼티 추가
4. **Line 128-135**: `TakeDamage()` 메서드
5. **Line 137-220**: `#region Status Effects` 전체 블록
   - `AddStatusEffect()`
   - `ProcessEndOfTurn()` - 독/출혈 데미지 처리
   - `HasEffect()` - 기절 체크용

**왜 중요한가**: 캐릭터의 HP와 상태이상을 관리하는 핵심 클래스

---

#### 2-2. CharacterUnit.cs ⭐⭐
**위치**: `Assets/02_Scripts/InDungeon/CharacterUnit/CharacterUnit.cs`

**확인할 새 코드**:
1. **Line 15-22**: `UnitPosition` enum (Front/Back)
2. **Line 36**: `positionIndex` 필드
3. **Line 48-52**: 새 프로퍼티들
   - `IsDead`, `IsPlayerUnit`, `PositionIndex`, `Position`
4. **Line 151-173**: 새 메서드들
   - `TakeDamage()`
   - `SetPositionIndex()`
   - `SetTarget()`

**왜 중요한가**: 유닛의 포지션 시스템과 전투 상태 관리

---

#### 2-3. SkillBase.cs ⭐⭐
**위치**: `Assets/02_Scripts/Scriptable/SkillBase.cs`

**확인할 새 코드**:
1. **Line 5**: `using DarkestLike.InDungeon.BattleSystem;`
2. **Line 7-12**: `TargetType` enum (Single/Multi/All)
3. **Line 27-31**: Position Requirements
   - `canUseFromFront`, `canUseFromBack`
4. **Line 33-40**: Targeting
   - `targetType`, `canTargetFront`, `canTargetBack`
5. **Line 42-50**: Status Effects
   - `appliesStatusEffect`, `statusEffectType`, `statusEffectDuration`

**왜 중요한가**: 스킬의 포지션 제한과 상태이상 적용 설정

---

### **Step 3: 핵심 - BattleSubsystem.cs** ⭐⭐⭐⭐⭐ 가장 중요!

**위치**: `Assets/02_Scripts/InDungeon/BattleSystem/BattleSubsystem.cs`

**확인 순서** (총 700+ 라인, 천천히 확인):

#### 3-1. 상단: 데이터 구조 (Line 1-70)
- **Line 27-32**: `BattleEndType` enum 추가
- **Line 44-50**: 필드 추가 (battleEndType, playerUnits, enemyUnits)

#### 3-2. StartBattle() (Line 87-136)
- 유닛 리스트 저장
- 포지션 인덱스 설정
- **Line 133**: `StartCoroutine(BattleLoop())` - 여기서 전투 시작!

#### 3-3. 🎯 BattleLoop() - 가장 중요! (Line 138-212)
**전체 전투 흐름이 여기 있습니다!**
```csharp
while (isBattleActive)
{
    // 1. 라운드 체크 (Line 153-165)
    // 2. 다음 유닛 계산 (Line 168-176) - CalculateNextUnit()
    // 3. 기절 체크 (Line 178-185)
    // 4. 턴 시작 (Line 188)
    // 5. 플레이어 or 적 턴 실행 (Line 191-198)
    // 6. 턴 종료 (Line 201)
    // 7. 승패 확인 (Line 204-208)
}
```

#### 3-4. 턴 계산 메서드들 (Line 214-291)
- **Line 217-239**: `CalculateNextUnit()` - Speed + Random(1-8)
- **Line 244-248**: `OnRoundStart()`
- **Line 253-273**: `OnRoundEnd()` - DOT 데미지 처리
- **Line 278-282**: `OnTurnStart()`
- **Line 287-291**: `OnTurnEnd()`

#### 3-5. 플레이어/적 턴 (Line 293-354)
- **Line 296-326**: `PlayerTurnCoroutine()`
  - 스킬/타겟 선택 대기 (Line 308-318)
  - ExecuteSkill 호출 (Line 321)
- **Line 331-354**: `EnemyTurnCoroutine()`
  - AI 결정 (Line 341)
  - ExecuteSkill 호출 (Line 346)

#### 3-6. 🔥 ExecuteSkill() - 핵심 로직! (Line 356-429)
**스킬 실행의 모든 것:**
```csharp
Line 364-368: 포지션 검증
Line 371-375: 타겟 검증
Line 378-382: 애니메이션 (TODO)
Line 385: DamageCalculator 호출 ⬅️ 중요!
Line 387-391: Miss 처리
Line 393-426: Hit 처리
  ├─ Line 395: 데미지 적용
  ├─ Line 403-419: 상태이상 적용
  └─ Line 422-425: 사망 처리
```

#### 3-7. 검증 및 사망 처리 (Line 431-478)
- **Line 434-439**: `ValidateSkillPosition()`
- **Line 444-450**: `ValidateTarget()`
- **Line 455-478**: `HandleUnitDeath()` - 유닛 제거

#### 3-8. 승패 확인 (Line 483-504)
- **Line 483-504**: `CheckBattleEnd()`
  - 아군 전멸 → Defeat
  - 적 전멸 → Victory

#### 3-9. 전투 종료 처리 (Line 514-635)
- **Line 514-545**: `EndBattle()` - 결과별 분기
- **Line 550-575**: `HandleVictory()`
- **Line 580-592**: `HandleDefeat()`
- **Line 597-620**: `HandleFlee()`
- **Line 625-635**: `CleanupEnemyUnits()`

#### 3-10. 추가 기능 (Line 640-719)
- **Line 640-665**: `AttemptFlee()` - 도망 시도
- **Line 670-688**: `SwapPositions()` - 포지션 교환
- **Line 693-719**: `PushUnit()` - 유닛 밀기/당기기

---

### **Step 4: 이벤트 시스템**

#### 4-1. DungeonEventBus.cs
**위치**: `Assets/02_Scripts/InDungeon/Manager/DungeonEventBus.cs`

**확인 포인트** (Line 9-41):
새로 추가된 이벤트 타입들:
- 전투: `BattleVictory`, `BattleDefeat`, `FleeSuccess`, `FleeFailed`
- 턴: `PlayerTurnStart`, `EnemyTurnStart`, `TurnSkipped`
- 액션: `AttackMiss`, `HealthChanged`, `DamageDealt`, `StatusEffectApplied`
- 라운드: `RoundStart`, `RoundEnd`

---

## 🎯 **추천 확인 순서 요약**

```
1단계 (10분): 기본 구조 이해
├─ StatusEffect.cs (상태이상 데이터)
├─ DamageCalculator.cs (데미지 계산)
└─ EnemyAI.cs (적 AI)

2단계 (15분): 데이터 확장 확인
├─ CharacterData.cs (HP, 상태이상 관리)
├─ CharacterUnit.cs (포지션 시스템)
└─ SkillBase.cs (스킬 확장 데이터)

3단계 (30분): 핵심 로직 이해 ⭐ 가장 중요!
└─ BattleSubsystem.cs
    ├─ BattleLoop() - 전투 흐름
    ├─ ExecuteSkill() - 스킬 실행
    ├─ PlayerTurnCoroutine() - 플레이어 턴
    ├─ EnemyTurnCoroutine() - 적 턴
    └─ EndBattle() - 승패 처리

4단계 (5분): 이벤트 확인
└─ DungeonEventBus.cs (새 이벤트 타입)
```

---

## 💡 **코드 이해를 위한 팁**

### 1. BattleLoop부터 보세요 (Line 138)
- 전체 전투 흐름이 한눈에 보입니다
- while문 안의 7단계를 순서대로 따라가면 됩니다

### 2. ExecuteSkill을 중점적으로 (Line 356)
- 데미지 계산부터 상태이상, 사망까지 모든 것이 여기 있습니다

### 3. Console 로그를 주목
- 모든 중요 메서드에 `Debug.Log`가 있어서 실행 흐름 추적 가능

### 4. TODO 주석 확인
- 아직 구현 안 된 부분 (애니메이션, UI 이벤트 등)

---

## 📊 전투 흐름 다이어그램

```
[전투 시작]
    ↓
[BattleLoop 시작]
    ↓
┌─────────────────────┐
│  라운드 체크        │ ← 모든 유닛 행동했으면 새 라운드
│  - DOT 데미지 처리  │
└─────────────────────┘
    ↓
┌─────────────────────┐
│  턴 순서 계산       │ ← Speed + Random(1-8)
│  CalculateNextUnit()│
└─────────────────────┘
    ↓
┌─────────────────────┐
│  기절 체크          │ ← HasEffect(Stun) 체크
└─────────────────────┘
    ↓
┌─────────────────────┐
│  턴 실행            │
│  ├─ 플레이어 턴     │ ← 입력 대기
│  └─ 적 턴          │ ← AI 결정
└─────────────────────┘
    ↓
┌─────────────────────┐
│  ExecuteSkill()     │
│  ├─ 포지션 검증     │
│  ├─ 데미지 계산     │
│  ├─ 상태이상 적용   │
│  └─ 사망 처리       │
└─────────────────────┘
    ↓
┌─────────────────────┐
│  승패 확인          │
│  ├─ 적 전멸 → 승리  │
│  └─ 아군 전멸 → 패배│
└─────────────────────┘
    ↓
[전투 종료]
```

---

## 🔑 핵심 메서드 체인

### 플레이어 공격 흐름:
```
PlayerTurnCoroutine()
    ↓
[플레이어가 스킬 + 타겟 선택]
    ↓
ExecuteSkill(player, skill, enemy)
    ↓
DamageCalculator.CalculateDamage()
    ↓
enemy.TakeDamage(damage)
    ↓
enemy.CharacterData.TakeDamage() ← HP 감소
    ↓
HandleUnitDeath() (if dead)
```

### 적 공격 흐름:
```
EnemyTurnCoroutine()
    ↓
EnemyAI.MakeDecision() ← 스킬 + 타겟 자동 선택
    ↓
ExecuteSkill(enemy, skill, player)
    ↓
[위와 동일한 흐름]
```

### 상태이상 처리 흐름:
```
OnRoundEnd() (라운드 종료시)
    ↓
unit.CharacterData.ProcessEndOfTurn()
    ↓
DOT 데미지 적용 (독, 출혈)
    ↓
지속시간 감소
    ↓
만료된 효과 제거
```

---

## ✅ 체크리스트

코드를 확인하면서 다음을 체크하세요:

### 기본 구조
- [ ] StatusEffect의 5가지 타입 이해
- [ ] DamageCalculator의 계산 공식 이해
- [ ] EnemyAI의 타겟 선택 로직 이해

### 데이터 클래스
- [ ] CharacterData의 상태이상 리스트 확인
- [ ] CharacterUnit의 포지션 시스템 확인
- [ ] SkillBase의 새 필드들 확인

### 핵심 로직
- [ ] BattleLoop의 7단계 이해
- [ ] CalculateNextUnit의 턴 순서 계산 이해
- [ ] ExecuteSkill의 전체 흐름 이해
- [ ] PlayerTurnCoroutine의 입력 대기 로직 이해
- [ ] EnemyTurnCoroutine의 AI 호출 이해

### 승패 처리
- [ ] CheckBattleEnd의 조건 이해
- [ ] HandleVictory/Defeat/Flee의 차이 이해

### 추가 기능
- [ ] AttemptFlee의 확률 시스템 이해
- [ ] SwapPositions의 교환 로직 이해
- [ ] PushUnit의 밀기 로직 이해

---

## 🚀 다음 단계

코드 확인 후:
1. Unity에서 직접 테스트
2. Console 로그로 흐름 추적
3. 버그 발견시 수정
4. UI 연동 (선택사항)
5. 애니메이션 추가 (선택사항)
