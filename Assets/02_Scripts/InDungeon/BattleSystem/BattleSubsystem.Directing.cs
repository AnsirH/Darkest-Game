using System.Collections;
using System.Collections.Generic;
using _02_Scripts.InDungeon.UI;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Unit;
using UnityEngine;
using UnityEngine.Playables;

namespace DarkestLike.InDungeon.BattleSystem
{
    public partial class BattleSubsystem
    {
        /// <summary>
        /// 스킬 Timeline 애니메이션을 재생합니다.
        /// </summary>
        /// <param name="caster">스킬 시전자</param>
        /// <param name="skill">실행할 스킬</param>
        /// <param name="targets">타겟 유닛들</param>
        private IEnumerator PlaySkillAnimation(CharacterUnit caster, SkillBase skill, List<CharacterUnit> targets)
        {
            if (skill.timelineAsset == null || skillDirector == null)
            {
                Debug.LogWarning($"[Timeline] {skill.description}의 TimelineAsset 또는 PlayableDirector가 없습니다.");
                yield break;
            }

            // 1. Timeline Asset 설정
            skillDirector.playableAsset = skill.timelineAsset;

            // 2. Track Binding 설정
            BindTimelineTracks(caster, targets);

            // 3. 액션 포지션으로 이동
            yield return StartCoroutine(MoveUnitsToActionPosition(caster, targets, 0.1f));

            // 4. HP바, 상태이상 바, 선택바 비활성화 (caster + targets)
            List<CharacterUnit> participatingUnits = new List<CharacterUnit> { caster };
            if (targets != null)
            {
                participatingUnits.AddRange(targets);
            }
            InDungeonManager.Inst.UISubsystem.SetMultipleHpBarsActive(participatingUnits, false);
            InDungeonManager.Inst.UISubsystem.SetMultipleStatusEffectBarsActive(participatingUnits, false);
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.SetActiveSelectedBar(false);
            InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ClearTargetableUnits();

            // 5. Timeline 재생
            isTimelinePlaying = true;
            skillDirector.Play();

            // 6. Timeline 완료 대기
            while (isTimelinePlaying)
            {
                yield return null;
            }

            // 7. HP바, 상태이상 바 활성화 (caster + targets)
            InDungeonManager.Inst.UISubsystem.SetMultipleHpBarsActive(participatingUnits, true);
            InDungeonManager.Inst.UISubsystem.SetMultipleStatusEffectBarsActive(participatingUnits, true);

            // 8. 원래 포지션으로 복귀
            yield return StartCoroutine(ReturnUnitsToOriginalPosition(caster, targets, 0.1f));
        }

        /// <summary>
        /// 단일 타겟의 데미지 값을 FloatingText1에 설정합니다.
        /// </summary>
        /// <param name="damage">데미지 값</param>
        /// <param name="isCritical">크리티컬 여부</param>
        private void SetFloatingText(int damage, bool isCritical = false)
        {
            if (floatingText1 == null)
            {
                Debug.LogWarning("[FloatingText] FloatingText1이 할당되지 않았습니다.");
                return;
            }

            // 데미지 값 설정 (Animation Clip이 활성화/애니메이션 제어)
            floatingText1.SetDamage(damage, isCritical);

            Debug.Log($"[FloatingText] FloatingText1에 {damage} 설정 (크리티컬: {isCritical})");
        }

        /// <summary>
        /// Multi 타겟의 데미지 값들을 FloatingText1~4에 설정합니다.
        /// </summary>
        /// <param name="damages">각 타겟의 (데미지, 크리티컬, Miss여부) 리스트</param>
        /// <param name="isHealing">힐링 여부 (기본: false)</param>
        private void SetMultiFloatingTexts(List<(int damage, bool isCritical, bool isMiss)> damages, bool isHealing = false)
        {
            FloatingTextController[] floatingTexts = { floatingText1, floatingText2, floatingText3, floatingText4 };

            for (int i = 0; i < damages.Count && i < floatingTexts.Length; i++)
            {
                var floatingTextCtrl = floatingTexts[i];
                if (floatingTextCtrl == null)
                {
                    Debug.LogWarning($"[FloatingText] FloatingText{i + 1}이 할당되지 않았습니다.");
                    continue;
                }

                // Miss, 데미지, 힐 값 설정 (Animation Clip이 활성화/애니메이션 제어)
                if (damages[i].isMiss)
                {
                    floatingTextCtrl.SetMiss();
                    Debug.Log($"[FloatingText] FloatingText{i + 1}에 Miss 설정");
                }
                else if (isHealing)
                {
                    floatingTextCtrl.SetHeal(damages[i].damage);
                    Debug.Log($"[FloatingText] FloatingText{i + 1}에 힐 {damages[i].damage} 설정");
                }
                else
                {
                    floatingTextCtrl.SetDamage(damages[i].damage, damages[i].isCritical);
                    Debug.Log($"[FloatingText] FloatingText{i + 1}에 {damages[i].damage} 설정");
                }
            }
        }

