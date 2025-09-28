# 전투 시스템 요구사항 명세서 (Battle System SRS)

아래 명세서는 **현재 PRD(1주 프로토타입 기준)**의 전투 시스템을 자세히 기술한 요구사항입니다. 구현팀(혹은 본인)이 바로 개발/테스트할 수 있도록 기능적 요구사항, 비기능적 요구사항, 데이터 모델, 상태/시퀀스 흐름, API(인터페이스), 테스트 항목 및 완료 조건까지 포함합니다. 가능한 한 ScriptableObject / 컴포넌트 기반으로 구성해 확장성과 빠른 프로토타이핑에 유리하도록 설계했습니다.

---

## 1. 목적 및 범위

**목적**: 던전 탐험 중 발생하는 턴제 전투를 3D 환경에서 재현하여 플레이어가 전투 → 횃불(밝기) → 스트레스(간단 연동)의 상호작용을 체험할 수 있게 한다.

**범위**: 1주 프로토타입에서는 핵심 루프(턴제 전투, 위치 기반 스킬, 상태이상, 전투와 횃불의 연동)를 최소 기능 수준으로 구현한다. 고도화(애니메이션 완성, 복잡한 AI)는 우선순위 낮음.

---

## 2. 용어 정의

- **Actor**: 전투에 참여하는 엔티티(아군 캐릭터 또는 적). `Character` 클래스 인스턴스.
- **Turn**: 한 Actor가 행동을 수행하는 시간 슬롯.
- **Initiative**: 턴 순서를 정하는 값(스피드 기반, 랜덤 보정 가능).
- **Skill**: 액션 단위. ScriptableObject `SkillData`로 정의.
- **Position**: 파티 내 슬롯(예: Front1, Front2, Back1, Back2). 위치 기반 스킬/타겟 규칙에 영향.
- **StatusEffect**: 지속 효과(출혈, 기절 등). Duration(턴 수) 가짐.
- **BattleState**: 전투 상태머신(Starting, AwaitingInput, Resolving, Ended 등).

---

## 3. 핵심 기능적 요구사항 (Functional Requirements)

각 항목에는 식별자(FR-xx)를 붙였습니다.

### FR-01 전투 개시

- **설명**: 플레이어가 전투 트리거(방 진입 등)를 발생시키면 `BattleManager.StartBattle(allies, enemies, roomContext)` 호출로 전투 시작.
- **입력/출력**: 입력 = 아군 리스트, 적군 리스트, 룸 메타; 출력 = 전투 시작 이벤트 `OnBattleStarted`.
- **수락기준**: 호출 후 전투 씬(또는 전투 UI)이 초기화되고 턴 루프가 준비됨.

### FR-02 턴 순서 계산

- **설명**: 각 Actor의 `CharacterStats.speed` 와 랜덤 보정값(테스트용 seed 가능)으로 초기 턴 순서(Queue)를 계산.
- **파라미터**: `initiativeJitterRange` (ex: ±0~10). `deterministicSeed` 옵션.
- **수락기준**: 같은 시드이면 같은 턴 순서가 재현되어야 함.

### FR-03 턴 처리 루프

- **설명**: 턴 루프는 다음 단계로 진행:
    1. `OnTurnStart(actor)`
    2. 상태효과(턴 시작형) 처리(예: 출혈 피해)
    3. (플레이어) 입력 대기 또는 (AI) 행동 선택
    4. 행동 실행(스킬 캐스팅 → 타겟 판정 → 데미지/효과 적용)
    5. 애니메이션/이펙트 완료 대기(콜백)
    6. 상태효과(턴 종료형) 갱신(지속시간 감소/제거)
    7. `OnTurnEnd(actor)` → 다음 턴
- **수락기준**: 모든 단계가 순서대로 실행되고 비동기 애니메이션 동기화가 지원됨.

### FR-04 스킬 시스템

