using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestGame.Map
{
    public enum CurrentLocation
    {
        Room,
        Tile
    }

    public class MapManager : Singleton<MapManager>
    {
        [Header("references")]
        [SerializeField] MapData mapData;
        [SerializeField] CharacterContainerController characterContainer;

        // variables
        Map map;
        HallwayData currentHallway;
        RoomData currentRoom;
        TileData currentTile;
        CurrentLocation currentLocation;

        // properties
        public Map Map => map;
        public HallwayData CurrentHallway => currentLocation == CurrentLocation.Tile ? currentHallway : null;
        public RoomData CurrentRoom => currentLocation == CurrentLocation.Room ? currentRoom : null;
        public TileData CurrentTile => currentLocation == CurrentLocation.Tile ? currentTile : null;
        public float TileWorldDistance = 5.0f;
        public CurrentLocation CurrentLocation => currentLocation;

        // events
        public event Action OnEnterRoom;
        public event Action OnEnterTile;

        public override void Awake()
        {
            base.Awake();
            map = MapGenerator.GenerateMap(mapData);
        }

        private void Start()
        {
            EnterTheRoom(map.Rooms[0]);
        }

        public void CheckCurrentTile()
        {
            currentTile = currentHallway.tiles[(int)(characterContainer.transform.position.x / TileWorldDistance)];
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

            currentLocation = CurrentLocation.Tile;
            currentHallway = currentRoom.GetExitHallway(nextRoom);
            currentTile = currentHallway.tiles[0];
            OnEnterTile?.Invoke();
        }
    }
}