using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class HpBar : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Transform FollowingTarget {get; private set;}

        [SerializeField] RectTransform hpBar;
        private Vector3 scale;

        void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            scale = hpBar.localScale;
        }
        
        public void SetHpBarPercent(float percent)
        {
            scale.x = percent;
            hpBar.localScale = scale;
        }

        public void SetFollowingTarget(Transform followingTarget)
        {
            FollowingTarget = followingTarget;
        }
    }
}
