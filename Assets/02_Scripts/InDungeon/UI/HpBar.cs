using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class HpBar : MonoBehaviour
    {
        public RectTransform hpBar;

        private Vector3 scale;

        void Start()
        {
            scale = hpBar.localScale;
        }

        public void SetHpBarPercent(float percent)
        {
            scale.x = percent;
            hpBar.localScale = scale;
        }
    }
}
