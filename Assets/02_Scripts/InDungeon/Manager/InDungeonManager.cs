using DarkestLike.InDungeon.BattleSystem;
using DarkestLike.Map;
using DarkestLike.SceneLoad;
using DarkestLike.Singleton;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkestLike.Character;
using DarkestLike.InDungeon.Hallway;
using DarkestLike.InDungeon.CameraControl;
using DarkestLike.InDungeon.Unit;
using UISubsystem = _02_Scripts.InDungeon.UI.UISubsystem;

namespace DarkestLike.InDungeon.Manager
{
    // 던전 씬에서 던전 진행 여부를 관리하는 객체
    // 던전 입장
    // 던전 진행 상태 업데이트
    // 던전 종료
    public partial class InDungeonManager : Singleton<InDungeonManager>
    {
        [Header("Subsystem")]
        [SerializeField] MapSubsystem mapSubsystem;
        [SerializeField] UISubsystem uiSubsystem;
        [SerializeField] BattleSubsystem battleSubsystem;
        [SerializeField] HallwaySubsystem hallwaySubsystem;
        [SerializeField] CameraSubsystem cameraSubsystem;
        [SerializeField] UnitSubsystem unitSubsystem;

        [Header("References")]
        [SerializeField] PartyController partyCtrl;
        [SerializeField] MapSOData mapSOData;

        // Variables
        private bool isTransitioning = false;

        // Properties
        /// <summary> Character Container Controller /// </summary>
        public PartyController PartyCtrl => partyCtrl;
        public UISubsystem UISubsystem => uiSubsystem;
        public UnitSubsystem UnitSubsystem => unitSubsystem;
        public CameraSubsystem CameraSubsystem => cameraSubsystem;
        public BattleSubsystem BattleSubsystem => battleSubsystem;
        public RoomData CurrentRoom => mapSubsystem.CurrentRoom;
        public TileData CurrentTile => mapSubsystem.CurrentTile;
        public CurrentLocation CurrentLocation => mapSubsystem.CurrentLocation;
        public Camera ViewCamera => cameraSubsystem.MainCamera;
        public bool IsTransitioning => isTransitioning;

        // 던전 입장
        public void EnterDungeon(MapData mapData, List<CharacterData> characterDatas)
        {
            InitializeSubsystems();
            mapSubsystem.SetMapData(mapData);
            uiSubsystem.GenerateMapUI(mapSubsystem.MapData);
            CreatePlayerCharacter(characterDatas);
            SelectPlayerUnit(unitSubsystem.PlayerUnits[0]);  // 비즈니스 로직을 거쳐서 UI 업데이트
            EnterRoom(mapData.StartRoom);
        }

        void InitializeSubsystems()
        {
            mapSubsystem.Initialize();
            uiSubsystem.Initialize();
            battleSubsystem.Initialize();
            hallwaySubsystem.Initialize();
            cameraSubsystem.Initialize();
            unitSubsystem.Initialize();
        }

        private void CreatePlayerCharacter(List<CharacterData> characterDatas)
        {
            for (int i = 0; i < characterDatas.Count; ++i)
            {
                if (unitSubsystem.AddPlayerCharacter(characterDatas[i], partyCtrl.PositionTargets[i],
                        out CharacterUnit createdUnit))
                {
                    uiSubsystem.CreateHpBar(createdUnit);
                    uiSubsystem.CreateStatusEffectBar(createdUnit);
                }
            }
        }

        #region Enter Room
        public void EnterRoom(RoomData roomData)
        {
            StartCoroutine(EnterRoomCoroutine(roomData));
        }
        
        public void EnterExitRoom()
        {
            if (CurrentLocation == CurrentLocation.Room) return;
            
            SetPlayerPositionToTarget();
            EnterRoom(mapSubsystem.CurrentHallway.ExitRoom);
            DungeonEventBus.Publish(DungeonEventType.ExitHallway);
        }

        // 룸에 들어가는 로직
        // 페이드인 포함
        // 페이드 이후 EnterRoom 이벤트 발행
        private IEnumerator EnterRoomCoroutine(RoomData roomData)
        {
            isTransitioning = true;

            mapSubsystem.SetRoomData(roomData);
            partyCtrl.SetMovableLimit(5);
            cameraSubsystem.SetToRoomTarget();

            yield return StartCoroutine(uiSubsystem.FadeOutCoroutine(false, 1));

            isTransitioning = false;

            DungeonEventBus.Publish(DungeonEventType.EnterRoom);
            // 배틀 확인
            if (roomData.IsBattleTile)
            {
                StartBattle(roomData.EnemyGroup);
            }
            else
            {
                uiSubsystem.MapDrawer.HighlightNearRooms(roomData);
            }
        }
        #endregion

        #region Enter Hallway
        public void StartEnteringHallway(RoomData targetRoomData)
        {
            if (targetRoomData == null) return;

            // 검증 1: 배틀 중 체크
            if (battleSubsystem.IsBattleActive)
            {
                Debug.LogError("[InDungeonManager] Cannot transition during battle");
                return;
            }

            // 검증 2: 이미 전환 중 체크
            if (isTransitioning)
            {
                Debug.LogWarning("[InDungeonManager] Transition already in progress");
                return;
            }

            // 검증 3: 현재 위치가 방인지 체크
            if (CurrentLocation != CurrentLocation.Room)
            {
                Debug.LogError("[InDungeonManager] Can only enter hallway from a room");
                return;
            }

            // 검증 4: 인접한 방인지 체크
            if (!CurrentRoom.CheckIsMoveableRoom(targetRoomData))
            {
                Debug.LogError($"[InDungeonManager] Target room {targetRoomData.Position} is not adjacent");
                return;
            }

            StartCoroutine(EnterHallwayCoroutine(targetRoomData));
        }

        IEnumerator EnterHallwayCoroutine(RoomData targetRoomData)
        {
            isTransitioning = true;

            yield return StartCoroutine(uiSubsystem.FadeOutCoroutine(true, 1));

            DungeonEventBus.Publish(DungeonEventType.Loading);
            mapSubsystem.SetHallwayData(mapSubsystem.CurrentRoom.GetExitHallway(targetRoomData));
            SetPlayerPositionToTarget();

            float hallwayLength = mapSubsystem.CurrentHallway.Tiles.Length * mapSubsystem.TileLength;
            cameraSubsystem.SetCameraMovementLimit(hallwayLength);
            cameraSubsystem.SetToPartyTarget();

            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(uiSubsystem.FadeOutCoroutine(false, 1));

            isTransitioning = false;

            DungeonEventBus.Publish(DungeonEventType.EnterHallway);
        }

        #endregion

        void SetPlayerPositionToTarget()
        {
            foreach (var playerUnit in unitSubsystem.PlayerUnits)
                playerUnit.SetPositionToTarget();
        }
        
        public void FadeOut(float duration)
        {
            StartCoroutine(uiSubsystem.FadeOutCoroutine(true, duration));
        }
    }
}