- **설명**: 스킬은 `SkillData` ScriptableObject로 정의. 최소 필드:
    - id, name, description
    - targetType (SingleEnemy, AllEnemies, SingleAlly, Self, FrontRow, BackRow 등)
    - positionRequirement (ex: only from front row)
    - baseDamage / damageType (Physical/Magic)
    - accuracy, criticalChance, cooldownTurns, cost(액션 포인트, 후순위)
    - effectList (StatusEffect 적용 목록)
- **수락기준**: 스킬 사용 시 쿨다운이 적용되고 위치/타겟 규칙을 위반하면 사용 불가능 메시지 표시.

### FR-05 위치 기반 전투 (Positioning)

- **설명**: 파티는 슬롯 배열(예: index 0..3)으로 관리. 앞줄(0,1), 뒷줄(2,3).
- **규칙**: 앞줄에 있는 적만 특정 스킬/아이템이 타겟 가능 등.
- **수락기준**: 위치에 따라 스킬의 유효 타겟이 달라짐을 확인할 수 있어야 함.

### FR-06 상태이상/버프/디버프

- **설명**: `StatusEffect`는 id, name, duration(턴), stackable(bool), tickTiming(OnTurnStart/End) 등을 가짐.
- **예시**: Bleed(턴 시작마다 피해), Stun(몇 턴 동안 행동 불가), Guard(방어 증가).
- **수락기준**: 상태 적용·갱신·제거가 정확히 동작하고 UI에 아이콘/카운트가 표시됨.

### FR-07 데미지/명중/크리티컬 공식 (조정 가능한 파라미터)

- **권장식** (조정 가능):
    - 히트 판정: `roll = Random(0,100)` → `roll <= attacker.accuracy - target.evasion` → 히트
    - 크리티컬: `critRoll <= attacker.critChance` → 피해 * critMultiplier
    - 최종데미지: `final = max(1, floor((attacker.attack + skill.baseDamage) - target.defense * defenseScale))`
- **수락기준**: 파라미터는 `IngameData` SO로 노출되어 튜닝 가능.

### FR-08 전투-횃불 연동

- **설명**: 전투 시작 시 현재 횃불 밝기에 따라 다음 보정 적용:
    - High torch: 아군 정확도 +X, 적 피해 -Y
    - Low torch: 적 공격력 +Z, 전리품 기대치 증가(전투 외 메커닉)
- **수락기준**: 횃불 값 변경 시 전투 중에도 즉시 보정 적용되어 전투 결과에 영향.

### FR-09 AI(적) 행동

- **설명**: 단순 규칙 기반 AI:
    - 우선순위: 치명적 위협 제거(최저 HP) → 자기치유(HP 낮음) → 강력한 스킬(쿨다운 가능) → 랜덤 공격
    - 타겟 선정은 `ThreatScore` 혹은 속성 기반(가장 앞줄/가장 HP 낮음 등)
- **수락기준**: 적이 합리적(예측 가능)한 행동을 수행.

### FR-10 애니메이션 / VFX / 오디오 훅

- **설명**: 행동 시작/종료 시점에 이벤트를 노출:
    - `OnActionStart(ActionContext)`, `OnActionComplete(ActionResult)`
- **수락기준**: 애니메이션 또는 VFX가 완료되면 전투 루프가 정상적으로 계속됨.

---

## 4. 비기능적 요구사항 (Non-Functional)

- **NFR-01 성능**: 전투 씬에서 최대 8 Actor(4:4) 동작 기준 60FPS 목표. (낮은 폴리/플레이스홀더 모델 사용 권장)
- **NFR-02 확장성**: 스킬/캐릭터/이펙트는 ScriptableObject로 정의하여 데이터 중심 확장 가능.
- **NFR-03 테스트성**: 턴/대미지/스킬 로직은 의존성 주입(랜덤 시드)으로 단위 테스트 가능해야 함.
- **NFR-04 안정성**: 잘못된 타겟/쿨다운 위반 시 예외가 발생하지 않고 사용자에게 피드백을 줄 것.
- **NFR-05 네트워크 준비성**: 현재는 로컬 전투지만, 주요 상태(턴 큐, 액션 시퀀스)는 직렬화 가능한 구조로 설계(향후 멀티플레이 확장 대비).

