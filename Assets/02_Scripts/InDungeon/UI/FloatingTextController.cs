using TMPro;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    /// <summary>
    /// 전투 중 떠다니는 텍스트 UI를 제어합니다 (데미지, 회복량 등).
    /// Timeline의 Activation Track으로 활성화/비활성화되며,
    /// 코드에서는 값과 색상만 설정합니다.
    /// </summary>
    public class FloatingTextController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI floatingText;
        [SerializeField] private Animator animator;

        // 텍스트 색상 프리셋
        [Header("Color Presets")]
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color criticalDamageColor = Color.yellow;
        [SerializeField] private Color healColor = Color.green;

        private void Awake()
        {
            floatingText ??= GetComponent<TextMeshProUGUI>();
            animator ??= GetComponent<Animator>();
        }

        /// <summary>
        /// 데미지 값을 설정합니다.
        /// </summary>
        /// <param name="damage">데미지 값</param>
        /// <param name="isCritical">크리티컬 여부</param>
        public void SetDamage(int damage, bool isCritical = false)
        {
            floatingText.text = damage.ToString();
            floatingText.color = isCritical ? criticalDamageColor : normalDamageColor;
        }

        /// <summary>
        /// 힐 값을 설정합니다.
        /// </summary>
        /// <param name="healAmount">회복량</param>
        public void SetHeal(int healAmount)
        {
            floatingText.text = healAmount.ToString();
            floatingText.color = healColor;
        }

        /// <summary>
        /// Miss 텍스트를 설정합니다.
        /// </summary>
        public void SetMiss()
        {
            floatingText.text = "빗나감!";
            floatingText.color = Color.gray;
        }

        /// <summary>
        /// Animator 컴포넌트를 반환합니다 (Timeline 바인딩용).
        /// </summary>
        public Animator GetAnimator() => animator;

        /// <summary>
        /// GameObject를 비활성화 상태로 초기화합니다.
        /// Timeline 재생 전 호출하여 깨끗한 상태로 준비합니다.
        /// </summary>
        public void ResetState()
        {
            gameObject.SetActive(false);
        }
    }
}
