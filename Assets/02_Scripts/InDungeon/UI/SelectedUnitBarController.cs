using System;
using DarkestLike.InDungeon.Manager;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class SelectedUnitBarController : MonoBehaviour
    {
        [SerializeField] private SelectedUnitBar playerSelectedBar;
        [SerializeField] private SelectedUnitBar enemySelectedBar;
        public Vector3 offset;
        public void SetActivePlayerBar(bool active) { playerSelectedBar.gameObject.SetActive(active); }
        public void SetActiveEnemyBar(bool active) { enemySelectedBar.gameObject.SetActive(active); }

        private void Start()
        {
            playerSelectedBar?.SetOffset(offset);
            playerSelectedBar?.SetViewCamera(InDungeonManager.Inst.ViewCamera);
            enemySelectedBar?.SetOffset(offset);
            enemySelectedBar?.SetViewCamera(InDungeonManager.Inst.ViewCamera);
        }

        private void Update()
        {
            playerSelectedBar.UpdatePosition();
        }

        public void SelectPlayerUnit(Transform player)
        {
            SetActivePlayerBar(true);
            playerSelectedBar.SetTarget(player);
        }

        public void SelectEnemyUnit(Transform enemy)
        {
            enemySelectedBar.SetTarget(enemy);
        }
    }
}
