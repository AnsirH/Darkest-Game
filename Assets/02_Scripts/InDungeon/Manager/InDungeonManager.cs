using DarkestLike.InDungeon.UI;
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

        // Properties
        /// <summary> Character Container Controller /// </summary>
        public PartyController PartyCtrl => partyCtrl;
        public RoomData CurrentRoom => mapSubsystem.CurrentRoom;
        public TileData CurrentTile => mapSubsystem.CurrentTile;

        //void OnSceneLoadedHandler(string sceneName)
        //{
        //}

        // 던전 입장
        public void EnterDungeon(MapData mapData, List<CharacterData> characterDatas)
        {
            InitializeSubsystems();
            mapSubsystem.SetMapData(mapData);
            uiSubsystem.GenerateMapUI(mapSubsystem.MapData);
            unitSubsystem.SetCharacterDatas(characterDatas);
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

        #region Enter Room
        public void EnterRoom(RoomData roomData)
        {
            StartCoroutine(EnterRoomCoroutine(roomData));
        }
        
        public void EnterExitRoom()
        {
            if (CurrentTile == null) return;
            
            EnterRoom(mapSubsystem.CurrentHallway.ExitRoom);
            DungeonEventBus.Publish(DungeonEventType.ExitHallway);
        }

        // 룸에 들어가는 로직
        // 페이드인 포함
        // 페이드 이후 EnterRoom 이벤트 발행
        private IEnumerator EnterRoomCoroutine(RoomData roomData)
        {
            mapSubsystem.SetRoomData(roomData);

            yield return StartCoroutine(uiSubsystem.FadeOutCoroutine(false, 1));
            
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
            DungeonEventBus.Publish(DungeonEventType.Loading);
            StartCoroutine(EnterHallwayCoroutine(targetRoomData));
        }

        IEnumerator EnterHallwayCoroutine(RoomData targetRoomData)
        {
            yield return StartCoroutine(uiSubsystem.FadeOutCoroutine(true, 1));

            mapSubsystem.SetHallwayData(mapSubsystem.CurrentRoom.GetExitHallway(targetRoomData));
            
            float hallwayLength = mapSubsystem.CurrentHallway.Tiles.Length * mapSubsystem.TileLength;
            cameraSubsystem.SetCameraMovementLimit(hallwayLength);
            
            yield return new WaitForSeconds(0.5f);
            
            yield return StartCoroutine(uiSubsystem.FadeOutCoroutine(false, 1));
            
            DungeonEventBus.Publish(DungeonEventType.EnterHallway);
        }

        #endregion

        public void FadeOut(float duration)
        {
            StartCoroutine(uiSubsystem.FadeOutCoroutine(true, duration));
        }
    }
}