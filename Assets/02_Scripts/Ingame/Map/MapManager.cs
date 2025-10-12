using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkestGame.Map
{
    public enum CurrentLocation
    {
        Room,
        Hallway
    }

    public class MapManager : Singleton<MapManager>
    {
        [Header("references")]
        [SerializeField] MapSOData mapData;
        [SerializeField] CharacterContainerController characterContainer;

        // variables
        Map map;
        HallwayData currentHallway;
        RoomData currentRoom;
        TileData currentTile;
        CurrentLocation currentLocation;
        bool isInitialized = false;

        // properties
        public Map Map => map;
        public HallwayData CurrentHallway => currentLocation == CurrentLocation.Hallway ? currentHallway : null;
        public RoomData CurrentRoom => currentLocation == CurrentLocation.Room ? currentRoom : null;
        public TileData CurrentTile => currentLocation == CurrentLocation.Hallway ? currentTile : null;
        public float TileWorldDistance = 5.0f;
        public CurrentLocation CurrentLocation => currentLocation;

        // events
        public event Action OnEnterRoom;
        public event Action OnEnterTile;

        public override void Awake()
        {
            base.Awake();

            if (!isInitialized)
            {
                map = MapGenerator.GenerateMap(mapData);
                SceneManager.sceneLoaded += SceneLoadedHandler;

                isInitialized = true;
            }
        }

        private void Start()
        {
            if (currentLocation == CurrentLocation.Room && currentRoom == null)
                EnterTheRoom(map.Rooms[0]);
        }

        //private void OnLevelWasLoaded(int level)
        //{
        //    if (level < 3)
        //    {
        //        OnEnterRoom = null;
        //        OnEnterTile = null;
        //        Debug.Log("여기 어디야. 초기화!");
        //    }
        //    else
        //    {
        //        Debug.Log("오 던전이다.");

        //        if (currentLocation == CurrentLocation.Room && currentRoom != null)
        //        {
        //            OnEnterRoom?.Invoke();
        //        }
        //        else if (currentLocation == CurrentLocation.Hallway && currentHallway != null)
        //            OnEnterTile?.Invoke();
        //        characterContainer = FindObjectOfType<CharacterContainerController>();
        //    }            
        //}

        void SceneLoadedHandler(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Hallway Scene")
            {
                Debug.Log("복도 입장");
                OnEnterTile?.Invoke();
                characterContainer = FindObjectOfType<CharacterContainerController>();
            }
            else if (scene.name == "Room Scene")
            {
                Debug.Log("방 입장");
                if (currentRoom != null)
                    OnEnterRoom?.Invoke();
            }
            else if (scene.name == "Loading Scene")
            {
                OnEnterRoom = null;
                OnEnterTile = null;
            }
            else
            {
                DestroySelf();
            }
        }

        public void CheckCurrentTile()
        {
            currentTile = currentHallway.Tiles[(int)(characterContainer.transform.position.x / TileWorldDistance)];
        }

        public void EnterTheRoom(RoomData roomData)
        {
            currentLocation = CurrentLocation.Room;
            currentRoom = roomData;
            OnEnterRoom?.Invoke();
        }

        public void ExitRoom(RoomData nextRoom)
        {
            if (currentLocation != CurrentLocation.Room) return;

            currentLocation = CurrentLocation.Hallway;
            currentHallway = currentRoom.GetExitHallway(nextRoom);
            currentTile = currentHallway.Tiles[0];
            SceneLoadManager.Inst.LoadSceneWithDungeonSetup(currentHallway.SceneName).Forget();
        }
    }
}