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

namespace DarkestLike.InDungeon
{
    public partial class InDungeonManager : Singleton<InDungeonManager>
    {
        [Header("Subsystem")]
        [SerializeField] InDungeonMapSubsystem mapSubsystem;
        [SerializeField] InDungeonUISubsystem UISubsystem;
        [SerializeField] BattleSubsystem battleSubsystem;
        [SerializeField] HallwaySubsystem hallwaySubsystem;
        [SerializeField] CameraSubsystem cameraSubsystem;
        [SerializeField] UnitSubsystem unitSubsystem;

        [Header("References")]
        [SerializeField] CharacterContainerController characterContainer;
        [SerializeField] MapSOData mapSOData;
        // Events
        public event Action<RoomData> OnRoomEntered;
        public event Action<HallwayData> OnHallwayEntered;
        public event Action<TileData> OnTileEntered;

        // Variables

        // Properties

        // 던전 매니저 Awake에서 던전 입장 로직 실행
        protected override void Awake()
        {
            base.Awake();
            //SceneLoadManager.Inst.OnLoadedSceneActivated += OnSceneLoadedHandler;
        }

        // 맵의 첫 번째 방으로
        void Start()
        {
            EnterDungeon(mapSOData);
            StartBattle(EnemyGroupGenerator.GenerateEnemyGroupForRoom(mapSOData, 0));
            //battleSubsystem.StartBattle(
            //    unitSubsystem.CharacterUnits,
            //    EnemyGroupGenerator.GenerateEnemyGroupForRoom(mapSOData, 0).Enemies, 
            //    Vector3.right * mapSubsystem.TileWorldDistance * 0.5f);
        }

        //void OnSceneLoadedHandler(string sceneName)
        //{
        //}

        // 던전 입장( room scene 자체 테스트용 )
        void EnterDungeon(MapSOData mapSOData)
        {
            InitializeSubsystems();
            mapSubsystem.SetMapData(MapGenerator.GenerateMap(mapSOData, 0));
            UISubsystem.GenerateMapUI(mapSubsystem.MapData);
            mapSubsystem.SetCurrentRoomData(mapSubsystem.MapData.StartRoom);
            CompleteRoomEntering();
        }

        public void EnterDungeon(MapData mapData)
        {
            InitializeSubsystems();
            mapSubsystem.SetMapData(mapData);
            UISubsystem.GenerateMapUI(mapSubsystem.MapData);
            mapSubsystem.SetCurrentRoomData(mapSubsystem.MapData.StartRoom);
            CompleteRoomEntering();
        }

        void InitializeSubsystems()
        {
            mapSubsystem.Initialize();
            UISubsystem.Initialize();
            battleSubsystem.Initialize();
            hallwaySubsystem.Initialize();
            cameraSubsystem.Initialize();
            unitSubsystem.Initialize();
        }

        #region Enter Room
        public void EnterRoom()
        {
            if (mapSubsystem.CurrentLocation != CurrentLocation.Hallway) return;
            UISubsystem.MapDrawer.ClearHighlight();
            mapSubsystem.SetCurrentRoomData(mapSubsystem.CurrentHallway.ExitRoom);
            hallwaySubsystem.ActiveExitDoor(false);
            StartCoroutine(EnterRoomCoroutine());
        }

        IEnumerator EnterRoomCoroutine()
        {
            yield return StartCoroutine(hallwaySubsystem.EnterRoomProcess(unitSubsystem.CharacterUnits));

            yield return StartCoroutine(UISubsystem.FadeOutCoroutine(true));

            UISubsystem.ActiveBattleHud(false);
            UISubsystem.ActiveMapDrawer(false);

            SceneLoadManager.Inst.LoadRoomScene().Forget();
        }

        public void CompleteRoomEntering()
        {
            // 몬스터 방 타입 체크
            if (mapSubsystem.CurrentRoom.RoomType == RoomType.Monster || 
                mapSubsystem.CurrentRoom.RoomType == RoomType.MonsterAndItem)
            {
                StartBattle(mapSubsystem.CurrentRoom.EnemyGroup);
            }
            StartCoroutine(UISubsystem.FadeOutCoroutine(false));
            OnRoomEntered?.Invoke(mapSubsystem.CurrentRoom);
            characterContainer.ResetPosition();
        }
#endregion

        #region Enter Hallway
        public void EnterHallway(RoomData targetRoomData)
        {
            if (targetRoomData == null) return;
            UISubsystem.MapDrawer.ClearHighlight();
            mapSubsystem.SetHallwayDataByRoomData(targetRoomData);
            StartCoroutine(EnterHallwayCoroutine());
        }

        IEnumerator EnterHallwayCoroutine()
        {
            yield return StartCoroutine(UISubsystem.FadeOutCoroutine(true));

            UISubsystem.ActiveBattleHud(false);
            UISubsystem.ActiveMapDrawer(false);

            SceneLoadManager.Inst.LoadHallwayScene().Forget();
        }

        public void CompleteHallwayEntering()
        {
            // 몬스터 타일 타입 체크
            if (mapSubsystem.CurrentTile.type == TileType.Monster)
            {
                StartBattle(mapSubsystem.CurrentTile.EnemyGroup);
            }

            float hallwayLength = mapSubsystem.CurrentHallway.Tiles.Length * mapSubsystem.TileWorldDistance;
            hallwaySubsystem.SetHallway(characterContainer, hallwayLength, mapSubsystem.TileWorldDistance);
            cameraSubsystem.SetCameraMovementLimit(hallwayLength - mapSubsystem.TileWorldDistance);
            characterContainer.ResetPosition();
            cameraSubsystem.SetCameraTarget(characterContainer.CamTrf);
            OnTileEntered?.Invoke(mapSubsystem.CurrentTile);
        }

        public void EnterTheTile(int tileIndex)
        {
            if (mapSubsystem.CurrentLocation != CurrentLocation.Hallway) return;
            if (tileIndex >= mapSubsystem.CurrentHallway.Tiles.Length) return;
            if (mapSubsystem.EnterTheTile(mapSubsystem.CurrentHallway.Tiles[tileIndex]))
            {
                // 몬스터 타일 타입 체크
                if (mapSubsystem.CurrentTile.type == TileType.Monster)
                {
                    StartBattle(mapSubsystem.CurrentTile.EnemyGroup);
                }

                OnTileEntered?.Invoke(mapSubsystem.CurrentTile);
            }
        }

        #endregion

    }
}