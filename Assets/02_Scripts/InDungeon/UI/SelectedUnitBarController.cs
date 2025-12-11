using System;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class SelectedUnitBarController : MonoBehaviour
    {
        [SerializeField] private SelectedUnitBar selectedBar;  // 현재 턴인 유닛 표시 (플레이어/적 공통)
        [SerializeField] private SelectedUnitBar hoverEnemyBar;  // 마우스 호버 시 적 표시

        [Header("Unified Targeting System")]
        [SerializeField] private SelectedUnitBar[] targetableBars;  // 통합 바 4개

        [Header("Connection Indicators (Multi-Target)")]
        [SerializeField] private UnityEngine.UI.Image[] connectionIndicators;  // 연결 이미지 3개 (0: 1-2, 1: 2-3, 2: 3-4)

        [Header("Colors")]
        [SerializeField] private Color attackColor = Color.red;        // 공격 스킬 색상
        [SerializeField] private Color supportColor = Color.green;     // 회복/버프 색상

        public Vector3 selectedBarOffset;
        public Vector3 hoverBarOffset;
        public Vector3 targetableBarOffset;

        /// <summary>
        /// 현재 턴 선택 바 활성화/비활성화
        /// </summary>
        public void SetActiveSelectedBar(bool active) { selectedBar.gameObject.SetActive(active); }

        /// <summary>
        /// 호버 바 활성화/비활성화
        /// </summary>
        public void SetActiveHoverBar(bool active) { hoverEnemyBar.gameObject.SetActive(active); }

        /// <summary>
        /// 적 유닛 호버 시 호버 바 표시
        /// </summary>
        public void HoverEnemyUnit(Transform enemy)
        {
            if (enemy == null) return;

            Vector3 screenPos = InDungeonManager.Inst.ViewCamera.WorldToScreenPoint(enemy.position);
            hoverEnemyBar.SetTarget(screenPos);
            SetActiveHoverBar(true);
        }

        /// <summary>
        /// 호버 해제
        /// </summary>
        public void ClearHover()
        {
            SetActiveHoverBar(false);
        }

        private void Start()
        {
            selectedBar?.SetOffset(selectedBarOffset);
            hoverEnemyBar?.SetOffset(hoverBarOffset);

            // 통합 타겟 바 초기화
            if (targetableBars != null)
            {
                for (int i = 0; i < targetableBars.Length; i++)
                {
                    if (targetableBars[i] != null)
                    {
                        targetableBars[i].SetOffset(targetableBarOffset);
                        targetableBars[i].gameObject.SetActive(false);
                    }
                }
            }

            // 연결 이미지 초기화
            if (connectionIndicators != null)
            {
                for (int i = 0; i < connectionIndicators.Length; i++)
                {
                    if (connectionIndicators[i] != null)
                    {
                        connectionIndicators[i].gameObject.SetActive(false);
                    }
                }
            }

            SetActiveHoverBar(false);
        }

        /// <summary>
        /// 유닛 선택 (플레이어/적 구분 없이)
        /// </summary>
        public void SelectUnit(Transform unitTransform)
        {
            if (unitTransform == null) return;

            SetActiveSelectedBar(false); // 선택 바 애니메이션 재생을 위해
            Vector3 screenPos = InDungeonManager.Inst.ViewCamera.WorldToScreenPoint(unitTransform.position);
            selectedBar.SetTarget(screenPos);
            SetActiveSelectedBar(true);
        }

        /// <summary>
        /// 타겟 가능한 유닛에 표시 바 활성화 (색상 동적 변경)
        /// </summary>
        /// <param name="targetableUnits">타겟 가능한 유닛 리스트</param>
        /// <param name="isSupportiveSkill">true=회복/버프(초록), false=공격(빨강)</param>
        public void ShowTargetableUnits(List<DarkestLike.InDungeon.Unit.CharacterUnit> targetableUnits, bool isSupportiveSkill)
        {
            ClearTargetableUnits();

            if (targetableBars == null || targetableUnits == null) return;

            Camera viewCam = InDungeonManager.Inst.ViewCamera;
            Color barColor = isSupportiveSkill ? supportColor : attackColor;

            int barIndex = 0;
            foreach (var unit in targetableUnits)
            {
                if (barIndex >= targetableBars.Length) break;
                if (unit == null || !unit.IsAlive) continue;

                Vector3 screenPos = viewCam.WorldToScreenPoint(unit.transform.position);
                targetableBars[barIndex].SetTarget(screenPos);

                // 색상 동적 변경
                UnityEngine.UI.Image barImage = targetableBars[barIndex].GetComponent<UnityEngine.UI.Image>();
                if (barImage != null)
                {
                    barImage.color = barColor;
                }

                targetableBars[barIndex].gameObject.SetActive(true);
                barIndex++;
            }
        }

        /// <summary>
        /// 멀티 타겟 연결 이미지 표시
        /// </summary>
        /// <param name="targetableUnits">타겟 가능한 유닛 리스트</param>
        /// <param name="canTargetFront">전열 타겟 가능 여부</param>
        /// <param name="canTargetBack">후열 타겟 가능 여부</param>
        /// <param name="isSupportiveSkill">true=회복/버프(초록), false=공격(빨강)</param>
        public void ShowConnectionIndicators(
            List<DarkestLike.InDungeon.Unit.CharacterUnit> targetableUnits,
            bool canTargetFront,
            bool canTargetBack,
            bool isSupportiveSkill)
        {
            // 먼저 개별 바 표시
            ShowTargetableUnits(targetableUnits, isSupportiveSkill);

            if (targetableUnits == null || targetableUnits.Count == 0 || connectionIndicators == null) return;

            Color connectionColor = isSupportiveSkill ? supportColor : attackColor;

            // 전열 + 후열 모두 타겟 가능 → 연결 이미지 3개 (1-2, 2-3, 3-4)
            if (canTargetFront && canTargetBack)
            {
                SetConnectionIndicator(0, 0, 1, connectionColor);  // 1-2 사이
                SetConnectionIndicator(1, 1, 2, connectionColor);  // 2-3 사이
                SetConnectionIndicator(2, 2, 3, connectionColor);  // 3-4 사이
            }
            // 전열만 → 연결 이미지 1개 (1-2)
            else if (canTargetFront && !canTargetBack)
            {
                SetConnectionIndicator(0, 0, 1, connectionColor);  // 1-2 사이
                // 나머지 비활성화
                if (connectionIndicators.Length > 1 && connectionIndicators[1] != null)
                    connectionIndicators[1].gameObject.SetActive(false);
                if (connectionIndicators.Length > 2 && connectionIndicators[2] != null)
                    connectionIndicators[2].gameObject.SetActive(false);
            }
            // 후열만 → 연결 이미지 1개 (실제로 활성화된 바 0-1 사이)
            else if (!canTargetFront && canTargetBack)
            {
                SetConnectionIndicator(0, 0, 1, connectionColor);  // 0-1 사이 (후열 유닛이 0, 1번 바에 할당되므로)
                // 나머지 비활성화
                if (connectionIndicators.Length > 1 && connectionIndicators[1] != null)
                    connectionIndicators[1].gameObject.SetActive(false);
                if (connectionIndicators.Length > 2 && connectionIndicators[2] != null)
                    connectionIndicators[2].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 모든 타겟 바 및 연결 이미지 비활성화
        /// </summary>
        public void ClearTargetableUnits()
        {
            if (targetableBars != null)
            {
                for (int i = 0; i < targetableBars.Length; i++)
                {
                    if (targetableBars[i] != null)
                    {
                        targetableBars[i].gameObject.SetActive(false);
                    }
                }
            }

            ClearConnectionIndicators();
        }

        /// <summary>
        /// 모든 연결 이미지 비활성화
        /// </summary>
        public void ClearConnectionIndicators()
        {
            if (connectionIndicators != null)
            {
                for (int i = 0; i < connectionIndicators.Length; i++)
                {
                    if (connectionIndicators[i] != null)
                    {
                        connectionIndicators[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 두 표시 바 사이의 중간 위치 계산
        /// </summary>
        private Vector3 GetConnectionPosition(int bar1Index, int bar2Index)
        {
            if (targetableBars == null || bar1Index >= targetableBars.Length || bar2Index >= targetableBars.Length)
                return Vector3.zero;

            var bar1 = targetableBars[bar1Index];
            var bar2 = targetableBars[bar2Index];

            if (bar1 == null || bar2 == null || !bar1.gameObject.activeSelf || !bar2.gameObject.activeSelf)
                return Vector3.zero;

            RectTransform rect1 = bar1.GetComponent<RectTransform>();
            RectTransform rect2 = bar2.GetComponent<RectTransform>();

            return (rect1.position + rect2.position) / 2f;
        }

        /// <summary>
        /// 연결 이미지 위치와 색상 설정
        /// </summary>
        private void SetConnectionIndicator(int indicatorIndex, int bar1Index, int bar2Index, Color color)
        {
            if (connectionIndicators == null || indicatorIndex >= connectionIndicators.Length)
                return;

            var indicator = connectionIndicators[indicatorIndex];
            if (indicator == null) return;

            Vector3 position = GetConnectionPosition(bar1Index, bar2Index);

            if (position == Vector3.zero)
            {
                indicator.gameObject.SetActive(false);
                return;
            }

            RectTransform rectTransform = indicator.GetComponent<RectTransform>();
            rectTransform.position = position;
            indicator.color = color;
            indicator.gameObject.SetActive(true);
        }
    }
}