        /// <summary>
        /// 단일 타겟의 힐 값을 FloatingText1에 설정합니다.
        /// </summary>
        /// <param name="healAmount">회복량</param>
        private void SetHealText(int healAmount)
        {
            if (floatingText1 == null)
            {
                Debug.LogWarning("[FloatingText] FloatingText1이 할당되지 않았습니다.");
                return;
            }

            // 힐 값 설정 (Animation Clip이 활성화/애니메이션 제어)
            floatingText1.SetHeal(healAmount);

            Debug.Log($"[FloatingText] FloatingText1에 힐 {healAmount} 설정");
        }

        /// <summary>
        /// Timeline 재생 전에 참가 유닛들을 액션 포지션으로 이동시킵니다.
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <param name="targets">타겟 목록</param>
        /// <param name="moveTime">이동 시간</param>
        private IEnumerator MoveUnitsToActionPosition(CharacterUnit caster, List<CharacterUnit> targets, float moveTime = 0.1f)
        {
            // 1. 입력 검증
            if (caster == null)
            {
                Debug.LogWarning("[ActionPosition] Caster is null, skipping movement");
                yield break;
            }

            // 2. 참가 유닛 수집 (caster + targets)
            List<CharacterUnit> participatingUnits = new List<CharacterUnit> { caster };
            if (targets != null)
            {
                participatingUnits.AddRange(targets);
            }

            // 3. 각 유닛을 액션 포지션으로 이동
            foreach (var unit in participatingUnits)
            {
                if (unit == null || !unit.IsAlive) continue;

                // 3-1. 진영에 따라 액션 포지션 배열 선택
                Transform[] actionPositions = unit.IsPlayerUnit ? actionPlayerPositions : actionEnemyPositions;

                // 3-2. 배열 검증
                if (actionPositions == null || actionPositions.Length == 0)
                {
                    Debug.LogWarning($"[ActionPosition] Action positions not set for {(unit.IsPlayerUnit ? "Player" : "Enemy")} units");
                    continue;
                }

                // 3-3. 인덱스 범위 검증
                if (unit.PositionIndex >= actionPositions.Length)
                {
                    Debug.LogError($"[ActionPosition] {unit.CharacterName}'s PositionIndex ({unit.PositionIndex}) out of range");
                    continue;
                }

                // 3-4. 액션 포지션으로 이동
                unit.ChangePositionMaintainerTarget(actionPositions[unit.PositionIndex], moveTime);
                Debug.Log($"[ActionPosition] {unit.CharacterName} moving to action position {unit.PositionIndex}");
            }

            // 4. 이동 완료 대기 (moveTime + 버퍼)
            yield return new WaitForSeconds(moveTime + 0.1f);
        }

        /// <summary>
        /// Timeline 재생 완료 후 참가 유닛들을 원래 포지션으로 복귀시킵니다.
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <param name="targets">타겟 목록</param>
        /// <param name="moveTime">이동 시간</param>
        private IEnumerator ReturnUnitsToOriginalPosition(CharacterUnit caster, List<CharacterUnit> targets, float moveTime = 0.1f)
        {
            // 1. 입력 검증
            if (caster == null)
            {
                yield break; // 시전자가 죽었으면 조용히 종료
            }

            // 2. 참가 유닛 수집
            List<CharacterUnit> participatingUnits = new List<CharacterUnit> { caster };
            if (targets != null)
            {
                participatingUnits.AddRange(targets);
            }

            // 3. 각 유닛을 원래 포지션으로 복귀
            foreach (var unit in participatingUnits)
            {
                // 3-1. 생존 유닛만 복귀 (액션 중 죽었을 수 있음)
                if (unit == null || !unit.IsAlive)
                {
                    Debug.Log($"[ActionPosition] Skipping return for dead/null unit");
                    continue;
                }

                // 3-2. 진영에 따라 일반 포지션 배열 선택
                Transform[] regularPositions = unit.IsPlayerUnit ? playerPositions : enemyPositions;

                // 3-3. 원래 포지션으로 복귀
                unit.ChangePositionMaintainerTarget(regularPositions[unit.PositionIndex], moveTime);
                Debug.Log($"[ActionPosition] {unit.CharacterName} returning to position {unit.PositionIndex}");
            }

            // 4. 복귀 완료 대기
            yield return new WaitForSeconds(moveTime + 0.1f);
        }