---

## 5. 데이터 모델 & 클래스 설계 (예시)

아래는 구현에 바로 참고할 수 있는 C# 클래스/ScriptableObject 설계 예시(시그니처 수준).

### 5.1 주요 ScriptableObject

```csharp
// SkillData.cs (ScriptableObject)
public class SkillData : ScriptableObject {
    public string skillId;
    public string displayName;
    public string description;
    public TargetType targetType; // enum
    public PositionRequirement positionReq; // enum
    public int baseDamage;
    public float accuracy; // 0~100
    public float critChance; // 0~100
    public int cooldownTurns;
    public List<StatusEffectData> effects; // 적용되는 상태
    public int actionCost; // (optional)
}

```

```csharp
// StatusEffectData.cs (ScriptableObject)
public class StatusEffectData : ScriptableObject {
    public string effectId;
    public string name;
    public int durationTurns;
    public bool stackable;
    public TickTiming tickTiming; // OnTurnStart/OnTurnEnd/Instant
    // 효과 내용(데미지, 버프 수치 등)은 구조화
}

```

### 5.2 런타임 클래스 (요약)

```csharp
public class Character {
    public string id;
    public CharacterStats stats;
    public int positionIndex; // 0..3
    public List<StatusEffectInstance> activeEffects;
    public bool IsAlive => stats.currentHP > 0;
}

public class CharacterStats {
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public int speed;
    public float accuracy;
    public float evasion;
    public float critChance;
    public int stress;
}

```

### 5.3 BattleManager API

```csharp
public class BattleManager : MonoBehaviour {
    public event Action OnBattleStarted;
    public event Action<Character> OnTurnStarted;
    public event Action<Character> OnTurnEnded;
    public event Action<ActionResult> OnActionResolved;
    public event Action<BattleResult> OnBattleEnded;

    public void StartBattle(List<Character> allies, List<Character> enemies, RoomContext ctx);
    public void RequestPlayerAction(string actorId, string skillId, string targetId); // 호출: UI
    public void ForceEndTurn(string actorId); // 디버그용
    public BattleState GetCurrentState();
}

```

---

## 6. 전투 상태 머신 (BattleState)

- `Idle` → `Starting` → `AwaitingInput` → `ResolvingAction` → `CheckingVictory` → `Ended`
- **AwaitingInput**: 플레이어 차례이면 입력 대기, AI 차례이면 즉시 행동 선택 후 `ResolvingAction`.
- **ResolvingAction**: 애니메이션/이펙트가 끝날 때까지 대기(비동기).

---

## 7. 시퀀스 플로우 (텍스트 다이어그램)

1. 플레이어가 방에 들어감 → `BattleManager.StartBattle(...)`
2. `BattleManager` 초기화: 스폰, UI 초기화, 턴 큐 계산 → `OnBattleStarted`
3. 첫 Actor의 턴 → `OnTurnStarted(actor)`
4. 상태효과(턴시작) 처리
5. Actor가 플레이어면 UI로 입력(스킬 선택/타겟) 대기, AI면 `AIController.ChooseAction()`
6. `BattleManager.ResolveAction(action)` 호출 → 데미지 계산 → 상태적용 → 애니메이션 이벤트 호출
7. 애니메이션 완료 콜백 수신 → 상태효과 갱신(턴감소) → `OnTurnEnded`
8. 승패 체크(`CheckVictory`) → 승/패이면 `OnBattleEnded`, 아니면 다음 턴(3 반복)

---

## 8. UI 요구사항 (전투 관련)

