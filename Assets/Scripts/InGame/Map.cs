using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
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
        public int RoomCount = 0;
        public int BridgeCount = 0;
        public int TileCount = 0;

        public List<RoomData> Rooms = new();
        public List<BridgeData> Bridges = new();
        public List<TileData> Tiles = new();

        public Map(List<RoomData> rooms, List<BridgeData> bridges, List<TileData> tiles)
        {
            Rooms = rooms;
            Bridges = bridges;
            Tiles = tiles;

            RoomCount = Rooms.Count;
            BridgeCount = Bridges.Count;
            TileCount = Bridges[0].tiles.Length;
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

        public void ConnectBridge(RoomData targetRoom, int tileCount, ExitBridgeDirection direction)
        {
            if (exitBridges[(int)direction] == null)
            {
                // 가는 통로 생성
                BridgeData goBridge = new(tileCount, targetRoom);
                exitBridges[(int)direction] = goBridge;
                goBridge.linkedRoom = targetRoom;

                TileData[] backBridgeTiles = new TileData[tileCount];
                for (int i = 0; i < goBridge.tiles.Length; i++)
                {
                    backBridgeTiles[i] = goBridge.tiles[goBridge.tiles.Length - 1 - i];
                }

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

        public BridgeData(int tileCount, RoomData linkedRoom)
        {
            tiles = GetRandomTiles(tileCount);
            this.linkedRoom = linkedRoom;
        }

        private TileData[] GetRandomTiles(int tileCount)
        {
            TileData[] result = new TileData[tileCount];

            for (int i = 0; i < tileCount; i++)
            {
                result[i] = new TileData();
            }

            return result;
        }
    }

    public class TileData
    {
        public TileType type;

        public TileData(TileType type)
        {
            this.type = type;
        }

        public TileData()
        {
            type = (TileType)Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);
        }
    }
}

