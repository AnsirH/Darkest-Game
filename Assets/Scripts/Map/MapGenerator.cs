using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DarkestGame.Map
{
    public class MapGenerator
    {
        // 맵 생성 로직
        // 1. 맵을 중앙에 하나 만든다
        // 2. 4방향 중 하나를 선택한다.
        // 2-1. 비어있으면 맵과 브릿지를 생성한다. 기존 맵에 브릿지를 연결하고 브릿지에 생성한 맵을 연결한다.
        // 2-2. 맵이 있으면 해당 맵의 브릿지가 기존 맵을 가리키고 있는지 확인한다.
        // 2-2-1. 브릿지가 기존 맵을 가리키고 있으면 현재 방 중에서 기존 방을 제외한 다른 방을 선택한다.
        // 2-2-2. 브릿지가 기존 맵을 가리키고 있지 않으면 브릿지 연결 후 다른 방을 선택한다.

        private const int distanceBetweenRoom = 1;

        public static Map GenerateMap(MapData mapData)
        {
            List<RoomData> newRooms = new();
            List<BridgeData> newBridges = new();
            List<TileData> newTiles = new();

            // 시작 방 생성
            Vector2Int roomPosition = Vector2Int.zero;
            
            RoomData currentStandardRoom = new RoomData(roomPosition, RoomType.None);
            newRooms.Add(currentStandardRoom);

            while (newRooms.Count < mapData.RoomCount)
            {
                // 방향 설정
                int direction = currentStandardRoom.GetRandomEmptyBridgeIndex();

                ExitBridgeDirection inDirection = (ExitBridgeDirection)direction;
                ExitBridgeDirection outDirection = (ExitBridgeDirection)(3 - direction);

                Vector2Int offset = inDirection switch
                {
                    ExitBridgeDirection.Up => Vector2Int.up,
                    ExitBridgeDirection.Left => Vector2Int.left,
                    ExitBridgeDirection.Right => Vector2Int.right,
                    ExitBridgeDirection.Down => Vector2Int.down,
                    _ => throw new System.NotImplementedException()
                } * distanceBetweenRoom;

                // 방 위치 이동
                roomPosition += offset;

                if (CanCreateRoomPosition(newRooms, roomPosition))
                {
                    // 방 생성
                    RoomData newRoom = new(roomPosition, mapData.GetRandomRoomType());

                    #region 다리 연결 및 타일 생성(공통)
                    currentStandardRoom.ConnectBridge(newRoom, GenerateTilesBy(mapData), inDirection);

                    BridgeData newBridge = currentStandardRoom.exitBridges[(int)inDirection];
                    newBridges.Add(newBridge);
                    newBridges.Add(newRoom.exitBridges[(int)outDirection]);

                    for (int i = 0; i < newBridge.tiles.Length; i++) { newTiles.Add(newBridge.tiles[i]); }
                    #endregion

                    newRooms.Add(newRoom);

                    currentStandardRoom = newRoom;
                } // 지정한 위치에 방이 없을 때

                else
                {
                    // 지정 위치에 겹친 방 정보
                    RoomData overlappedRoom = GetOverlappedRoomData(newRooms, roomPosition);

                    #region 다리 연결 및 타일 생성(공통)
                    currentStandardRoom.ConnectBridge(overlappedRoom, GenerateTilesBy(mapData), inDirection);

                    BridgeData newBridge = currentStandardRoom.exitBridges[(int)inDirection];
                    newBridges.Add(newBridge);
                    newBridges.Add(overlappedRoom.exitBridges[(int)outDirection]);

                    for (int i = 0; i < newBridge.tiles.Length; i++) { newTiles.Add(newBridge.tiles[i]); }
                    #endregion

                    currentStandardRoom = overlappedRoom;
                } // 지정한 위치에 방이 있을 때 
            }

            return new Map(mapData, newRooms, newBridges, newTiles);
        }

        /// <summary> 해당 위치에 방을 생성해도 되는지 확인 </summary>
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

        /// <summary> 해당 위치에 겹치는 방을 반환 </summary>
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