using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
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
        [SerializeField] MapRoomNode roomNodePrefab;

        /// <summary>타일 버튼의 RectTransform 프리팹</summary>
        [SerializeField] MapNode tileNodePrefab;

        [Header("variables")]
        /// <summary>방 버튼 간의 오프셋 거리</summary>
        [SerializeField] float roomNodeSize = 70.0f;

        /// <summary>타일 버튼 간의 오프셋 거리</summary>
        [SerializeField] float tileNodeSize = 20.0f;

        // variables
        Dictionary<RoomData, MapRoomNode> roomNodes = new();
        Dictionary<TileData, MapNode> tileNodes = new();
        MapNode currentRoomNode;
        MapNode currentTileNode;
        List<MapNode> highlightedNearRoomNodes = new();

        /// <summary>
        /// 맵 데이터를 기반으로 방과 복도 타일들의 UI 버튼을 생성합니다.
        /// </summary>
        /// <param name="map">생성할 맵 데이터</param>
        public void GenerateButtons(MapData map)
        {
            if (map == null) return;
            roomNodes.Clear();
            tileNodes.Clear();

            // 1. 방 생성 (이미 생성된 방 제외)
            // 2. 복도의 다른 방 (이미 생성된 방 제외)
            // 3. 복도의 다른방으로 가는 복도 (이미 생성된 방 제외)

            Vector2 mapRect = mapDrawArea.rect.size;
            Vector2 nextRoomPosition = Vector2.zero;
            float distanceBetweenRooms = roomNodeSize + map.TileCount * tileNodeSize;
            
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
                if (!roomNodes.ContainsKey(newRoom))
                {
                    CreateRoomMapTile(newRoom, nextRoomPosition);
                }

                // 4방향 복도 확인 및 타일 버튼 생성
                for (int j = 0; j < 4; j++)
                {
                    if (newRoom.ExitHallways[j] != null && !tileNodes.ContainsKey(newRoom.ExitHallways[j].Tiles[0]))
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
                        nextButtonPosition += direction * roomNodeSize * 0.5f;

                        HallwayData newHallway = newRoom.ExitHallways[j];

                        // 복도의 각 타일에 대해 버튼 생성
                        for (int k = 0; k < newHallway.Tiles.Length; k++)
                        {
                            nextButtonPosition += direction * tileNodeSize * 0.5f;

                            CreateTileButton(newHallway.Tiles[k], nextButtonPosition);

                            nextButtonPosition += direction * tileNodeSize * 0.5f;
                        }
                    }
                }
            }
            mapDrawArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mapRect.x + distanceBetweenRooms);
            mapDrawArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mapRect.y + distanceBetweenRooms);
            SetCenter();
        }
        
        private void CreateRoomMapTile(RoomData roomData, Vector2 buttonPosition)
        {
            MapRoomNode roomUI = Instantiate(roomNodePrefab, mapDrawArea);
            roomUI.SetTileSize(Vector2.one * roomNodeSize);
            roomUI.UpdateImage(roomData.RoomType);
            roomUI.SetRoomData(roomData);
            roomNodes.Add(roomData, roomUI);

            roomUI.SetPosition(buttonPosition);
        }

        private void CreateTileButton(TileData tileData, Vector2 buttonPosition)
        {
            MapNode node = Instantiate(tileNodePrefab, mapDrawArea);
            node.SetTileSize(Vector2.one * tileNodeSize);
            node.UpdateImage(tileData.type);
            tileNodes.Add(tileData, node);

            node.SetPosition(buttonPosition);
        }

        private void SetCenter()
        {
            // 방 버튼들의 중심점 계산
            Vector2 center = Vector2.zero;
            foreach (var roomButton in roomNodes)
            {
                center += roomButton.Value.AnchoredPosition;
            }
            center /= roomNodes.Count;

            // 중앙으로 이동할 벡터 계산
            Vector2 moveVector = Vector2.zero - center;

            // 모든 방 버튼을 중앙으로 이동
            foreach (var roomButton in roomNodes)
            {
                roomButton.Value.SetPosition(roomButton.Value.AnchoredPosition + moveVector);
            }

            // 모든 타일 버튼을 중앙으로 이동
            foreach (var tileButton in tileNodes)
            {
                tileButton.Value.SetPosition(tileButton.Value.AnchoredPosition + moveVector);
            }
        }

        public void HighlightRoom(RoomData roomData)
        {
            if (currentRoomNode != null)
                currentRoomNode.ActiveTileHighlight_Color(false);
            if (currentTileNode != null)
                currentTileNode.ActiveTileHighlight_Scale(false);
            if (roomNodes.TryGetValue(roomData, out MapRoomNode roomMT))
            {
                currentRoomNode = roomMT;
                currentRoomNode.ActiveTileHighlight_Color(true);
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
            if (roomNodes.TryGetValue(roomData, out MapRoomNode roomMT))
            {
                highlightedNearRoomNodes.Add(roomMT);
                roomMT.ActiveLoopHighlight_Color();
            }
        }

        public void ClearHighlight()
        {
            if (highlightedNearRoomNodes.Count == 0) return;
            while (highlightedNearRoomNodes.Count > 0)
            {
                highlightedNearRoomNodes[highlightedNearRoomNodes.Count - 1].ResetHighlight();
                highlightedNearRoomNodes.RemoveAt(highlightedNearRoomNodes.Count - 1);
            }

            if (currentRoomNode is not null)
            {
                currentRoomNode.ActiveTileHighlight_Color(false);
                currentRoomNode = null;
            }
            if (currentTileNode is not null)
            {
                currentTileNode.ActiveTileHighlight_Scale(false);
                currentTileNode = null;
            }
        }

        public void HighlightCurrentTile()
        {
            TileData tileData = InDungeonManager.Inst.CurrentTile;
            if (tileData == null) return;
            if (currentTileNode != null)
                currentTileNode.ActiveTileHighlight_Scale(false);
            if (currentRoomNode != null)
                currentRoomNode.ActiveTileHighlight_Color(false);
            if (tileNodes.TryGetValue(tileData, out MapNode tileMT))
            {
                currentTileNode = tileMT;
                currentTileNode.ActiveTileHighlight_Scale(true);
            }
        }

        public void ActiveRoomButtonsInteractive(bool active)
        {
            foreach (MapRoomNode mapBtn in roomNodes.Values)
            {
                mapBtn.button.interactable = active;
            }
        }

        public void ActiveMapButtonInteractive(RoomData roomData, bool active)
        {
            if (roomNodes.TryGetValue(roomData, out MapRoomNode roomBtn))
                roomBtn.button.interactable = active;
        }
    }
}