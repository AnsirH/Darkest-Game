using DarkestLike.InDungeon;
using DarkestLike.InDungeon.Hallway;
using DarkestLike.SceneLoad;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkestLike.Map
{
    public enum CurrentLocation
    {
        Room,
        Hallway
    }

    public class InDungeonMapSubsystem : InDungeonSubsystem
    {
        // Variables
        private MapData map;
        private HallwayData currentHallway;
        private RoomData currentRoom;
        private TileData currentTile;
        private CurrentLocation currentLocation;

        // Properties
        public MapData MapData => map;
        public HallwayData CurrentHallway => currentLocation == CurrentLocation.Hallway ? currentHallway : null;
        public RoomData CurrentRoom => currentLocation == CurrentLocation.Room ? currentRoom : null;
        public TileData CurrentTile => currentLocation == CurrentLocation.Hallway ? currentTile : null;
        public Vector3 CurrentTilePosition
        {
            get
            {
                if (CurrentLocation == CurrentLocation.Hallway)
                {
                    return Vector3.right * (TileWorldDistance * 0.5f + CurrentHallway.GetTileIndex(currentTile) * TileWorldDistance);
                }
                return Vector3.zero;
            }
        }
        public float TileWorldDistance = 5.0f;
        public CurrentLocation CurrentLocation => currentLocation;

        protected override void OnInitialize()
        {
            SceneManager.sceneLoaded += SceneLoadedHandler;
        }

        void SceneLoadedHandler(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Hallway Scene")
            {
                Debug.Log("[MapSubsystem] 복도 씬 로드");
            }
            else if (scene.name == "Room Scene")
            {
                Debug.Log("[MapSubsystem] 방 씬 로드");
                //if (currentRoom != null)
                //    OnRoomEntered?.Invoke(currentRoom);
            }
        }

        public void SetCurrentRoomData(RoomData roomData)
        {
            currentLocation = CurrentLocation.Room;
            currentRoom = roomData;
        }

        public void SetHallwayDataByRoomData(RoomData nextRoom)
        {
            if (currentLocation != CurrentLocation.Room) return;
            if (!currentRoom.CheckIsMoveableRoom(nextRoom)) return;

            currentLocation = CurrentLocation.Hallway;
            currentHallway = currentRoom.GetExitHallway(nextRoom);
            currentTile = currentHallway.Tiles[0];
        }

        public override void Shutdown()
        {
            base.Shutdown();
            SceneManager.sceneLoaded -= SceneLoadedHandler;
        }

        public void SetMapData(MapData mapData)
        {
            map = mapData;
            currentRoom = map.Rooms[0];
            currentLocation = CurrentLocation.Room;
        }

        public bool CanEnterableTile(TileData tileData)
        {
            if (currentLocation != CurrentLocation.Hallway) return false;

            for (int i = 0; i < currentHallway.Tiles.Length; ++i)
            {
                //if (currentHallway.Tiles[i] == currentTile) continue;
                if (currentHallway.Tiles[i] == tileData) return true;
            }

            return false;
        }

        public bool EnterTheTile(TileData tileData)
        {
            if (CanEnterableTile(tileData))
            {
                currentTile = tileData;
                return true;
            }
            else return false;
        }
    }
}