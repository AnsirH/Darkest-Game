# Darkest Dungeon 3D Prototype

Unity로 개발된 다키스트 던전 3D 모작 프로젝트입니다. 원작의 핵심 플레이 루프인 던전 탐험, 턴제 전투, 스트레스/횃불 시스템을 3D 환경에서 구현한 프로토타입입니다.

## 프로젝트 개요

- **플랫폼**: PC (Windows, Unity 기반)
- **장르**: 던전 탐험 + 턴제 전략 RPG
- **Unity 버전**: 2022 LTS 이상
- **렌더링 파이프라인**: URP (Universal Render Pipeline)

## 주요 기능

### 1. 던전 탐험 시스템
- **노드 기반 맵 구조**: Room → Hallway → Room 형태의 던전 생성
- **동적 맵 생성**: MapGenerator를 통한 절차적 던전 생성
- **맵 선택 UI**: 던전별로 여러 맵을 선택하여 탐험 가능
- **진행도 관리**: 던전 레벨과 경험치 시스템

### 2. 캐릭터 시스템
- **스탯 시스템**: HP, 공격력, 방어력, 속도, 명중률, 회피율
- **레벨업 시스템**: 레벨에 따른 스탯 증가
- **버프/디버프**: StatModifier를 통한 일시적 스탯 변경
- **위치 기반 전투**: 앞줄/뒷줄 위치에 따른 전투 시스템

### 3. 전투 시스템
- **턴제 전투**: 속도 기반 턴 순서 결정
- **캐릭터 관리**: BattleManager를 통한 전투 캐릭터 관리
- **스킬 시스템**: 위치 기반 스킬 사용

### 4. 이동 시스템
- **횡스크롤 이동**: 좌우 이동으로 던전 탐험
- **제한된 이동**: LimitedMovement를 통한 이동 범위 제한
- **부드러운 이동**: PositionMaintainer를 통한 자연스러운 캐릭터 이동

### 5. UI 시스템
- **씬별 UI 관리**: 각 씬마다 전용 UI 컨트롤러
- **던전 패널**: 던전 정보 표시 및 맵 선택
- **로딩 시스템**: LoadingManager를 통한 씬 전환 관리

## 프로젝트 구조

### 씬 구성
- **Main Scene**: 게임 시작 화면
- **Start And Ready Scene**: 던전 선택 및 준비 화면
- **Playing Scene**: 실제 게임플레이 화면
- **Loading Scene**: 씬 전환 로딩 화면

### 스크립트 구조

#### 01_Main_Scene
- `MainSceneUI.cs`: 메인 씬 UI 관리

#### 02_Start_And_Ready_Scene
- `DungeonManager.cs`: 던전 데이터 및 진행도 관리
- `MapSelectController.cs`: 맵 선택 컨트롤러
- `StartAndReadySceneUI.cs`: 준비 씬 UI 관리
- `DungeonPanel.cs`: 던전 정보 패널 UI

#### 03_Playing_Scene
- **BattleSystem**: 전투 관련 시스템
  - `BattleManager.cs`: 전투 관리
  - `Character.cs`: 캐릭터 클래스
  - `CharacterStats.cs`: 캐릭터 스탯 관리
  - `StatModifier.cs`: 스탯 수정자
- **Movement**: 이동 시스템
  - `Movement.cs`: 기본 이동 클래스
  - `LimitedMovement.cs`: 제한된 이동
- **Map**: 맵 관련 시스템
  - `MapDrawer.cs`: 맵 UI 그리기
  - `RoomButton.cs`: 방 버튼 UI
  - `TileButton.cs`: 타일 버튼 UI
- `CharacterAnimationController.cs`: 캐릭터 애니메이션
- `CharacterContainerController.cs`: 캐릭터 컨테이너 관리
- `PlayerInput.cs`: 플레이어 입력 처리
- `PositionMaintainer.cs`: 위치 유지 시스템
- `RandomProp.cs`: 랜덤 오브젝트 생성
- `MapStage.cs`: 맵 스테이지 설정

#### Map
- `Map.cs`: 맵 데이터 구조 정의
- `MapGenerator.cs`: 절차적 맵 생성

#### Scriptable
- `DungeonData.cs`: 던전 데이터
- `MapData.cs`: 맵 데이터
- `MapThemeData.cs`: 맵 테마 데이터
- `IngameData.cs`: 인게임 설정 데이터

#### PUBLIC
- `LoadingManager.cs`: 씬 로딩 관리
- `Singleton.cs`: 싱글톤 패턴 구현

## 데이터 구조

### 맵 시스템
- **RoomData**: 방의 위치, 타입, 연결된 다리 정보
- **BridgeData**: 방을 연결하는 다리와 타일 정보
- **TileData**: 개별 타일의 타입 정보

### 던전 시스템
- **Dungeon**: 던전의 레벨, 경험치, 현재 맵 목록
- **DungeonData**: 던전의 기본 정보와 설정

### 캐릭터 시스템
- **CharacterStats**: 기본 스탯과 버프된 스탯 관리
- **StatModifier**: 스탯 수정자 (지속 시간 포함)

## 기술적 특징

1. **ScriptableObject 활용**: 게임 데이터를 에셋으로 관리
2. **싱글톤 패턴**: 매니저 클래스들의 전역 접근
3. **네임스페이스 사용**: DarkestGame.Map 네임스페이스로 맵 관련 코드 분리
4. **컴포넌트 기반 설계**: Unity의 컴포넌트 시스템 활용
5. **비동기 처리**: UniTask를 사용한 씬 로딩

## 개발 상태

현재 프로토타입 단계로, 다키스트 던전의 핵심 시스템들이 기본적으로 구현되어 있습니다. 추가 개발이 필요한 부분들:

- 전투 시스템의 상세 구현
- 스트레스/횃불 시스템 구현
- 카메라 효과 (Cinemachine 활용)
- 사운드 시스템
- UI/UX 개선

## 실행 방법

1. Unity 2022 LTS 이상에서 프로젝트 열기
2. Main Scene을 빌드 설정에 추가
3. 플레이 모드로 실행하여 게임 테스트

이 프로젝트는 다키스트 던전의 핵심 메커니즘을 3D 환경에서 재구현한 학습용 프로토타입입니다.
