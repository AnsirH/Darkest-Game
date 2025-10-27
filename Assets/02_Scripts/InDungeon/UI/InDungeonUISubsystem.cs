using DarkestLike.Map;
using DarkestLike.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkestLike.InDungeon.UI
{
    public class InDungeonUISubsystem : InDungeonSubsystem
    {
        [Header("references")]
        [SerializeField] MapDrawer mapDrawer;
        [SerializeField] BattleHud battleHud;
        [SerializeField] Image fadeOutImage;

        [Header("Variables")]
        [SerializeField] float fadeDuration;

        // Properties
        public MapDrawer MapDrawer => mapDrawer;

        public void ActiveMapDrawer(bool active) { mapDrawer.gameObject.SetActive(active); }
        public void ActiveBattleHud(bool active) { battleHud.gameObject.SetActive(active); }

        public void GenerateMapUI(MapData mapData)
        {
            mapDrawer.GenerateButtons(mapData);
        }

        protected override void OnInitialize()
        {
            InDungeonManager.Inst.OnRoomEntered += OnRoomEnteredHandler;
            InDungeonManager.Inst.OnHallwayEntered += OnHallwayEnteredHandler;
            InDungeonManager.Inst.OnTileEntered += OnTileEnteredHandler;
        }

        private void OnRoomEnteredHandler(RoomData roomData)
        {
            ActiveBattleHud(true);
            ActiveMapDrawer(true);
            StartCoroutine(FadeOutCoroutine(false));
            // 입장한 방 하이라이트
            mapDrawer.HighlightRoom(roomData);
            // 입장 가능한 방 하이라이트
            // 이건 추후에 방 전투가 끝났을 시 실행되도록 해야함. 방 전투 없으면 상관없는데. 고민해봐야 함
            mapDrawer.HighlightNearRooms(roomData); 
        }

        void OnHallwayEnteredHandler(HallwayData hallwayData)
        {
            StartCoroutine(FadeOutCoroutine(false));
        }

        private void OnTileEnteredHandler(TileData tileData)
        {
            ActiveBattleHud(true);
            ActiveMapDrawer(true); 
            mapDrawer.ClearHighlight();
            mapDrawer.HighlightTile(tileData);
        }

        public IEnumerator FadeOutCoroutine(bool active)
        {
            float timer = 0.0f;
            Color startColor = active ? new(0, 0, 0, 0) : Color.black;
            Color endColor = active ? Color.black : new(0, 0, 0, 0);

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeOutImage.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
                yield return null;
            }
        }

        public void ResetFadeOutImage()
        {
            fadeOutImage.color = new(0, 0, 0, 0);
        }
    }
}