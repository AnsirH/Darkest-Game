using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Unit;
using UnityEngine;

namespace DarkestLike.InDungeon.BattleSystem
{
    public partial class BattleSubsystem
    {
        private readonly string characterUnitLayerName = "CharacterUnit";
        
        /// <summary>
        /// 마우스 위치의 캐릭터 유닛을 찾습니다.
        /// </summary>
        /// <param name="unit">찾은 유닛 (없으면 null)</param>
        /// <returns>유닛을 찾았는지 여부</returns>
        private bool TryGetCharacterUnitUnderMouse(out CharacterUnit unit)
        {
            unit = null;
            RaycastHit hit;

            if (!Physics.Raycast(InDungeonManager.Inst.ViewCamera.ScreenPointToRay(Input.mousePosition),
                    out hit, 500, LayerMask.GetMask(characterUnitLayerName)))
            {
                return false;
            }

            return hit.collider.TryGetComponent(out unit);
        }

        /// <summary>
        /// 마우스 호버 시 적 유닛 감지 및 호버 바 표시 (적 유닛 전용)
        /// </summary>
        private void HandleUnitHover()
        {
            // Timeline 재생 중에는 호버 기능 비활성화
            if (isTimelinePlaying)
            {
                // Timeline 재생 중 호버 해제
                if (currentHoveredUnit != null)
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearHover();
                    currentHoveredUnit = null;
                }
                return;
            }

            CharacterUnit hoveredUnit = null;

            // 마우스 아래 유닛 감지
            TryGetCharacterUnitUnderMouse(out hoveredUnit);

            // 호버 상태 변경 확인 (최적화: 변경 시에만 UI 업데이트)
            if (hoveredUnit != currentHoveredUnit)
            {
                // 이전 호버 해제
                if (currentHoveredUnit != null)
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearHover();
                }

                // 새 유닛 호버 (적 유닛만)
                if (hoveredUnit != null && hoveredUnit.IsAlive && hoveredUnit.IsEnemyUnit)
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.HoverEnemyUnit(hoveredUnit.transform);
                }

                currentHoveredUnit = hoveredUnit;
            }
        }

        /// <summary>
        /// 유닛 클릭 처리 (배틀 전 - 플레이어 유닛 선택)
        /// </summary>
        private void HandleUnitClick()
        {
            if (!TryGetCharacterUnitUnderMouse(out CharacterUnit clickedUnit))
                return;

            if (!clickedUnit.IsEnemyUnit)
            {
                InDungeonManager.Inst.SelectPlayerUnit(clickedUnit);
            }
        }

        /// <summary>
        /// 배틀 중 타겟 클릭 처리 (적 또는 아군) - Multi 타겟 지원
        /// </summary>
        private void HandleTargetClick()
        {
            if (!TryGetCharacterUnitUnderMouse(out CharacterUnit clickedUnit))
                return;

            if (!clickedUnit.IsAlive || SelectedSkill == null)
                return;

            // 지원 스킬 → 아군 타겟팅
            if (IsSupportiveSkill(SelectedSkill))
            {
                if (!clickedUnit.IsPlayerUnit)
                {
                    Debug.Log($"[BattleSubsystem] 지원 스킬은 아군만 타겟할 수 있습니다.");
                    return;
                }

                // Multi 타겟 스킬
                if (IsMultiTargetSkill(SelectedSkill))
                {
                    // 클릭한 아군의 영역 검증
                    if (!ValidateAllyTarget(clickedUnit, SelectedSkill, SelectedPlayerUnit))
                    {
                        Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}이(가) 속한 영역을 타겟할 수 없습니다.");
                        return;
                    }

                    // 영역 내 모든 아군 선택
                    List<CharacterUnit> targetedAllies = GetTargetedUnitsInArea(clickedUnit, SelectedSkill, false);

                    if (targetedAllies.Count > 0)
                    {
                        // 다중 아군 선택 (첫 번째 유닛을 대표로 설정)
                        SelectedAllyUnit = targetedAllies[0];
                        Debug.Log($"[BattleSubsystem] 아군 영역 선택: {targetedAllies.Count}명");
                    }
                }
                // Single 타겟 스킬 (기존 로직)
                else
                {
                    if (ValidateAllyTarget(clickedUnit, SelectedSkill, SelectedPlayerUnit))
                    {
                        SelectAlly(clickedUnit);
                    }
                    else
                    {
                        Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}은(는) 타겟할 수 없습니다.");
                    }
                }
            }
            // 공격 스킬 → 적 타겟팅
            else
            {
                if (!clickedUnit.IsEnemyUnit)
                {
                    Debug.Log($"[BattleSubsystem] 공격 스킬은 적만 타겟할 수 있습니다.");
                    return;
                }

                // Multi 타겟 스킬
                if (IsMultiTargetSkill(SelectedSkill))
                {
                    // 클릭한 적의 영역 검증
                    if (!ValidateTarget(clickedUnit, SelectedSkill))
                    {
                        Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}이(가) 속한 영역을 공격할 수 없습니다.");
                        return;
                    }

                    // 영역 내 모든 적 선택
                    List<CharacterUnit> targetedEnemies = GetTargetedUnitsInArea(clickedUnit, SelectedSkill, true);

                    if (targetedEnemies.Count > 0)
                    {
                        // 다중 적 선택 (첫 번째 유닛을 대표로 설정)
                        InDungeonManager.Inst.SelectEnemyUnit(targetedEnemies[0]);
                        Debug.Log($"[BattleSubsystem] 적 영역 선택: {targetedEnemies.Count}명");
                    }
                }
                // Single 타겟 스킬 (기존 로직)
                else
                {
                    if (ValidateTarget(clickedUnit, SelectedSkill))
                    {
                        InDungeonManager.Inst.SelectEnemyUnit(clickedUnit);
                    }
                    else
                    {
                        Debug.Log($"[BattleSubsystem] {clickedUnit.CharacterName}은(는) 공격할 수 없습니다.");
                    }
                }
            }
        }
    }
}