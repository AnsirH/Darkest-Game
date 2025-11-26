using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class SelectedUnitBarController : MonoBehaviour
    {
        [SerializeField] private RectTransform playerSelectedBar;
        [SerializeField] private RectTransform enemySelectedBar;
        
        public void SetActivePlayerBar(bool active) { playerSelectedBar.gameObject.SetActive(active); }
        public void SetActiveEnemyBar(bool active) { enemySelectedBar.gameObject.SetActive(active); }

        public void SelectPlayerUnit(Vector3 rectPosition)
        {
            playerSelectedBar.position = rectPosition;
        }

        public void SelectEnemyUnit(Vector3 rectPosition)
        {
            enemySelectedBar.position = rectPosition;
        }
    }
}