        /// <summary>
        /// Timeline 재생 완료 콜백
        /// </summary>
        private void OnTimelineStopped(PlayableDirector director)
        {
            if (director == skillDirector)
            {
                isTimelinePlaying = false;
                Debug.Log("[Timeline] 재생 완료");
            }
        }

        /// <summary>
        /// Timeline의 Track들을 캐스터와 타겟 유닛에 바인딩합니다.
        /// </summary>
        private void BindTimelineTracks(CharacterUnit caster, List<CharacterUnit> targets)
        {
            if (skillDirector.playableAsset == null) return;

            foreach (var output in skillDirector.playableAsset.outputs)
            {
                string trackName = output.streamName;

                // 1. Caster Track: 시전자의 Animator 바인딩
                if (trackName.Contains("Caster") && caster != null)
                {
                    if (caster.AnimController != null && caster.AnimController.anim != null)
                    {
                        skillDirector.SetGenericBinding(output.sourceObject, caster.AnimController.anim);
                        Debug.Log($"[Timeline] Caster Track '{trackName}' → {caster.CharacterName}");
                    }
                }

                // 2. Target Track: 첫 번째 타겟의 Animator 바인딩
                else if (trackName.Contains("Target") && targets != null && targets.Count > 0)
                {
                    if (trackName == "Target" || trackName.Contains("Target1"))
                    {
                        var target = targets[0];
                        if (target != null && target.AnimController != null && target.AnimController.anim != null)
                        {
                            skillDirector.SetGenericBinding(output.sourceObject, target.AnimController.anim);
                            Debug.Log($"[Timeline] Target Track '{trackName}' → {target.CharacterName}");
                        }
                    }
                    // Multi 타겟 스킬: Target2, Target3 처리
                    else if (trackName.Contains("Target2") && targets.Count > 1)
                    {
                        var target = targets[1];
                        if (target != null && target.AnimController != null && target.AnimController.anim != null)
                        {
                            skillDirector.SetGenericBinding(output.sourceObject, target.AnimController.anim);
                            Debug.Log($"[Timeline] Target Track '{trackName}' → {target.CharacterName}");
                        }
                    }
                    else if (trackName.Contains("Target3") && targets.Count > 2)
                    {
                        var target = targets[2];
                        if (target != null && target.AnimController != null && target.AnimController.anim != null)
                        {
                            skillDirector.SetGenericBinding(output.sourceObject, target.AnimController.anim);
                            Debug.Log($"[Timeline] Target Track '{trackName}' → {target.CharacterName}");
                        }
                    }
                }

                // 3. Camera Track: 카메라의 Animator 바인딩
                else if (trackName.Contains("Camera"))
                {
                    Camera viewCamera = InDungeonManager.Inst.ViewCamera;
                    if (viewCamera != null)
                    {
                        Animator cameraAnimator = viewCamera.GetComponent<Animator>();
                        if (cameraAnimator != null)
                        {
                            skillDirector.SetGenericBinding(output.sourceObject, cameraAnimator);
                            Debug.Log($"[Timeline] Camera Track '{trackName}' → ViewCamera");
                        }
                        else
                        {
                            Debug.LogWarning($"[Timeline] ViewCamera에 Animator 컴포넌트가 없습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Timeline] ViewCamera를 찾을 수 없습니다.");
                    }
                }

                // 4. FloatingText Track: FloatingText의 Animator 바인딩
                else if (trackName.Contains("FloatingText"))
                {
                    FloatingTextController targetFloatingText = null;

                    // FloatingText1, FloatingText2, FloatingText3, FloatingText4 인덱스 추출
                    if (trackName.Contains("FloatingText1"))
                    {
                        targetFloatingText = floatingText1;
                    }
                    else if (trackName.Contains("FloatingText2"))
                    {
                        targetFloatingText = floatingText2;
                    }
                    else if (trackName.Contains("FloatingText3"))
                    {
                        targetFloatingText = floatingText3;
                    }
                    else if (trackName.Contains("FloatingText4"))
                    {
                        targetFloatingText = floatingText4;
                    }

                    // Animator 바인딩
                    if (targetFloatingText != null)
                    {
                        Animator floatingTextAnimator = targetFloatingText.GetAnimator();
                        if (floatingTextAnimator != null)
                        {
                            skillDirector.SetGenericBinding(output.sourceObject, floatingTextAnimator);
                            Debug.Log($"[Timeline] FloatingText Track '{trackName}' → {targetFloatingText.name}");
                        }
                        else
                        {
                            Debug.LogWarning($"[Timeline] {targetFloatingText.name}에 Animator가 없습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Timeline] FloatingText Track '{trackName}'에 해당하는 UI가 할당되지 않았습니다.");
                    }
                }
            }
        }
    }
}