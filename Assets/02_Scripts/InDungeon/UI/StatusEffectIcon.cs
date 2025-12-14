using DarkestLike.InDungeon.BattleSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _02_Scripts.InDungeon.UI
{
    /// <summary>
    /// 개별 상태 이상 아이콘을 표시하는 컴포넌트
    /// </summary>
    public class StatusEffectIcon : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI durationText;  // 선택적: 지속시간 표시

        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 상태 이상 효과를 설정하고 아이콘을 업데이트합니다.
        /// </summary>
        public void SetEffect(StatusEffect effect, Sprite sprite)
        {
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
            }

            if (durationText != null)
            {
                durationText.text = effect.duration.ToString();
            }
        }

        /// <summary>
        /// 아이콘을 숨깁니다 (풀로 반환될 때 호출).
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 아이콘을 표시합니다 (풀에서 가져올 때 호출).
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
