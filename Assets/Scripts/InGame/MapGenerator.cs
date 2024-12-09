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
        // 맵 생성 로직
        // 1. 맵을 중앙에 하나 만든다
        // 2. 4방향 중 하나를 선택한다.
        // 2-1. 비어있으면 맵과 브릿지를 생성한다. 기존 맵에 브릿지를 연결하고 브릿지에 생성한 맵을 연결한다.
        // 2-2. 맵이 있으면 해당 맵의 브릿지가 기존 맵을 가리키고 있는지 확인한다.
        // 2-2-1. 브릿지가 기존 맵을 가리키고 있으면 현재 방 중에서 기존 방을 제외한 다른 방을 선택한다.
        // 2-2-2. 브릿지가 기존 맵을 가리키고 있지 않으면 브릿지 연결 후 다른 방을 선택한다.

        [Header("맵 Drawer")]
        public MapDrawer mapDrawer;

        /// <summary> 시작 방 </summary>
        public RoomData startRoom;

        /// <summary> 모든 방 배열 </summary>
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

            // 시작 방 생성
            Vector2Int roomPosition = Vector2Int.zero;
            startRoom = new RoomData(roomPosition, RoomType.None);
            newRooms.Add(startRoom);

            RoomData currentStandardRoom = startRoom;

            while (newRooms.Count < roomCount)
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
                    RoomData newRoom = new(roomPosition, RoomType.None);

                    currentStandardRoom.ConnectBridge(newRoom, tileCount, inDirection);

                    BridgeData newBridge = currentStandardRoom.exitBridges[(int)inDirection];
                    newBridges.Add(newBridge);
                    newBridges.Add(newRoom.exitBridges[(int)outDirection]);

                    for (int i = 0; i < newBridge.tiles.Length; i++) { newTiles.Add(newBridge.tiles[i]); }

                    newRooms.Add(newRoom);

                    currentStandardRoom = newRoom;
                } // 지정한 위치에 방이 없을 때

                else
                {
                    // 지정 위치에 겹친 방 정보
                    RoomData overlappedRoom = GetOverlappedRoomData(newRooms, roomPosition);

                    currentStandardRoom.ConnectBridge(overlappedRoom, tileCount, inDirection);

                    BridgeData newBridge = currentStandardRoom.exitBridges[(int)inDirection];
                    newBridges.Add(newBridge);
                    newBridges.Add(overlappedRoom.exitBridges[(int)outDirection]);

                    for (int i = 0; i < newBridge.tiles.Length; i++) { newTiles.Add(newBridge.tiles[i]); }

                    currentStandardRoom = overlappedRoom;
                } // 있을 때 
            }

            return new Map(newRooms, newBridges, newTiles);
        }

        /// <summary> 해당 위치에 방을 생성해도 되는지 확인 </summary>
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

        /// <summary> 해당 위치에 겹치는 방을 반환 </summary>
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
                // 가는 통로 생성
                BridgeData goBridge = new(tileCount, endRoom);
                startRoom.exitBridges[(int)direction] = goBridge;
                goBridge.linkedRoom = endRoom;

                TileData[] backBridgeTiles = new TileData[tileCount];
                for (int i = 0; i < goBridge.tiles.Length; i++)
                {
                    backBridgeTiles[i] = goBridge.tiles[goBridge.tiles.Length - 1 - i];
                }

                // 오는 통로 생성
                BridgeData backBridge = new(backBridgeTiles, startRoom);
                endRoom.exitBridges[3 - (int)direction] = backBridge;
                backBridge.linkedRoom = startRoom;
            }
        }
    }
}