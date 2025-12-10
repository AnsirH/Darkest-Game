using System;
using DarkestLike.InDungeon.Manager;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class SelectedUnitBarController : MonoBehaviour
    {
        [SerializeField] private SelectedUnitBar selectedBar;  // 현재 턴인 유닛 표시 (플레이어/적 공통)
        [SerializeField] private SelectedUnitBar hoverEnemyBar;  // 마우스 호버 시 적 표시
        [SerializeField] private SelectedUnitBar[] targetableEnemyBars;  // 공격 가능한 적 표시 (최대 4개)
        [SerializeField] private SelectedUnitBar[] targetableAllyBars;  // 타겟 가능한 아군 표시 (최대 4개)
        public Vector3 selectedBarOffset;
        public Vector3 hoverBarOffset;
        public Vector3 targetableBarOffset;
        public Vector3 targetableAllyBarOffset;

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
            SetActiveHoverBar(true);
            hoverEnemyBar.SetTarget(enemy);
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
            selectedBar?.SetViewCamera(InDungeonManager.Inst.ViewCamera);
            hoverEnemyBar?.SetOffset(hoverBarOffset);
            hoverEnemyBar?.SetViewCamera(InDungeonManager.Inst.ViewCamera);

            // 공격 가능 표시 바 초기화
            if (targetableEnemyBars != null)
            {
                for (int i = 0; i < targetableEnemyBars.Length; i++)
                {
                    if (targetableEnemyBars[i] != null)
                    {
                        targetableEnemyBars[i].SetOffset(targetableBarOffset);
                        targetableEnemyBars[i].SetViewCamera(InDungeonManager.Inst.ViewCamera);
                        targetableEnemyBars[i].gameObject.SetActive(false);
                    }
                }
            }

            // 타겟 가능 아군 표시 바 초기화
            if (targetableAllyBars != null)
            {
                for (int i = 0; i < targetableAllyBars.Length; i++)
                {
                    if (targetableAllyBars[i] != null)
                    {
                        targetableAllyBars[i].SetOffset(targetableAllyBarOffset);
                        targetableAllyBars[i].SetViewCamera(InDungeonManager.Inst.ViewCamera);
                        targetableAllyBars[i].gameObject.SetActive(false);
                    }
                }
            }

            SetActiveHoverBar(false);
        }

        private void Update()
        {
            selectedBar.UpdatePosition();
            hoverEnemyBar.UpdatePosition();

            // 공격 가능 표시 바 위치 업데이트
            if (targetableEnemyBars != null)
            {
                for (int i = 0; i < targetableEnemyBars.Length; i++)
                {
                    if (targetableEnemyBars[i] != null && targetableEnemyBars[i].gameObject.activeSelf)
                    {
                        targetableEnemyBars[i].UpdatePosition();
                    }
                }
            }

            // 타겟 가능 아군 표시 바 위치 업데이트
            if (targetableAllyBars != null)
            {
                for (int i = 0; i < targetableAllyBars.Length; i++)
                {
                    if (targetableAllyBars[i] != null && targetableAllyBars[i].gameObject.activeSelf)
                    {
                        targetableAllyBars[i].UpdatePosition();
                    }
                }
            }
        }

        /// <summary>
        /// 유닛 선택 (플레이어/적 구분 없이)
        /// </summary>
        public void SelectUnit(Transform unitTransform)
        {
            SetActiveSelectedBar(false); // 선택 바 애니메이션 재생을 위해
            SetActiveSelectedBar(true);
            selectedBar.SetTarget(unitTransform);
        }

        /// <summary>
        /// 공격 가능한 적 유닛에 표시 바 활성화
        /// </summary>
        /// <param name="targetableEnemies">공격 가능한 적 유닛 리스트</param>
        public void ShowTargetableEnemies(System.Collections.Generic.List<DarkestLike.InDungeon.Unit.CharacterUnit> targetableEnemies)
        {
            // 모든 표시 바 비활성화
            ClearTargetableEnemies();

            if (targetableEnemyBars == null || targetableEnemies == null) return;

            // 공격 가능한 적에게 표시 바 활성화
            int barIndex = 0;
            foreach (var enemy in targetableEnemies)
            {
                if (barIndex >= targetableEnemyBars.Length) break;
                if (enemy == null || !enemy.IsAlive) continue;

                targetableEnemyBars[barIndex].SetTarget(enemy.transform);
                targetableEnemyBars[barIndex].gameObject.SetActive(true);
                barIndex++;
            }
        }

        /// <summary>
        /// 모든 공격 가능 표시 바 비활성화
        /// </summary>
        public void ClearTargetableEnemies()
        {
            if (targetableEnemyBars == null) return;

            for (int i = 0; i < targetableEnemyBars.Length; i++)
            {
                if (targetableEnemyBars[i] != null)
                {
                    targetableEnemyBars[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 타겟 가능한 아군 유닛에 표시 바 활성화
        /// </summary>
        /// <param name="targetableAllies">타겟 가능한 아군 유닛 리스트</param>
        public void ShowTargetableAllies(System.Collections.Generic.List<DarkestLike.InDungeon.Unit.CharacterUnit> targetableAllies)
        {
            // 모든 표시 바 비활성화
            ClearTargetableAllies();

            if (targetableAllyBars == null || targetableAllies == null) return;

            // 타겟 가능한 아군에게 표시 바 활성화
            int barIndex = 0;
            foreach (var ally in targetableAllies)
            {
                if (barIndex >= targetableAllyBars.Length) break;
                if (ally == null || !ally.IsAlive) continue;

                targetableAllyBars[barIndex].SetTarget(ally.transform);
                targetableAllyBars[barIndex].gameObject.SetActive(true);
                barIndex++;
            }
        }

        /// <summary>
        /// 모든 타겟 가능 아군 표시 바 비활성화
        /// </summary>
        public void ClearTargetableAllies()
        {
            if (targetableAllyBars == null) return;

            for (int i = 0; i < targetableAllyBars.Length; i++)
            {
                if (targetableAllyBars[i] != null)
                {
                    targetableAllyBars[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
