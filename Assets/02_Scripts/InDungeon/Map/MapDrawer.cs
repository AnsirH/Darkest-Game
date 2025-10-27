using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.Map
{
    /// 맵 데이터를 기반으로 UI 버튼들을 생성하고 배치하는 클래스
    /// 방과 복도의 타일들을 화면에 표시합니다.
    public class MapDrawer : MonoBehaviour
    {
        [Header("references")]
        [SerializeField] RectTransform mapDrawArea;

        /// <summary>방 버튼의 RectTransform 프리팹</summary>
        [SerializeField] MapTileRoomButton roomMapTilePrefab;

        /// <summary>타일 버튼의 RectTransform 프리팹</summary>
        [SerializeField] MapTileUI tileMapTilePrefab;

        [Header("variables")]
        /// <summary>방 버튼 간의 오프셋 거리</summary>
        [SerializeField] float roomMapTileSize = 70.0f;

        /// <summary>타일 버튼 간의 오프셋 거리</summary>
        [SerializeField] float tileMapTileSize = 20.0f;

        // variables
        Dictionary<RoomData, MapTileRoomButton> roomMapTiles = new();
        Dictionary<TileData, MapTileUI> tileMapTiles = new();
        MapTileUI currentRoomMT;
        MapTileUI currentTileMT;
        List<MapTileUI> highlightedNearRoomTiles = new();

        /// <summary>
        /// 맵 데이터를 기반으로 방과 복도 타일들의 UI 버튼을 생성합니다.
        /// </summary>
        /// <param name="map">생성할 맵 데이터</param>
        public void GenerateButtons(MapData map)
        {
            if (map == null) return;
            roomMapTiles.Clear();
            tileMapTiles.Clear();

            // 1. 방 생성 (이미 생성된 방 제외)
            // 2. 복도의 다른 방 (이미 생성된 방 제외)
            // 3. 복도의 다른방으로 가는 복도 (이미 생성된 방 제외)

            Vector2 mapRect = mapDrawArea.rect.size;
            Vector2 nextRoomPosition = Vector2.zero;
            float distanceBetweenRooms = roomMapTileSize + map.TileCount * tileMapTileSize;
            
            // 모든 방을 순회하며 UI 버튼 생성
            for (int i = 0; i < map.Rooms.Count; i++)
            {
                RoomData newRoom = map.Rooms[i];
                nextRoomPosition = newRoom.Position * (int)distanceBetweenRooms;
                if (Mathf.Abs(nextRoomPosition.x) * 2 > mapRect.x)
                    mapRect.x = Mathf.Abs(nextRoomPosition.x) * 2;
                if (Mathf.Abs(nextRoomPosition.y) * 2 > mapRect.y)
                    mapRect.y = Mathf.Abs(nextRoomPosition.y) * 2;

                // 방 버튼이 아직 생성되지 않았다면 생성
                if (!roomMapTiles.ContainsKey(newRoom))
                {
                    CreateRoomMapTile(newRoom, nextRoomPosition);
                }

                // 4방향 복도 확인 및 타일 버튼 생성
                for (int j = 0; j < 4; j++)
                {
                    if (newRoom.ExitHallways[j] != null && !tileMapTiles.ContainsKey(newRoom.ExitHallways[j].Tiles[0]))
                    {
                        Vector2 nextButtonPosition = nextRoomPosition;
                        
                        // 방향에 따른 벡터 계산
                        Vector2 direction = j switch
                        {
                            0 => Vector2.up,      // 상
                            1 => Vector2.left,    // 좌
                            2 => Vector2.right,   // 우
                            3 => Vector2.down,    // 하
                            _ => throw new System.NotImplementedException()
                        };

                        // 복도 시작 위치로 이동
                        nextButtonPosition += direction * roomMapTileSize * 0.5f;

                        HallwayData newHallway = newRoom.ExitHallways[j];

                        // 복도의 각 타일에 대해 버튼 생성
                        for (int k = 0; k < newHallway.Tiles.Length; k++)
                        {
                            nextButtonPosition += direction * tileMapTileSize * 0.5f;

                            CreateTileButton(newHallway.Tiles[k], nextButtonPosition);

                            nextButtonPosition += direction * tileMapTileSize * 0.5f;
                        }
                    }
                }
            }
            mapDrawArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mapRect.x + distanceBetweenRooms);
            mapDrawArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mapRect.y + distanceBetweenRooms);
            SetCenter();
        }

        /// <summary>
        /// 방 데이터에 대한 UI 버튼을 생성합니다.
        /// </summary>
        /// <param name="roomData">방 데이터</param>
        /// <param name="buttonPosition">버튼 위치</param>
        private void CreateRoomMapTile(RoomData roomData, Vector2 buttonPosition)
        {
            MapTileRoomButton roomUI = Instantiate(roomMapTilePrefab, mapDrawArea);
            roomUI.SetTileSize(Vector2.one * roomMapTileSize);
            roomUI.UpdateImage(roomData.RoomType);
            roomUI.SetRoomData(roomData);
            roomMapTiles.Add(roomData, roomUI);

            roomUI.SetPosition(buttonPosition);
        }

        /// <summary>
        /// 타일 데이터에 대한 UI 버튼을 생성합니다.
        /// </summary>
        /// <param name="tileData">타일 데이터</param>
        /// <param name="buttonPosition">버튼 위치</param>
        private void CreateTileButton(TileData tileData, Vector2 buttonPosition)
        {
            MapTileUI tileUI = Instantiate(tileMapTilePrefab, mapDrawArea);
            tileUI.SetTileSize(Vector2.one * tileMapTileSize);
            tileUI.UpdateImage(tileData.type);
            tileMapTiles.Add(tileData, tileUI);

            tileUI.SetPosition(buttonPosition);
        }

        /// <summary>
        /// 모든 버튼들을 화면 중앙에 배치합니다.
        /// 방 버튼들의 중심점을 계산하여 모든 버튼을 중앙으로 이동시킵니다.
        /// </summary>
        private void SetCenter()
        {
            // 방 버튼들의 중심점 계산
            Vector2 center = Vector2.zero;
            foreach (var roomButton in roomMapTiles)
            {
                center += roomButton.Value.AnchoredPosition;
            }
            center /= roomMapTiles.Count;

            // 중앙으로 이동할 벡터 계산
            Vector2 moveVector = Vector2.zero - center;

            // 모든 방 버튼을 중앙으로 이동
            foreach (var roomButton in roomMapTiles)
            {
                roomButton.Value.SetPosition(roomButton.Value.AnchoredPosition + moveVector);
            }

            // 모든 타일 버튼을 중앙으로 이동
            foreach (var tileButton in tileMapTiles)
            {
                tileButton.Value.SetPosition(tileButton.Value.AnchoredPosition + moveVector);
            }
        }

        public void HighlightRoom(RoomData roomData)
        {
            if (currentRoomMT != null)
                currentRoomMT.ActiveTileHighlight_Color(false);
            if (currentTileMT != null)
                currentTileMT.ActiveTileHighlight_Scale(false);
            if (roomMapTiles.TryGetValue(roomData, out MapTileRoomButton roomMT))
            {
                currentRoomMT = roomMT;
                currentRoomMT.ActiveTileHighlight_Color(true);
            }
        }

        public void HighlightNearRooms(RoomData roomData)
        {
            ClearHighlight();
            foreach (HallwayData hallway in roomData.ValidExitHallway)
                LoopHighlightRoom(hallway.ExitRoom);
        }

        public void LoopHighlightRoom(RoomData roomData)
        {
            if (roomMapTiles.TryGetValue(roomData, out MapTileRoomButton roomMT))
            {
                highlightedNearRoomTiles.Add(roomMT);
                roomMT.ActiveLoopHighlight_Color();
            }
        }

        public void ClearHighlight()
        {
            if (highlightedNearRoomTiles.Count == 0) return;
            while (highlightedNearRoomTiles.Count > 0)
            {
                highlightedNearRoomTiles[highlightedNearRoomTiles.Count - 1].ResetHighlight();
                highlightedNearRoomTiles.RemoveAt(highlightedNearRoomTiles.Count - 1);
            }

            if (currentRoomMT != null)
            {
                currentRoomMT.ActiveTileHighlight_Color(false);
                currentRoomMT = null;
            }
            if (currentTileMT != null)
            {
                currentTileMT.ActiveTileHighlight_Scale(false);
                currentTileMT = null;
            }
        }

        public void HighlightTile(TileData tileData)
        {
            if (currentTileMT != null)
                currentTileMT.ActiveTileHighlight_Scale(false);
            if (currentRoomMT != null)
                currentRoomMT.ActiveTileHighlight_Color(false);
            if (tileMapTiles.TryGetValue(tileData, out MapTileUI tileMT))
            {
                currentTileMT = tileMT;
                currentTileMT.ActiveTileHighlight_Scale(true);
            }
        }
    }
}