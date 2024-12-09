using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestGame.Map
{
    public class MapDrawer : MonoBehaviour
    {
        public RectTransform roomButtonRect;
        public RectTransform tileButtonRect;

        public float roomButtonOffset = 30.0f;
        public float tileButtonOffset = 10.0f;

        Dictionary<RoomData, GameObject> roomButtons = new();
        Dictionary<TileData, GameObject> tileButtons = new();

        private void Awake()
        {
            roomButtonOffset = roomButtonRect.rect.width * 0.5f;
            tileButtonOffset = tileButtonRect.rect.width * 0.5f;
        }

        public void GenerateButtons(Map map)
        {
            if (map == null) return;
            roomButtons.Clear();
            tileButtons.Clear();

            // 1. 방 생성( 이미 있으면 생성 x )
            // 2. 연결된 다리 생성( 이미 있으면 생성 x )
            // 3. 연결된 다리에 연결된 방 생성( 이미 있으면 생성 x )

            Vector2 nextRoomPosition = Vector2.zero;
            float distanceBetweenRooms = roomButtonOffset * 2 + map.TileCount * 2 * tileButtonOffset;
            for (int i = 0; i < map.Rooms.Count; i++)
            {
                RoomData newRoom = map.Rooms[i];
                nextRoomPosition = newRoom.position * (int)distanceBetweenRooms;
                if (!roomButtons.ContainsKey(newRoom))
                {
                    CreateRoomButton(newRoom, nextRoomPosition);
                }

                for (int j = 0; j < 4; j++)
                {
                    if (newRoom.exitBridges[j] != null && !tileButtons.ContainsKey(newRoom.exitBridges[j].tiles[0]))
                    {
                        Vector2 nextButtonPosition = nextRoomPosition;
                        Vector2 direction = j switch
                        {
                            0 => Vector2.up,
                            1 => Vector2.left,
                            2 => Vector2.right,
                            3 => Vector2.down,
                            _ => throw new System.NotImplementedException()
                        };

                        nextButtonPosition += direction * roomButtonOffset;

                        BridgeData newBridge = newRoom.exitBridges[j];

                        for (int k = 0; k < newBridge.tiles.Length; k++)
                        {
                            nextButtonPosition += direction * tileButtonOffset;

                            CreateTileButton(newBridge.tiles[k], nextButtonPosition);

                            nextButtonPosition += direction * tileButtonOffset;
                        }
                    }
                }
            }

        }

        private void CreateRoomButton(RoomData roomData, Vector2 buttonPosition)
        {
            GameObject newRoomButtonObj = Instantiate(roomButtonRect.gameObject, transform);
            roomButtons.Add(roomData, newRoomButtonObj);

            RectTransform newRoomButtonRect = newRoomButtonObj.GetComponent<RectTransform>();
            newRoomButtonRect.anchoredPosition = buttonPosition;
        }

        private void CreateTileButton(TileData tileData, Vector2 buttonPosition)
        {
            GameObject newTileObj = Instantiate(tileButtonRect.gameObject, transform);
            tileButtons.Add(tileData, newTileObj);

            RectTransform tileRect = newTileObj.GetComponent<RectTransform>();
            tileRect.anchoredPosition = buttonPosition;
        }
    }
}