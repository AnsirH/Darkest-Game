using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DarkestGame.Map
{
    public class MapGenerator
    {
        // �� ���� ����
        // 1. ���� �߾ӿ� �ϳ� �����
        // 2. 4���� �� �ϳ��� �����Ѵ�.
        // 2-1. ��������� �ʰ� �긴���� �����Ѵ�. ���� �ʿ� �긴���� �����ϰ� �긴���� ������ ���� �����Ѵ�.
        // 2-2. ���� ������ �ش� ���� �긴���� ���� ���� ����Ű�� �ִ��� Ȯ���Ѵ�.
        // 2-2-1. �긴���� ���� ���� ����Ű�� ������ ���� �� �߿��� ���� ���� ������ �ٸ� ���� �����Ѵ�.
        // 2-2-2. �긴���� ���� ���� ����Ű�� ���� ������ �긴�� ���� �� �ٸ� ���� �����Ѵ�.

        private const int distanceBetweenRoom = 1;

        public static Map GenerateMap(MapData mapData)
        {
            List<RoomData> newRooms = new();
            List<BridgeData> newBridges = new();
            List<TileData> newTiles = new();

            // ���� �� ����
            Vector2Int roomPosition = Vector2Int.zero;
            
            RoomData currentStandardRoom = new RoomData(roomPosition, RoomType.None);
            newRooms.Add(currentStandardRoom);

            while (newRooms.Count < mapData.RoomCount)
            {
                // ���� ����
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

                // �� ��ġ �̵�
                roomPosition += offset;

                if (CanCreateRoomPosition(newRooms, roomPosition))
                {
                    // �� ����
                    RoomData newRoom = new(roomPosition, mapData.GetRandomRoomType());

                    #region �ٸ� ���� �� Ÿ�� ����(����)
                    currentStandardRoom.ConnectBridge(newRoom, GenerateTilesBy(mapData), inDirection);

                    BridgeData newBridge = currentStandardRoom.exitBridges[(int)inDirection];
                    newBridges.Add(newBridge);
                    newBridges.Add(newRoom.exitBridges[(int)outDirection]);

                    for (int i = 0; i < newBridge.tiles.Length; i++) { newTiles.Add(newBridge.tiles[i]); }
                    #endregion

                    newRooms.Add(newRoom);

                    currentStandardRoom = newRoom;
                } // ������ ��ġ�� ���� ���� ��

                else
                {
                    // ���� ��ġ�� ��ģ �� ����
                    RoomData overlappedRoom = GetOverlappedRoomData(newRooms, roomPosition);

                    #region �ٸ� ���� �� Ÿ�� ����(����)
                    currentStandardRoom.ConnectBridge(overlappedRoom, GenerateTilesBy(mapData), inDirection);

                    BridgeData newBridge = currentStandardRoom.exitBridges[(int)inDirection];
                    newBridges.Add(newBridge);
                    newBridges.Add(overlappedRoom.exitBridges[(int)outDirection]);

                    for (int i = 0; i < newBridge.tiles.Length; i++) { newTiles.Add(newBridge.tiles[i]); }
                    #endregion

                    currentStandardRoom = overlappedRoom;
                } // ������ ��ġ�� ���� ���� �� 
            }

            return new Map(mapData, newRooms, newBridges, newTiles);
        }

        /// <summary> �ش� ��ġ�� ���� �����ص� �Ǵ��� Ȯ�� </summary>
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

        /// <summary> �ش� ��ġ�� ��ġ�� ���� ��ȯ </summary>
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