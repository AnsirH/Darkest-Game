using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Random;

namespace DarkestGame.Map
{

    public class MapGenerator : MonoBehaviour
    {
        // �� ���� ����
        // 1. ���� �߾ӿ� �ϳ� �����
        // 2. 4���� �� �ϳ��� �����Ѵ�.
        // 2-1. ��������� �ʰ� �긴���� �����Ѵ�. ���� �ʿ� �긴���� �����ϰ� �긴���� ������ ���� �����Ѵ�.
        // 2-2. ���� ������ �ش� ���� �긴���� ���� ���� ����Ű�� �ִ��� Ȯ���Ѵ�.
        // 2-2-1. �긴���� ���� ���� ����Ű�� ������ ���� �� �߿��� ���� ���� ������ �ٸ� ���� �����Ѵ�.
        // 2-2-2. �긴���� ���� ���� ����Ű�� ���� ������ �긴�� ���� �� �ٸ� ���� �����Ѵ�.

        [Header("�� Drawer")]
        public MapDrawer mapDrawer;

        /// <summary> ���� �� </summary>
        public RoomData startRoom;

        /// <summary> ��� �� �迭 </summary>
        public Map map;

        private const int distanceBetweenRoom = 1;

        public void OnGUI()
        {
            if (GUILayout.Button("Generate Map"))
            {
                if (map != null) { map = null; }
                map = GenerateRooms(8, 5);
            }

            if (GUILayout.Button("MapDraw"))
            {
                if (map == null) { return; }
                mapDrawer.GenerateButtons(map);
            }
        }

        public Map GenerateRooms(int roomCount, int tileCount)
        {
            List<RoomData> newRooms = new();
            List<BridgeData> newBridges = new();
            List<TileData> newTiles = new();

            // ���� �� ����
            Vector2Int roomPosition = Vector2Int.zero;
            startRoom = new RoomData(roomPosition, RoomType.None);
            newRooms.Add(startRoom);

            RoomData currentStandardRoom = startRoom;

            while (newRooms.Count < roomCount)
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
                    RoomData newRoom = new(roomPosition, RoomType.None);

                    currentStandardRoom.ConnectBridge(newRoom, tileCount, inDirection);

                    BridgeData newBridge = currentStandardRoom.exitBridges[(int)inDirection];
                    newBridges.Add(newBridge);
                    newBridges.Add(newRoom.exitBridges[(int)outDirection]);

                    for (int i = 0; i < newBridge.tiles.Length; i++) { newTiles.Add(newBridge.tiles[i]); }

                    newRooms.Add(newRoom);

                    currentStandardRoom = newRoom;
                } // ������ ��ġ�� ���� ���� ��

                else
                {
                    // ���� ��ġ�� ��ģ �� ����
                    RoomData overlappedRoom = GetOverlappedRoomData(newRooms, roomPosition);

                    currentStandardRoom.ConnectBridge(overlappedRoom, tileCount, inDirection);

                    BridgeData newBridge = currentStandardRoom.exitBridges[(int)inDirection];
                    newBridges.Add(newBridge);
                    newBridges.Add(overlappedRoom.exitBridges[(int)outDirection]);

                    for (int i = 0; i < newBridge.tiles.Length; i++) { newTiles.Add(newBridge.tiles[i]); }

                    currentStandardRoom = overlappedRoom;
                } // ���� �� 
            }

            return new Map(newRooms, newBridges, newTiles);
        }

        /// <summary> �ش� ��ġ�� ���� �����ص� �Ǵ��� Ȯ�� </summary>
        bool CanCreateRoomPosition(List<RoomData> rooms, Vector2Int roomPosition)
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
        RoomData GetOverlappedRoomData(List<RoomData> rooms, Vector2Int roomPosition)
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

        void ConnectBridge(RoomData startRoom, RoomData endRoom, int tileCount, ExitBridgeDirection direction)
        {
            if (startRoom.exitBridges[(int)direction] == null)
            {
                // ���� ��� ����
                BridgeData goBridge = new(tileCount, endRoom);
                startRoom.exitBridges[(int)direction] = goBridge;
                goBridge.linkedRoom = endRoom;

                TileData[] backBridgeTiles = new TileData[tileCount];
                for (int i = 0; i < goBridge.tiles.Length; i++)
                {
                    backBridgeTiles[i] = goBridge.tiles[goBridge.tiles.Length - 1 - i];
                }

                // ���� ��� ����
                BridgeData backBridge = new(backBridgeTiles, startRoom);
                endRoom.exitBridges[3 - (int)direction] = backBridge;
                backBridge.linkedRoom = startRoom;
            }
        }
    }
}