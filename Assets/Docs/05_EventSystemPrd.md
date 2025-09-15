## 1. 시스템 개요

- **목적**:
    
    던전 생성 시 각 Room(방)과 Hallway(복도)의 Tile(타일)에 발생할 이벤트를 미리 배치하여, 탐험 중 플레이어가 해당 타일에 진입했을 때 이벤트가 실행되도록 한다.
    
- **적용 범위**:
    - **방(Room Tile)**: 주요 전투, 상자, 강화 이벤트 배치
    - **복도(Hallway Tile)**: 소규모 전투, 장애물, 상자, 아이템 발견, 강화 이벤트 배치

---

## 2. 시스템 기능 요구사항

### **REQ-EVENTSYS-001 이벤트 사전 배치**

- 던전 맵을 생성할 때, 각 Room과 Tile마다 발생 가능한 이벤트를 무작위로 결정해야 한다.
- Room과 Tile은 이벤트가 없이 비어있을 수도 있다.
- 배치된 이벤트는 게임 플레이 중 변경되지 않아야 한다.

---

### **REQ-EVENTSYS-002 타일 유형별 이벤트 매핑**

- 장소에 따라 발생 가능한 이벤트 종류가 다르며, 맵 생성 시 이벤트가 미리 할당된다.

| 장소 | 발생 가능한 이벤트 |
| --- | --- |
| Room(방) | 전투, 상자, 강화 |
| Tile(타일) | 전투, 장애물, 상자, 아이템 발견, 강화 |

---

### **REQ-EVENTSYS-003 확률 기반 이벤트 배치**

- 이벤트는 맵 생성 시 확률 기반으로 각 Room과 Tile에 배치되어야 한다.

예시 확률:

- Room: 전투 40%, 상자 30%, 없음 20%, 강화 10%
- **복도**: 없음 40%, 전투 20%, 장애물 10%, 상자 10%, 아이템 발견 15%, 강화 5%

---

### **REQ-EVENTSYS-004 이벤트 난이도 및 보상 차등**

- **방 이벤트**는 복도 이벤트보다 난이도가 높고, 보상이 커야 한다.
- 보상 및 난이도는 장소(Room/Tile)에 따라 별도의 테이블로 정의해야 한다.

---

### **REQ-EVENTSYS-005 이벤트 데이터 구조**

- 각 타일은 생성 시점에 이벤트 정보를 포함해야 한다.

### 1. 공통 구조

```csharp
public interface IEventData
{
    string EventId { get; }
    string Name { get; }
    string Description { get; }
    EventType Type { get; }
}

public enum EventType
{
    Empty, Combat, Chest, Enhancement, Obstacle, Item
}
```

---

### 2. 데이터 클래스

### RoomData

```csharp
public class RoomData
{
    public int RoomId { get; set; }
    public Vector2Int Position { get; set; }
    public IEventData Event { get; set; }

    public bool IsBossRoom { get; set; }
    public bool IsCleared { get; set; }
}
```

- 이벤트 후보: Combat(EVT-101), Chest(EVT-102), Enhancement(EVT-103), Empty(EVT-000, 특수)

---

### TileData

```csharp
public class TileData
{
    public int TileId { get; set; }
    public Vector2Int Position { get; set; }
    public IEventData Event { get; set; }

    public bool IsDiscovered { get; set; }
    public bool IsCleared { get; set; }
}
```

- 이벤트 후보: Combat(EVT-201), Obstacle(EVT-202), Chest(EVT-203), Item(EVT-204), Enhancement(EVT-205), Empty(EVT-000)

---

## 3. 이벤트 예시

```csharp
public class CombatEvent : IEventData
{
    public string EventId { get; }
    public string Name { get; }
    public string Description { get; }
    public EventType Type => EventType.Combat;

    public int EnemyGroupId { get; }
    public int Difficulty { get; }
    public RewardData Reward { get; }
}
```

```csharp
public class RewardData
{
    public int Gold { get; set; }
    public int Exp { get; set; }
    public List<string> ItemIds { get; set; }
}
```

---

### 4. 규칙

1. **RoomData**: Combat, Chest, Enhancement 중 하나 필수. (특수 케이스로 Empty 가능)
2. **TileData**: 모든 이벤트 가능, Empty 포함.
3. **전투/강화 난이도·보상**: Room > Tile.
4. 이벤트는 **EVT-XXX ID** 기반으로 확장 관리.

---

### **REQ-EVENTSYS-006 이벤트 실행 파이프라인**

1. 던전 맵이 생성될 때, 각 타일에 이벤트를 할당한다.
2. 플레이어가 타일에 진입하면 이미 배치된 이벤트를 불러온다.
3. 이벤트 처리기를 실행하여 결과(전투, 보상, 강화 등)를 적용한다.
4. 이벤트 발생 후 해당 타일은 "소모됨(IsCleared=true)" 상태로 갱신된다.

---

### **REQ-EVENTSYS-007 이벤트 관리 매니저**

- **EventManager**는 이벤트 배치와 실행을 모두 관리한다.
- 주요 역할:
    - 맵 생성 시 이벤트 초기 배치
    - 타일 이벤트 실행 및 결과 적용
    - 이벤트 발생 기록 관리

---

### **REQ-EVENTSYS-008 중복 및 재사용 제한**

- 한 타일에서 배정된 이벤트는 중복 실행되지 않아야 한다.
- 이벤트가 종료되면 해당 타일은 "이벤트 없음" 상태로 변환된다.

---

### **REQ-EVENTSYS-009 예외 처리**

- 이벤트 실행 불가능 상황에 대한 처리 필요:
    - 인벤토리 부족 → 아이템 드롭 또는 소멸
    - 열쇠 부족 → 상자 열기 불가, 함정 발생 가능
    - 파티 전원 사망 → 게임 오버 처리

---

## 3. 비기능 요구사항

- **성능**: 이벤트 배치는 맵 생성 시 단 한 번만 수행되므로 탐험 중 추가 연산은 최소화된다.
- **확장성**: 새로운 이벤트 유형을 추가할 때, 배치 확률과 실행 처리기만 정의하면 된다.
- **재현성**: 동일한 시드(seed)를 사용할 경우 항상 동일한 이벤트 배치 결과를 재현할 수 있어야 한다.

---

## 4. 이벤트 배치 흐름도

```
던전 생성 → 맵 타일 생성 → 이벤트 후보 조회
             ↓
        확률 계산 후 이벤트 배치
             ↓
      타일 데이터에 이벤트 기록
             ↓
  탐험 중 타일 진입 → 기록된 이벤트 실행
             ↓
        이벤트 소모/종료 처리
```