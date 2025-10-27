using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DarkestLike.InDungeon.BattleSystem;

namespace DarkestLike.Map
{
    public static class MapGenerator
    {
        /// <summary>
        /// 던전 맵 생성 알고리즘
        /// 1. 시작점(0,0)에 첫 번째 방을 생성
        /// 2. 현재 방에서 빈 다리 방향 중 하나를 랜덤하게 선택
        /// 3. 선택된 방향으로 이동하여 새 위치 계산
        /// 4. 새 위치에 방이 있는지 확인
        ///    - 없으면: 새 방을 생성하고 다리로 연결
        ///    - 있으면: 기존 방과 다리로 연결
        /// 5. 새로 생성된 방(또는 기존 방)을 현재 방으로 설정
        /// 6. 지정된 방 개수만큼 반복
        /// </summary>

        /// <summary>방과 방 사이의 거리 (격자 단위) </summary
        private const int distanceBetweenRoom = 1;

        /// <summary>
        /// 맵 데이터를 기반으로 던전 맵을 생성합니다.
        /// </summary>
        /// <param name="mapData">맵 생성에 사용할 데이터</param>
        /// <param name="dungeonLevel">던전 레벨</param>
        /// <returns>생성된 맵 객체</returns>
        public static MapData GenerateMap(MapSOData mapData, int dungeonLevel)
        {
            List<RoomData> newRooms = new();
            List<HallwayData> newHallways = new();
            List<TileData> newTiles = new();

            // 시작점에서 첫 번째 방 생성
            Vector2Int roomPosition = Vector2Int.zero;
            
            // 첫 번째 방 생성
            // currentStandardRoom: 현재 기준 방
            RoomType firstRoomType = RoomType.None;
            EnemyGroup firstRoomEnemyGroup = GenerateEnemyGroupForRoom(mapData, dungeonLevel, firstRoomType);
            RoomData currentStandardRoom = new RoomData(roomPosition, firstRoomType, enemyGroup: firstRoomEnemyGroup);
            newRooms.Add(currentStandardRoom);

            // 복도 연결 및 타일 생성 메서드
            void LinkCorridorsAndPlaceTiles(RoomData targetRoom, 
                ExitHallwayDirection inDirection, ExitHallwayDirection outDirection)
            {
                currentStandardRoom.ConnectHallway(targetRoom, GenerateTilesBy(mapData, dungeonLevel), inDirection);

                HallwayData newHallway = targetRoom.ExitHallways[(int)outDirection];
                newHallway.SetSceneName("Hallway Scene");
                newHallways.Add(newHallway);

                newHallway = currentStandardRoom.ExitHallways[(int)inDirection];
                newHallway.SetSceneName("Hallway Scene");
                newHallways.Add(newHallway);

                for (int i = 0; i < newHallway.Tiles.Length; i++) { newTiles.Add(newHallway.Tiles[i]); }
            }

            // 지정된 방 개수만큼 방을 생성
            while (newRooms.Count < mapData.RoomCount)
            {
                // 현재 방에서 빈 복도 방향 중 하나를 랜덤하게 선택
                int direction = currentStandardRoom.GetRandomEmptyHallwayIndex();

                ExitHallwayDirection inDirection = (ExitHallwayDirection)direction;
                ExitHallwayDirection outDirection = (ExitHallwayDirection)(3 - direction);

                // 선택된 방향으로의 오프셋 계산
                Vector2Int offset = inDirection switch
                {
                    ExitHallwayDirection.Up => Vector2Int.up,
                    ExitHallwayDirection.Left => Vector2Int.left,
                    ExitHallwayDirection.Right => Vector2Int.right,
                    ExitHallwayDirection.Down => Vector2Int.down,
                    _ => throw new System.NotImplementedException()
                } * distanceBetweenRoom;

                // 새 방 위치 계산
                roomPosition += offset;

                // 새 위치에 방이 없으면 새 방 생성
                if (CanCreateRoomPosition(newRooms, roomPosition))
                {
                    RoomType roomType = mapData.GetRandomRoomType();
                    EnemyGroup enemyGroup = GenerateEnemyGroupForRoom(mapData, dungeonLevel, roomType);
                    RoomData newRoom = new(roomPosition, roomType, enemyGroup: enemyGroup);
                    newRoom.SetSceneName("Room Scene");
                    LinkCorridorsAndPlaceTiles(newRoom, inDirection, outDirection);                    
                    newRooms.Add(newRoom);
                    currentStandardRoom = newRoom;
                }
                // 새 위치에 이미 방이 있으면 기존 방과 연결
                else
                {
                    // 새 위치에 있는 기존 방 데이터를 반환
                    RoomData overlappedRoom = GetOverlappedRoomData(newRooms, roomPosition);
                    LinkCorridorsAndPlaceTiles(overlappedRoom, inDirection, outDirection);                    
                    currentStandardRoom = overlappedRoom;
                }
            }
            return new MapData(mapData, newRooms, newHallways, newTiles);
        }

