using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DarkestGame.Map
{
    /// <summary>
    /// 맵 데이터를 기반으로 UI 버튼들을 생성하고 배치하는 클래스
    /// 방과 복도의 타일들을 화면에 표시합니다.
    /// </summary>
    public class MapDrawer : MonoBehaviour
    {
        /// <summary>방 버튼의 RectTransform 프리팹</summary>
        public RectTransform roomButtonRect;
        
        /// <summary>타일 버튼의 RectTransform 프리팹</summary>
        public RectTransform tileButtonRect;

        /// <summary>방 버튼 간의 오프셋 거리</summary>
        public float roomButtonOffset = 30.0f;
        
        /// <summary>타일 버튼 간의 오프셋 거리</summary>
        public float tileButtonOffset = 10.0f;

        /// <summary>방 데이터와 UI 버튼을 매핑하는 딕셔너리</summary>
        Dictionary<RoomData, RectTransform> roomButtons = new();
        
        /// <summary>타일 데이터와 UI 버튼을 매핑하는 딕셔너리</summary>
        Dictionary<TileData, RectTransform> tileButtons = new();

        /// <summary>
        /// 컴포넌트 초기화
        /// 오프셋 값을 설정하고 맵 버튼들을 생성합니다.
        /// </summary>
        private void Awake()
        {
            roomButtonOffset = roomButtonRect.rect.width * 0.5f;
            tileButtonOffset = tileButtonRect.rect.width * 0.5f;

            GenerateButtons(DungeonManager.Inst.currentMap);
        }

        /// <summary>
        /// 맵 데이터를 기반으로 방과 복도 타일들의 UI 버튼을 생성합니다.
        /// </summary>
        /// <param name="map">생성할 맵 데이터</param>
        public void GenerateButtons(Map map)
        {
            if (map == null) return;
            roomButtons.Clear();
            tileButtons.Clear();

            // 1. 방 생성 (이미 생성된 방 제외)
            // 2. 복도의 다른 방 (이미 생성된 방 제외)
            // 3. 복도의 다른방으로 가는 복도 (이미 생성된 방 제외)

            Vector2 nextRoomPosition = Vector2.zero;
            float distanceBetweenRooms = roomButtonOffset * 2 + map.TileCount * 2 * tileButtonOffset;
            
            // 모든 방을 순회하며 UI 버튼 생성
            for (int i = 0; i < map.Rooms.Count; i++)
            {
                RoomData newRoom = map.Rooms[i];
                nextRoomPosition = newRoom.position * (int)distanceBetweenRooms;
                
                // 방 버튼이 아직 생성되지 않았다면 생성
                if (!roomButtons.ContainsKey(newRoom))
                {
                    CreateRoomButton(newRoom, nextRoomPosition);
                }

                // 4방향 복도 확인 및 타일 버튼 생성
                for (int j = 0; j < 4; j++)
                {
                    if (newRoom.exitHallways[j] != null && !tileButtons.ContainsKey(newRoom.exitHallways[j].tiles[0]))
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
                        nextButtonPosition += direction * roomButtonOffset;

                        HallwayData newHallway = newRoom.exitHallways[j];

                        // 복도의 각 타일에 대해 버튼 생성
                        for (int k = 0; k < newHallway.tiles.Length; k++)
                        {
                            nextButtonPosition += direction * tileButtonOffset;

                            CreateTileButton(newHallway.tiles[k], nextButtonPosition);

                            nextButtonPosition += direction * tileButtonOffset;
                        }
                    }
                }
            }

            SetCenter();
        }

        /// <summary>
        /// 방 데이터에 대한 UI 버튼을 생성합니다.
        /// </summary>
        /// <param name="roomData">방 데이터</param>
        /// <param name="buttonPosition">버튼 위치</param>
        private void CreateRoomButton(RoomData roomData, Vector2 buttonPosition)
        {
            RectTransform newRoomButtonRect = Instantiate(roomButtonRect.gameObject, transform).GetComponent<RectTransform>();
            roomButtons.Add(roomData, newRoomButtonRect);

            newRoomButtonRect.anchoredPosition = buttonPosition;
        }

        /// <summary>
        /// 타일 데이터에 대한 UI 버튼을 생성합니다.
        /// </summary>
        /// <param name="tileData">타일 데이터</param>
        /// <param name="buttonPosition">버튼 위치</param>
        private void CreateTileButton(TileData tileData, Vector2 buttonPosition)
        {
            RectTransform tileRect = Instantiate(tileButtonRect.gameObject, transform).GetComponent<RectTransform>();
            tileButtons.Add(tileData, tileRect);

            tileRect.anchoredPosition = buttonPosition;
        }

        /// <summary>
        /// 모든 버튼들을 화면 중앙에 배치합니다.
        /// 방 버튼들의 중심점을 계산하여 모든 버튼을 중앙으로 이동시킵니다.
        /// </summary>
        private void SetCenter()
        {
            // 방 버튼들의 중심점 계산
            Vector2 center = Vector2.zero;
            foreach (var roomButton in roomButtons)
            {
                center += roomButton.Value.anchoredPosition;
            }
            center /= roomButtons.Count;

            // 중앙으로 이동할 벡터 계산
            Vector2 moveVector = Vector2.zero - center;

            // 모든 방 버튼을 중앙으로 이동
            foreach (var roomButton in roomButtons)
            {
                roomButton.Value.anchoredPosition += moveVector;
            }

            // 모든 타일 버튼을 중앙으로 이동
            foreach (var tileButton in tileButtons)
            {
                tileButton.Value.anchoredPosition += moveVector;
            }
        }
    }
}