using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DarkestGame.Map
{
    public class MapGenerator
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
        /// <returns>생성된 맵 객체</returns>
        public static Map GenerateMap(MapData mapData)
        {
            List<RoomData> newRooms = new();
            List<HallwayData> newHallways = new();
            List<TileData> newTiles = new();

            // 시작점에서 첫 번째 방 생성
            Vector2Int roomPosition = Vector2Int.zero;
            
            // 첫 번째 방 생성
            // currentStandardRoom: 현재 기준 방
            RoomData currentStandardRoom = new RoomData(roomPosition, RoomType.None);
            newRooms.Add(currentStandardRoom);

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
                    RoomData newRoom = new(roomPosition, mapData.GetRandomRoomType());

                    #region 복도 연결 및 타일 생성
                    currentStandardRoom.ConnectHallway(newRoom, GenerateTilesBy(mapData), inDirection);

                    HallwayData newHallway = currentStandardRoom.exitHallways[(int)inDirection];
                    newHallways.Add(newHallway);
                    newHallways.Add(newRoom.exitHallways[(int)outDirection]);

                    for (int i = 0; i < newHallway.tiles.Length; i++) { newTiles.Add(newHallway.tiles[i]); }
                    #endregion

                    newRooms.Add(newRoom);
                    currentStandardRoom = newRoom;
                }
                // 새 위치에 이미 방이 있으면 기존 방과 연결
                else
                {
                    // 새 위치에 있는 기존 방 데이터를 반환
                    RoomData overlappedRoom = GetOverlappedRoomData(newRooms, roomPosition);

                    #region 복도 연결 및 타일 생성
                    currentStandardRoom.ConnectHallway(overlappedRoom, GenerateTilesBy(mapData), inDirection);

                    HallwayData newHallway = currentStandardRoom.exitHallways[(int)inDirection];
                    newHallways.Add(newHallway);
                    newHallways.Add(overlappedRoom.exitHallways[(int)outDirection]);

                    for (int i = 0; i < newHallway.tiles.Length; i++) { newTiles.Add(newHallway.tiles[i]); }
                    #endregion

                    currentStandardRoom = overlappedRoom;
                }
            }

            return new Map(mapData, newRooms, newHallways, newTiles);
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

                if (rooms[i].position.x == roomPosition.x && rooms[i].position.y == roomPosition.y)
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

                if (rooms[i].position.x == roomPosition.x && rooms[i].position.y == roomPosition.y)
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
        /// <returns>생성된 타일 배열</returns>
        static TileData[] GenerateTilesBy(MapData mapData)
        {
            TileData[] tiles = new TileData[mapData.TileCount];

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new(mapData.GetRandomTileType());
            }

            return tiles;
        }
    }
}