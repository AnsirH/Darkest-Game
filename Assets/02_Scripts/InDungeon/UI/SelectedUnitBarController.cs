using System;
using DarkestLike.InDungeon.Manager;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class SelectedUnitBarController : MonoBehaviour
    {
        [SerializeField] private SelectedUnitBar playerSelectedBar;
        [SerializeField] private SelectedUnitBar enemySelectedBar;
        [SerializeField] private SelectedUnitBar hoverEnemyBar;
        public Vector3 offset;
        public void SetActivePlayerBar(bool active) { playerSelectedBar.gameObject.SetActive(active); }
        public void SetActiveEnemyBar(bool active) { enemySelectedBar.gameObject.SetActive(active); }
        public void SetActiveHoverEnemyBar(bool active) { hoverEnemyBar.gameObject.SetActive(active); }

        public void HoverEnemyUnit(Transform enemy)
        {
            SetActiveHoverEnemyBar(true);
            hoverEnemyBar.SetTarget(enemy);
        }

        public void ClearHover()
        {
            SetActiveHoverEnemyBar(false);
        }

        private void Start()
        {
            playerSelectedBar?.SetOffset(offset);
            playerSelectedBar?.SetViewCamera(InDungeonManager.Inst.ViewCamera);
            enemySelectedBar?.SetOffset(offset);
            enemySelectedBar?.SetViewCamera(InDungeonManager.Inst.ViewCamera);
            hoverEnemyBar?.SetOffset(offset);
            hoverEnemyBar?.SetViewCamera(InDungeonManager.Inst.ViewCamera);

            SetActiveHoverEnemyBar(false);
        }

        private void Update()
        {
            playerSelectedBar.UpdatePosition();
            enemySelectedBar.UpdatePosition();
            hoverEnemyBar.UpdatePosition();
        }

        public void SelectPlayerUnit(Transform player)
        {
            SetActivePlayerBar(false); // 선택 바 애니메이션을 재생시키기 위해
            SetActivePlayerBar(true);
            playerSelectedBar.SetTarget(player);
        }

        public void SelectEnemyUnit(Transform enemy)
        {
            SetActiveEnemyBar(true);
            enemySelectedBar.SetTarget(enemy);
        }
    }
}
