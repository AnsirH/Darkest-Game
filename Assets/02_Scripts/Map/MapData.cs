using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkestLike.InDungeon.BattleSystem;

namespace DarkestLike.Map
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
    public class MapData
    {
        MapSOData mapSOData;
        [SerializeField] List<RoomData> rooms = new();
        [SerializeField] List<HallwayData> hallways = new();
        [SerializeField] List<TileData> tiles = new();

        public MapSOData MapSOData => mapSOData;
        public List<RoomData> Rooms => rooms;
        public List<HallwayData> Hallways => hallways;
        public List<TileData> Tiles => tiles;

        public int RoomCount => rooms.Count;
        public int HallwayCount => hallways.Count;
        public int TileCount => mapSOData.TileCount;

        /// <summary>맵의 시작 방</summary>
        public RoomData StartRoom { get; private set; }

        /// <summary>
        /// 맵 생성자
        /// </summary>
        /// <param name="mapData">맵 데이터</param>
        /// <param name="rooms">방 목록</param>
        /// <param name="hallways">복도 목록</param>
        /// <param name="tiles">타일 목록</param>
        public MapData(MapSOData mapData, List<RoomData> rooms, List<HallwayData> hallways, List<TileData> tiles)
        {
            this.mapSOData = mapData;
            this.rooms = rooms;
            this.hallways = hallways;
            this.tiles = tiles;

            StartRoom = this.rooms[0];
        }
    }

    /// <summary>
    /// 방의 위치, 타입, 다리 정보를 저장하는 클래스
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public Vector2Int Position { get; private set; }

        public RoomType RoomType { get; private set; }

        public HallwayData[] ExitHallways { get; private set; } = new HallwayData[4];

        private EnemyGroup enemyGroup;

        public string SceneName { get; private set; }

        public EnemyGroup EnemyGroup => enemyGroup;

        public bool HasEnemies => enemyGroup != null && enemyGroup.Enemies.Count > 0;

        public bool IsBattleTile => RoomType == RoomType.Monster || RoomType == RoomType.MonsterAndItem;
        
        public List<HallwayData> ValidExitHallway
        {
            get
            {
                List<HallwayData> result = new();
                for (int i = 0; i < ExitHallways.Length; ++i)
                {
                    if (ExitHallways[i] != null)
                        result.Add(ExitHallways[i]);
                }
                return result;
            }
        }

        #region 방 데이터 생성자
        /// <summary>
        /// 방 데이터 생성자
        /// </summary>
        /// <param name="position">방의 위치</param>
        /// <param name="type">방의 타입</param>
        /// <param name="leftHallway">좌측 복도 (선택사항)</param>
        /// <param name="rightHallway">우측 복도 (선택사항)</param>
        /// <param name="upHallway">상단 복도 (선택사항)</param>
        /// <param name="downHallway">하단 복도 (선택사항)</param>
        /// <param name="enemyGroup">적 그룹 (선택사항)</param>
        public RoomData(Vector2Int position, RoomType type,
            HallwayData leftHallway = null,
            HallwayData rightHallway = null,
            HallwayData upHallway = null,
            HallwayData downHallway = null,
            EnemyGroup enemyGroup = null)
        {
            this.Position = position;
            this.RoomType = type;
            this.enemyGroup = enemyGroup;

            ExitHallways[(int)ExitHallwayDirection.Up] = upHallway;
            ExitHallways[(int)ExitHallwayDirection.Left] = leftHallway;
            ExitHallways[(int)ExitHallwayDirection.Right] = rightHallway;
            ExitHallways[(int)ExitHallwayDirection.Down] = downHallway;
        }
        #endregion

        /// <summary>
        /// 모든 복도가 연결되어 있는지 확인
        /// </summary>
        public bool IsFullHallway
        {
            get
            {
                for (int i = 0; i < ExitHallways.Length; i++)
                {
                    if (ExitHallways[i] == null) { return false; }
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
                if (ExitHallways[randomIndex] == null) { return randomIndex; }
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
            if (ExitHallways[(int)direction] == null)
            {
                // 현재 방에서 대상 방으로 가는 복도 생성
                HallwayData goHallway = new(tiles, targetRoom);
                ExitHallways[(int)direction] = goHallway;
                goHallway.SetExitRoom(targetRoom);

                // 역방향 타일 배열 생성 (복도 순서 반전)
                TileData[] backHallwayTiles = new TileData[tiles.Length];
                for (int i = 0; i < goHallway.Tiles.Length; i++)
                {
                    backHallwayTiles[i] = goHallway.Tiles[goHallway.Tiles.Length - 1 - i];
                }

                // 대상 방에서 현재 방으로 가는 복도 생성
                HallwayData backHallway = new(backHallwayTiles, this);
                targetRoom.ExitHallways[3 - (int)direction] = backHallway;
                backHallway.SetExitRoom(this);
            }
        }

        /// <summary>
        /// 다음으로 이동할 방으로 가는 복도를 반환한다.
        /// </summary>
        /// <param name="nextRoom"></param>
        /// <returns></returns>
        public HallwayData GetExitHallway(RoomData nextRoom)
        {
            for (int i = 0; i < ExitHallways.Length; ++i)
            {
                if (ExitHallways[i] != null && ExitHallways[i].ExitRoom == nextRoom)
                    return ExitHallways[i];
            }

            return null;
        }

        /// <summary>
        /// 전환할 씬 이름 설정
        /// </summary>
        /// <param name="sceneName"></param>
        public void SetSceneName(string sceneName) { SceneName = sceneName; }

        /// <summary>
        /// targetRoom이 이동 가능한 방인지 확인
        /// </summary>
        /// <param name="targetRoom"></param>
        /// <returns></returns>
        public bool CheckIsMoveableRoom(RoomData targetRoom)
        {
            for (int i = 0; i < ExitHallways.Length; ++i)
            {
                if (ExitHallways[i] != null && ExitHallways[i].ExitRoom == targetRoom)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 방과 방을 연결하는 복도 정보를 저장하는 클래스
    /// </summary>
    public class HallwayData
    {
        /// <summary>복도를 구성하는 타일 배열</summary>
        public TileData[] Tiles { get; private set; }

        /// <summary>복도가 연결된 방</summary>
        public RoomData ExitRoom { get; private set; }

        public string SceneName { get; private set; }

        /// <summary>
        /// 복도 데이터 생성자
        /// </summary>
        /// <param name="tiles">복도 타일 배열</param>
        /// <param name="exitRoom">연결된 방</param>
        public HallwayData(TileData[] tiles, RoomData exitRoom)
        {
            Tiles = tiles;
            ExitRoom = exitRoom;
        }

        public void SetExitRoom(RoomData roomData) { ExitRoom = roomData; }
        public void SetSceneName(string sceneName) { SceneName = sceneName; }
        public int GetTileIndex(TileData tileData)
        {
            for (int i = 0; i < Tiles.Length; ++i)
            {
                if (tileData == Tiles[i])
                    return i;
            }
            return -1;
        }

        public TileData GetNextTile(TileData currentTile)
        {
            return Tiles[GetTileIndex(currentTile) + 1];
        }

        public TileData GetPreviousTile(TileData currentTile)
        {
            return Tiles[GetTileIndex(currentTile) - 1];
        }
    }

    /// <summary>
    /// 개별 타일의 정보를 저장하는 클래스
    /// </summary>
    public class TileData
    {
        public TileType type;
        public EnemyGroup EnemyGroup { get; private set; }
        public Vector3 Position { get; private set; }

        public TileData(TileType type, EnemyGroup enemyGroup)
        {
            this.type = type;
            EnemyGroup = enemyGroup;
        }

        public void SetType(TileType newType)
        {
            type = newType;
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }
        
        /// <summary>
        /// 적 그룹 설정
        /// </summary>
        /// <param name="enemyGroup">설정할 적 그룹</param>
        public void SetEnemyGroup(EnemyGroup enemyGroup) { EnemyGroup = enemyGroup; }
    }
}