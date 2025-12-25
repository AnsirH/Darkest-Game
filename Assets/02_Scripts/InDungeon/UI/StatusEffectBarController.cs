using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Unit;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    /// <summary>
    /// 상태 이상 바 동적 생성 및 관리
    /// HpBarController와 동일한 패턴 사용
    /// </summary>
    public class StatusEffectBarController : MonoBehaviour
    {
        [SerializeField] private Vector3 offset;
        [SerializeField] private StatusEffectBar statusEffectBarPrefab;
        [SerializeField] private StatusEffectIconPool iconPool;

        private Dictionary<CharacterUnit, StatusEffectBar> statusEffectBars = new Dictionary<CharacterUnit, StatusEffectBar>();

        private void Update()
        {
            // 모든 StatusEffectBar의 위치 갱신
            foreach (var bar in statusEffectBars.Values)
            {
                bar.UpdatePosition();
                bar.UpdateIcons();  // 상태 이상 변경 체크 및 아이콘 갱신
            }
        }

        /// <summary>
        /// 특정 유닛의 StatusEffectBar를 생성합니다.
        /// </summary>
        public void CreateStatusEffectBar(CharacterUnit targetUnit)
        {
            StatusEffectBar newBar = Instantiate(statusEffectBarPrefab, transform);
            statusEffectBars[targetUnit] = newBar;
            newBar.SetOffset(offset);
            newBar.SetViewCamera(InDungeonManager.Inst.ViewCamera);
            newBar.SetTarget(targetUnit.transform);
            newBar.Initialize(targetUnit, iconPool);
            newBar.UpdatePosition();
        }

        /// <summary>
        /// 특정 유닛의 StatusEffectBar를 제거합니다.
        /// </summary>
        public void RemoveStatusEffectBar(CharacterUnit targetUnit)
        {
            if (statusEffectBars.TryGetValue(targetUnit, out StatusEffectBar bar))
            {
                bar.ClearIcons();  // 아이콘 풀로 반환
                Destroy(bar.gameObject);
                statusEffectBars.Remove(targetUnit);
            }
        }

        /// <summary>
        /// 특정 유닛의 상태 이상 아이콘을 즉시 갱신합니다.
        /// </summary>
        public void UpdateStatusEffectIcons(CharacterUnit targetUnit)
        {
            if (targetUnit != null && statusEffectBars.ContainsKey(targetUnit))
            {
                statusEffectBars[targetUnit].UpdateIcons();
            }
        }

        /// <summary>
        /// 특정 유닛의 상태 이상 바를 활성화/비활성화합니다.
        /// </summary>
        public void SetStatusEffectBarActive(CharacterUnit targetUnit, bool isActive)
        {
            if (targetUnit != null && statusEffectBars.TryGetValue(targetUnit, out StatusEffectBar bar))
            {
                bar.gameObject.SetActive(isActive);
            }
        }

        /// <summary>
        /// 여러 유닛의 상태 이상 바를 활성화/비활성화합니다.
        /// </summary>
        public void SetMultipleStatusEffectBarsActive(List<CharacterUnit> units, bool isActive)
        {
            if (units == null) return;

            foreach (var unit in units)
            {
                if (unit != null && statusEffectBars.TryGetValue(unit, out StatusEffectBar bar))
                {
                    bar.gameObject.SetActive(isActive);
                }
            }
        }
    }
}
