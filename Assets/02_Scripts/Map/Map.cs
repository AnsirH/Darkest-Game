using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestGame.Map
{
    /// <summary>방의 타입을 정의하는 열거형</summary>
    public enum RoomType
    {
        None = 0,
        Item,
        Monster,
        MonsterAndItem
    }

    /// <summary>복도 출구 방향을 정의하는 열거형</summary>
    public enum ExitHallwayDirection
    {
        Up = 0,
        Left = 1,
        Right = 2,
        Down = 3
    }

    /// <summary>타일의 타입을 정의하는 열거형</summary>
    public enum TileType
    {
        None = 0,
        Item,
        Monster
    }

    /// <summary>
    /// 던전 맵을 나타내는 클래스
    /// 방, 다리, 타일 정보를 포함합니다.
    /// </summary>
    [System.Serializable]
    public class Map
    {
        MapData mapData;
        [SerializeField] List<RoomData> rooms = new();
        [SerializeField] List<HallwayData> hallways = new();
        [SerializeField] List<TileData> tiles = new();

        public MapData MapData => mapData;
        public List<RoomData> Rooms => rooms;
        public List<HallwayData> Hallways => hallways;
        public List<TileData> Tiles => tiles;

        public int RoomCount => rooms.Count;
        public int HallwayCount => hallways.Count;
        public int TileCount => mapData.TileCount;

        /// <summary>맵의 시작 방</summary>
        public RoomData startRoom;

        /// <summary>
        /// 맵 생성자
        /// </summary>
        /// <param name="mapData">맵 데이터</param>
        /// <param name="rooms">방 목록</param>
        /// <param name="hallways">복도 목록</param>
        /// <param name="tiles">타일 목록</param>
        public Map(MapData mapData, List<RoomData> rooms, List<HallwayData> hallways, List<TileData> tiles)
        {
            this.mapData = mapData;
            this.rooms = rooms;
            this.hallways = hallways;
            this.tiles = tiles;

            startRoom = this.rooms[0];
        }
    }

    /// <summary>
    /// 방의 위치, 타입, 다리 정보를 저장하는 클래스
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        /// <summary>방의 위치 (격자 좌표)</summary>
        public Vector2Int position;
        
        /// <summary>방의 타입</summary>
        public RoomType type;
        
        /// <summary>4방향 복도 배열 (상, 좌, 우, 하)</summary>
        public HallwayData[] exitHallways = new HallwayData[4];

        /// <summary>
        /// 방 데이터 생성자
        /// </summary>
        /// <param name="position">방의 위치</param>
        /// <param name="type">방의 타입</param>
        /// <param name="leftHallway">좌측 복도 (선택사항)</param>
        /// <param name="rightHallway">우측 복도 (선택사항)</param>
        /// <param name="upHallway">상단 복도 (선택사항)</param>
        /// <param name="downHallway">하단 복도 (선택사항)</param>
        public RoomData(Vector2Int position, RoomType type,
            HallwayData leftHallway = null, HallwayData rightHallway = null, HallwayData upHallway = null, HallwayData downHallway = null)
        {
            this.position = position;
            this.type = type;

            exitHallways[(int)ExitHallwayDirection.Up] = upHallway;
            exitHallways[(int)ExitHallwayDirection.Left] = leftHallway;
            exitHallways[(int)ExitHallwayDirection.Right] = rightHallway;
            exitHallways[(int)ExitHallwayDirection.Down] = downHallway;
        }

        /// <summary>
        /// 모든 복도가 연결되어 있는지 확인
        /// </summary>
        public bool IsFullHallway
        {
            get
            {
                for (int i = 0; i < exitHallways.Length; i++)
                {
                    if (exitHallways[i] == null) { return false; }
                }
                return true;
            }
        }

        /// <summary>
        /// 빈 복도 방향 중 하나를 랜덤하게 선택
        /// </summary>
        /// <returns>빈 복도의 인덱스 (모든 복도가 가득 찬 경우 -1)</returns>
        public int GetRandomEmptyHallwayIndex()
        {
            if (IsFullHallway) return -1;

            while (true)
            {
                int randomIndex = Random.Range(0, 4);
                if (exitHallways[randomIndex] == null) { return randomIndex; }
            }
        }

        /// <summary>
        /// 대상 방과 복도를 연결합니다.
        /// 양방향으로 복도를 생성하여 서로 연결합니다.
        /// </summary>
        /// <param name="targetRoom">연결할 대상 방</param>
        /// <param name="tiles">복도에 사용할 타일 배열</param>
        /// <param name="direction">복도 방향</param>
        public void ConnectHallway(RoomData targetRoom, TileData[] tiles, ExitHallwayDirection direction)
        {
            if (exitHallways[(int)direction] == null)
            {
                // 현재 방에서 대상 방으로 가는 복도 생성
                HallwayData goHallway = new(tiles, targetRoom);
                exitHallways[(int)direction] = goHallway;
                goHallway.linkedRoom = targetRoom;

                // 역방향 타일 배열 생성 (복도 순서 반전)
                TileData[] backHallwayTiles = new TileData[tiles.Length];
                for (int i = 0; i < goHallway.tiles.Length; i++)
                {
                    backHallwayTiles[i] = goHallway.tiles[goHallway.tiles.Length - 1 - i];
                }

                // 대상 방에서 현재 방으로 가는 복도 생성
                HallwayData backHallway = new(backHallwayTiles, this);
                targetRoom.exitHallways[3 - (int)direction] = backHallway;
                backHallway.linkedRoom = this;
            }
        }

        public HallwayData GetExitHallway(RoomData nextRoom)
        {
            for (int i = 0; i < exitHallways.Length; ++i)
            {
                if (exitHallways[i].linkedRoom == nextRoom)
                    return exitHallways[i];
            }

            return null;
        }
    }

    /// <summary>
    /// 방과 방을 연결하는 복도 정보를 저장하는 클래스
    /// </summary>
    public class HallwayData
    {
        /// <summary>복도를 구성하는 타일 배열</summary>
        public TileData[] tiles;
        
        /// <summary>복도가 연결된 방</summary>
        public RoomData linkedRoom;

        /// <summary>
        /// 복도 데이터 생성자
        /// </summary>
        /// <param name="tiles">복도 타일 배열</param>
        /// <param name="linkedRoom">연결된 방</param>
        public HallwayData(TileData[] tiles, RoomData linkedRoom)
        {
            this.tiles = tiles;
            this.linkedRoom = linkedRoom;
        }
    }

    /// <summary>
    /// 개별 타일의 정보를 저장하는 클래스
    /// </summary>
    public class TileData
    {
        /// <summary>타일의 타입</summary>
        public TileType type;

        /// <summary>
        /// 타일 데이터 생성자
        /// </summary>
        /// <param name="type">타일 타입</param>
        public TileData(TileType type)
        {
            this.type = type;
        }
        
        /// <summary>
        /// 타일 타입을 변경합니다.
        /// </summary>
        /// <param name="newType">새로운 타일 타입</param>
        public void SetType(TileType newType)
        {
            type = newType;
        }
    }
}