- **턴 오더 바**: 현재 턴과 다음 턴 목록 시각화(아이콘/슬롯).
- **액션/스킬 패널**: 현재 선택된 Actor의 스킬 버튼(쿨다운 표기, 사용 불가 상태 회색 처리).
- **타겟 하이라이트**: 마우스 오버 시 타겟 가능/불가능 색상 표시.
- **상태 아이콘**: 각 캐릭터 HUD에 상태 아이콘과 남은 턴 숫자.
- **횃불/스트레스 인디케이터**: 전투 화면에 횃불 보정 표시(툴팁 포함).

---

## 9. 테스트 케이스 및 수락 기준 (Acceptance Tests)

각 항목은 자동화 단위테스트/통합테스트로 검증 가능해야 함.

- **TC-01 턴 순서 재현성**
    - 조건: 동일 seed, 동일 캐릭터 stats.
    - 기대: 턴 순서 동일.
- **TC-02 기본 공격 데미지**
    - 조건: attacker.attack=20, skill.baseDamage=5, target.defense=10
    - 기대: 최종데미지 계산 함수 호출 후 target HP 감소(>=1).
- **TC-03 스킬 쿨다운**
    - 조건: 스킬 사용 → 쿨다운 2턴 설정
    - 기대: 사용 후 2턴 동안 스킬 사용 불가.
- **TC-04 상태이상 지속 시간**
    - 조건: Bleed duration=3턴 적용
    - 기대: 3턴 동안 피해가 발생하고 4턴째에는 사라짐.
- **TC-05 횃불 보정**
    - 조건: 횃불 낮음 → 적 공격력 증가
    - 기대: 횃불 수치 바꾸면 전투 보정 즉시 적용되어 전투 결과에 영향.
- **TC-06 전투 종료**
    - 조건: 모든 적 사망
    - 기대: `OnBattleEnded`가 호출되고 전투 씬/UI가 종료됨.

---

## 10. 에러/예외 처리 및 엣지 케이스

- 잘못된 타겟(이미 사망 등): 액션 거부 & 사용자 피드백.
- 스킬 사용 중간에 타깃이 사망: 대상 재선택 또는 행동 실패 판정(스킬 정의에 따름).
- 동시 소멸(마지막 공격자가 서로 죽이는 경우): 승패 판정 우선순위(플레이어 우선 또는 동시무승부 처리) 규정 필요 — **프로토타입에서는 플레이어 승리 우선 처리**로 단순화.
- 애니메이션/이펙트 실패(콜백 누락): 타임아웃 후 루프 계속.

---

## 11. 우선순위화된 개발 작업 목록 (프로토타입 우선)

(1주 플랜 기준 핵심 작업)

1. `BattleManager` 기본 골격 & 상태머신 구현 (핵심)
2. `Character`, `CharacterStats` 및 파티/적 스폰 로직 (핵심)
3. 턴 순서 계산 및 턴 루프 (핵심)
4. 기본 공격 스킬 및 `SkillData` SO (핵심)
5. 위치(포지션) 시스템과 타겟 필터링 (핵심)
6. 간단한 AIController (적 행동) (핵심)
7. 상태이상 시스템(적어도 Bleed, Stun) (우선)
8. 횃불 보정 연동(전투 시작 시 보정 적용) (핵심)
9. UI(턴바, 스킬 버튼, 상태 아이콘)와 입력 연동 (핵심)
10. 단위/통합 테스트 케이스 작성 및 실행 (필수)

---

## 12. Definition of Done (완료 정의)

- 위 핵심 기능(FR-01 ~ FR-08)이 모두 동작하여 **한 번의 던전 탐험에서 맵 이동 → 전투 진입 → 전투(턴제) → 전투 종료 → 횃불 보정 반영** 플레이 루프가 문제 없이 수행된다.
- 최소 다음 항목이 충족되어야 함:
    - 전투 시작/종료 이벤트 정상 동작
    - 턴 순서와 턴 루프가 안정적
    - 스킬 사용(쿨다운 포함) 정상
    - 상태효과(지속/삭제) 정상
    - 횃불 보정이 전투에 실시간 반영
    - 기본 AI가 동작
    - 핵심 테스트 6개 이상 통과