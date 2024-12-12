using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestGame.Map
{
    public enum RoomType
    {
        None = 0,
        Item,
        Monster,
        MonsterAndItem
    }

    public enum ExitBridgeDirection
    {
        Up = 0,
        Left = 1,
        Right = 2,
        Down = 3
    }

    public enum TileType
    {
        None = 0,
        Item,
        Monster
    }

    [System.Serializable]
    public class Map
    {
        MapData mapData;
        [SerializeField] List<RoomData> rooms = new();
        [SerializeField] List<BridgeData> bridges = new();
        [SerializeField] List<TileData> tiles = new();

        public MapData MapData => mapData;
        public List<RoomData> Rooms => rooms;
        public List<BridgeData> Bridges => bridges;
        public List<TileData> Tiles => tiles;

        public int RoomCount => rooms.Count;
        public int BridgeCount => bridges.Count;
        public int TileCount => mapData.TileCount;

        public RoomData startRoom;

        public Map(MapData mapData, List<RoomData> rooms, List<BridgeData> bridges, List<TileData> tiles)
        {
            this.mapData = mapData;
            this.rooms = rooms;
            this.bridges = bridges;
            this.tiles = tiles;

            startRoom = this.rooms[0];
        }
    }

    [System.Serializable]
    /// <summary> 방 위치, 타입, 나가는 통로 </summary>
    public class RoomData
    {
        public Vector2Int position;
        public RoomType type;
        public BridgeData[] exitBridges = new BridgeData[4];

        public RoomData(Vector2Int position, RoomType type,
            BridgeData leftBridge = null, BridgeData rightBridge = null, BridgeData upBridge = null, BridgeData downBridge = null)
        {
            this.position = position;
            this.type = type;

            exitBridges[(int)ExitBridgeDirection.Up] = upBridge;
            exitBridges[(int)ExitBridgeDirection.Left] = leftBridge;
            exitBridges[(int)ExitBridgeDirection.Right] = rightBridge;
            exitBridges[(int)ExitBridgeDirection.Down] = downBridge;
        }

        public bool IsFullBridge
        {
            get
            {
                for (int i = 0; i < exitBridges.Length; i++)
                {
                    if (exitBridges[i] == null) { return false; }
                }
                return true;
            }
        }

        public int GetRandomEmptyBridgeIndex()
        {
            if (IsFullBridge) return -1;

            while (true)
            {
                int randomIndex = Random.Range(0, 4);
                if (exitBridges[randomIndex] == null) { return randomIndex; }
            }
        }

        public void ConnectBridge(RoomData targetRoom, TileData[] tiles, ExitBridgeDirection direction)
        {
            if (exitBridges[(int)direction] == null)
            {
                // 가는 통로 생성
                BridgeData goBridge = new(tiles, targetRoom);
                exitBridges[(int)direction] = goBridge;
                goBridge.linkedRoom = targetRoom;

                TileData[] backBridgeTiles = new TileData[tiles.Length];
                for (int i = 0; i < goBridge.tiles.Length; i++)
                {
                    backBridgeTiles[i] = goBridge.tiles[goBridge.tiles.Length - 1 - i];
                }// 역순으로 배치

                // 오는 통로 생성
                BridgeData backBridge = new(backBridgeTiles, this);
                targetRoom.exitBridges[3 - (int)direction] = backBridge;
                backBridge.linkedRoom = this;
            }
        }
    }

    public class BridgeData
    {
        public TileData[] tiles;
        public RoomData linkedRoom;

        public BridgeData(TileData[] tiles, RoomData linkedRoom)
        {
            this.tiles = tiles;
            this.linkedRoom = linkedRoom;
        }
    }

    public class TileData
    {
        public TileType type;

        public TileData(TileType type)
        {
            this.type = type;
        }
        
        public void SetType(TileType newType)
        {
            type = newType;
        }
    }
}