        /// <summary>
        /// 해당 위치에 새 방을 생성할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="rooms">기존 방 목록</param>
        /// <param name="roomPosition">확인할 방 위치</param>
        /// <returns>새 방 생성 가능 여부</returns>
        static bool CanCreateRoomPosition(List<RoomData> rooms, Vector2Int roomPosition)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i] == null) continue;

                if (rooms[i].Position.x == roomPosition.x && rooms[i].Position.y == roomPosition.y)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 해당 위치에 있는 기존 방 데이터를 반환합니다.
        /// </summary>
        /// <param name="rooms">방 목록</param>
        /// <param name="roomPosition">찾을 방 위치</param>
        /// <returns>해당 위치의 방 데이터 (없으면 null)</returns>
        static RoomData GetOverlappedRoomData(List<RoomData> rooms, Vector2Int roomPosition)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i] == null) continue;

                if (rooms[i].Position.x == roomPosition.x && rooms[i].Position.y == roomPosition.y)
                {
                    return rooms[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 맵 데이터를 기반으로 복도용 타일들을 생성합니다.
        /// </summary>
        /// <param name="mapData">타일 생성에 사용할 맵 데이터</param>
        /// <param name="dungeonLevel">던전 레벨</param>
        /// <returns>생성된 타일 배열</returns>
        static TileData[] GenerateTilesBy(MapSOData mapData, int dungeonLevel)
        {
            TileData[] tiles = new TileData[mapData.TileCount];

            for (int i = 0; i < tiles.Length; i++)
            {
                TileType tileType = mapData.GetRandomTileType();
                EnemyGroup enemyGroup = GenerateEnemyGroupForTile(mapData, dungeonLevel, tileType);
                tiles[i] = new(tileType, enemyGroup);
            }

            return tiles;
        }

        /// <summary>
        /// 방 타입에 따른 적 그룹을 생성합니다.
        /// </summary>
        /// <param name="mapData">맵 데이터</param>
        /// <param name="dungeonLevel">던전 레벨</param>
        /// <param name="roomType">방 타입</param>
        /// <returns>생성된 적 그룹</returns>
        private static EnemyGroup GenerateEnemyGroupForRoom(MapSOData mapData, int dungeonLevel, RoomType roomType)
        {
            // 몬스터가 있는 방 타입만 적 생성
            if (roomType == RoomType.Monster || roomType == RoomType.MonsterAndItem)
            {
                return EnemyGroupGenerator.GenerateEnemyGroupForRoom(mapData, dungeonLevel);
            }
            
            return null;
        }

        /// <summary>
        /// 타일 타입에 따른 적 그룹을 생성합니다.
        /// </summary>
        /// <param name="mapData">맵 데이터</param>
        /// <param name="dungeonLevel">던전 레벨</param>
        /// <param name="tileType">타일 타입</param>
        /// <returns>생성된 적 그룹</returns>
        private static EnemyGroup GenerateEnemyGroupForTile(MapSOData mapData, int dungeonLevel, TileType tileType)
        {
            // 몬스터 타일만 적 생성
            if (tileType == TileType.Monster)
            {
                return EnemyGroupGenerator.GenerateEnemyGroupForTile(mapData, dungeonLevel);
            }
            
            return null;
        }
    }
}