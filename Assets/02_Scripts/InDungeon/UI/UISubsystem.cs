using DarkestLike.Map;
using DarkestLike.Singleton;
using System.Collections;
using System.Collections.Generic;
using _02_Scripts.InDungeon.UI;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace DarkestLike.InDungeon.UI
{
    public class UISubsystem : InDungeonSubsystem
    {
        [Header("references")]
        [SerializeField] MapDrawer mapDrawer;
        [SerializeField] BattleHud battleHud;
        [SerializeField] Image fadeOutImage;
        [SerializeField] HpBarController hpBarController;
        [SerializeField] SelectedUnitBarController selectedUnitBarController;

        // Properties
        public MapDrawer MapDrawer => mapDrawer;
        public SelectedUnitBarController SelectedUnitBarController => selectedUnitBarController;

        public void ActiveMapDrawer(bool active) { mapDrawer.gameObject.SetActive(active); }
        public void ActiveBattleHud(bool active) { battleHud.gameObject.SetActive(active); }

        private void OnEnable()
        {
            DungeonEventBus.Subscribe(DungeonEventType.Loading, LoadingHandler);
            DungeonEventBus.Subscribe(DungeonEventType.EnterRoom,  EnterRoomHandler);
            DungeonEventBus.Subscribe(DungeonEventType.EnterHallway, EnterHallwayHandler);
            DungeonEventBus.Subscribe(DungeonEventType.EnterTile, EnterTileHandler);
        }

        private void OnDisable()
        {
            DungeonEventBus.Unsubscribe(DungeonEventType.Loading, LoadingHandler);
            DungeonEventBus.Unsubscribe(DungeonEventType.EnterRoom, EnterRoomHandler);
            DungeonEventBus.Unsubscribe(DungeonEventType.EnterHallway, EnterHallwayHandler);
            DungeonEventBus.Unsubscribe(DungeonEventType.EnterTile, EnterTileHandler);
        }

        private void LoadingHandler()
        {
            mapDrawer.ClearHighlight();
            mapDrawer.ActiveRoomButtonsInteractive(false);
        }

        private void EnterRoomHandler()
        {            
            // 입장한 방 하이라이트
            mapDrawer.HighlightRoom(InDungeonManager.Inst.CurrentRoom);
            mapDrawer.ActiveRoomButtonsInteractive(true);
            mapDrawer.ActiveMapButtonInteractive(InDungeonManager.Inst.CurrentRoom, false);
        }

        private void EnterHallwayHandler()
        {
            mapDrawer.ActiveRoomButtonsInteractive(false);
            mapDrawer.HighlightCurrentTile();
        }

        private void EnterTileHandler()
        {
            mapDrawer.ClearHighlight();
            mapDrawer.HighlightCurrentTile();
        }
        
        protected override void OnInitialize()
        {
        }
        
        public void GenerateMapUI(MapData mapData)
        {
            mapDrawer.GenerateButtons(mapData);
        }

        public IEnumerator FadeOutCoroutine(bool active, float duration)
        {
            float timer = 0.0f;
            Color startColor = active ? new(0, 0, 0, 0) : Color.black;
            Color endColor = active ? Color.black : new(0, 0, 0, 0);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                fadeOutImage.color = Color.Lerp(startColor, endColor, timer / duration);
                yield return null;
            }
        }

        public void ResetFadeOutImage()
        {
            fadeOutImage.color = new(0, 0, 0, 0);
        }

        public void CreateHpBar(CharacterUnit unit)
        {
            hpBarController.CreateHpBar(unit);
        }

        public void OnSelectPlayerUnit(CharacterUnit characterUnit)
        {
            battleHud.SetCharacterInfo(characterUnit);
            
            SelectedUnitBarController.SetActivePlayerBar(true);
            SelectedUnitBarController.SelectPlayerUnit(characterUnit.transform);
            battleHud.UpdateSkillIcon(characterUnit.CharacterData.Base.skills);
            battleHud.UpdateSkilInfo(characterUnit.CharacterData.Base.skills[0]);
        }
        public void OnSelectPlayerSkill(SkillBase skill) { battleHud.UpdateSkilInfo(skill);}
    }
}