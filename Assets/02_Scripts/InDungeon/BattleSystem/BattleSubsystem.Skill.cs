using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Unit;
using UnityEngine;

namespace DarkestLike.InDungeon.BattleSystem
{
    public partial class BattleSubsystem
    {
        /// <summary>
        /// 스킬 실행
        /// </summary>
        private IEnumerator ExecuteSkill(CharacterUnit caster, SkillBase skill, CharacterUnit target)
        {
            battleState = BattleState.Action;

            // 1. 포지션 검증
            if (!ValidateSkillPosition(caster, skill))
            {
                Debug.LogWarning($"[ExecuteSkill] {caster.CharacterName}의 포지션에서 {skill.description}를 사용할 수 없습니다.");
                yield break;
            }

            // 2. 타겟 검증 (지원 스킬이면 아군 검증)
            bool isValidTarget = IsSupportiveSkill(skill)
                ? ValidateAllyTarget(target, skill, caster)
                : ValidateTarget(target, skill);

            if (!isValidTarget)
            {
                Debug.LogWarning($"[ExecuteSkill] {target.CharacterName}은(는) 유효한 타겟이 아닙니다.");
                yield break;
            }

            // 3. 데미지/힐 계산 (적용은 하지 않음, Timeline 재생 후 적용)
            int healAmount = 0;
            DamageResult damageResult = default;
            StatusEffect? statusEffectToApply = null;

            if (IsSupportiveSkill(skill))
            {
                // 3a. 힐링 계산
                if (skill.isHealing)
                {
                    // attackRatio를 사용하여 회복량 계산 (데미지와 동일한 방식)
                    healAmount = (int)(caster.CharacterData.Attack * (skill.attackRatio / 100f));

                    // 힐 텍스트 값 설정 (Timeline 재생 전에 값 설정)
                    SetHealText(healAmount);

                    Debug.Log($"[Heal] {caster.CharacterName}이(가) {target.CharacterName}에게 {healAmount} 회복 예정 (Timeline 재생 후 적용)");
                }

                // 3b. 버프 계산
                if (skill.appliesStatusEffect && skill.statusEffectType == StatusEffectType.Buff)
                {
                    int roll = Random.Range(0, 100);
                    if (roll < skill.statusEffectChance)
                    {
                        statusEffectToApply = new StatusEffect(
                            skill.statusEffectType,
                            skill.statusEffectDuration,
                            skill.statusEffectValue,
                            skill.description
                        );

                        Debug.Log($"[Buff] {target.CharacterName}에게 {statusEffectToApply.type} 효과 적용 예정");
                    }
                }
            }
            // 4. 공격 스킬 계산
            else
            {
                damageResult = DamageCalculator.CalculateDamage(caster, target, skill);

                if (damageResult.isMiss)
                {
                    Debug.Log($"[Combat] {caster.CharacterName}의 공격이 빗나갔습니다!");
                }
                else
                {
                    // 데미지 텍스트 값 설정 (Timeline 재생 전에 값 설정)
                    SetFloatingText(damageResult.damage, damageResult.isCrit);

                    Debug.Log($"[Combat] {caster.CharacterName}이(가) {target.CharacterName}에게 {damageResult.damage} 데미지 예정 (Timeline 재생 후 적용)");

                    // 상태이상 계산
                    if (skill.appliesStatusEffect)
                    {
                        int roll = Random.Range(0, 100);
                        if (roll < skill.statusEffectChance)
                        {
                            statusEffectToApply = new StatusEffect(
                                skill.statusEffectType,
                                skill.statusEffectDuration,
                                skill.statusEffectValue,
                                skill.description
                            );

                            Debug.Log($"[StatusEffect] {target.CharacterName}에게 {statusEffectToApply.type} 효과 적용 예정");
                        }
                    }
                }
            }

            // 5. Timeline 재생 (값 설정 후)
            if (skill.timelineAsset != null)
            {
                List<CharacterUnit> targets = new List<CharacterUnit> { target };
                yield return StartCoroutine(PlaySkillAnimation(caster, skill, targets));
            }
            else
            {
                // Fallback: TimelineAsset이 없으면 기본 대기 시간 사용
                Debug.Log($"[Skill] TimelineAsset 없음, 기본 대기 시간 사용: {skill.description}");
                yield return new WaitForSeconds(0.5f);
            }

            // 6. 데미지/힐 적용 (Timeline 재생 후!)
            if (IsSupportiveSkill(skill))
            {
                if (skill.isHealing)
                {
                    int healedAmount = target.CharacterData.Heal(healAmount);

                    Debug.Log($"[Heal] {caster.CharacterName}이(가) {target.CharacterName}을(를) {healedAmount} 회복!");
                    Debug.Log($"[Heal] {target.CharacterName} HP: {target.CharacterData.CurrentHealth}/{target.CharacterData.MaxHealth}");

                    // HP바 업데이트
                    InDungeonManager.Inst.UISubsystem.UpdateHpBar(target);
                }

                if (skill.appliesStatusEffect && skill.statusEffectType == StatusEffectType.Buff)
                {
                    target.CharacterData.AddStatusEffect(statusEffectToApply);
                    Debug.Log($"[Buff] {target.CharacterName}에게 {statusEffectToApply.type} 효과 적용 완료!");
                }
            }
            else
            {
                if (!damageResult.isMiss)
                {
                    // 데미지 적용
                    target.TakeDamage(damageResult.damage);

                    Debug.Log($"[Combat] {caster.CharacterName}이(가) {target.CharacterName}에게 {damageResult.damage} 데미지 적용 완료!");
                    Debug.Log($"[Combat] {target.CharacterName} HP: {target.CharacterData.CurrentHealth}/{target.CharacterData.MaxHealth}");

                    // HP바 업데이트
                    InDungeonManager.Inst.UISubsystem.UpdateHpBar(target);

                    if (skill.appliesStatusEffect)
                    {
                        target.CharacterData.AddStatusEffect(statusEffectToApply);
                        Debug.Log($"[StatusEffect] {target.CharacterName}에게 {statusEffectToApply.type} 효과 적용 완료!");
                    }
                }
            }

            // 7. 사망 처리 (Timeline 재생 및 데미지 적용 후)
            if (!IsSupportiveSkill(skill) && target.IsDead)
            {
                yield return StartCoroutine(HandleUnitDeath(target));
            }

            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// 다중 타겟에게 스킬 실행 (Multi 타겟용)
        /// </summary>
        private IEnumerator ExecuteSkillOnMultipleTargets(CharacterUnit caster, SkillBase skill, List<CharacterUnit> targets)
        {
            battleState = BattleState.Action;

            // 1. 포지션 검증
            if (!ValidateSkillPosition(caster, skill))
            {
                Debug.LogWarning($"[ExecuteSkill] {caster.CharacterName}의 포지션에서 {skill.description}를 사용할 수 없습니다.");
                yield break;
            }

            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning($"[ExecuteSkillOnMultipleTargets] 타겟이 없습니다.");
                yield break;
            }

            Debug.Log($"[ExecuteSkillOnMultipleTargets] {caster.CharacterName}이(가) {targets.Count}명에게 {skill.description} 사용");

            // 2. 데미지/힐 계산 (적용은 하지 않음, Timeline 재생 후 적용)
            List<int> healAmounts = new List<int>();
            List<DamageResult> damageResults = new List<DamageResult>();
            List<StatusEffect?> statusEffectsToApply = new List<StatusEffect>();
            List<(int damage, bool isCritical, bool isMiss)> healList = new List<(int, bool, bool)>();
            List<(int damage, bool isCritical, bool isMiss)> damageList = new List<(int, bool, bool)>();

            if (IsSupportiveSkill(skill))
            {
                foreach (var target in targets)
                {
                    if (target == null || !target.IsAlive)
                    {
                        healAmounts.Add(0);
                        statusEffectsToApply.Add(null);
                        continue;
                    }

                    // 힐링 계산
                    if (skill.isHealing)
                    {
                        int healAmount = (int)(caster.CharacterData.Attack * (skill.attackRatio / 100f));
                        healAmounts.Add(healAmount);
                        healList.Add((healAmount, false, false));

                        Debug.Log($"[Heal] {caster.CharacterName}이(가) {target.CharacterName}에게 {healAmount} 회복 예정 (Timeline 재생 후 적용)");
                    }
                    else
                    {
                        healAmounts.Add(0);
                    }

                    // 버프 계산
                    if (skill.appliesStatusEffect && skill.statusEffectType == StatusEffectType.Buff)
                    {
                        int roll = Random.Range(0, 100);
                        if (roll < skill.statusEffectChance)
                        {
                            var effect = new StatusEffect(
                                skill.statusEffectType,
                                skill.statusEffectDuration,
                                skill.statusEffectValue,
                                skill.description
                            );
                            statusEffectsToApply.Add(effect);

                            Debug.Log($"[Buff] {target.CharacterName}에게 {effect.type} 효과 적용 예정");
                        }
                        else
                        {
                            statusEffectsToApply.Add(null);
                        }
                    }
                    else
                    {
                        statusEffectsToApply.Add(null);
                    }
                }

                // Multi 힐 텍스트 설정 (Timeline 재생 전에 값 설정)
                if (healList.Count > 0)
                {
                    SetMultiFloatingTexts(healList, isHealing: true);
                }
            }
            // 3. 공격 스킬 계산
            else
            {
                foreach (var target in targets)
                {
                    if (target == null || !target.IsAlive)
                    {
                        damageResults.Add(default);
                        statusEffectsToApply.Add(null);
                        continue;
                    }

                    // 데미지 계산
                    DamageResult result = DamageCalculator.CalculateDamage(caster, target, skill);
                    damageResults.Add(result);

                    if (result.isMiss)
                    {
                        Debug.Log($"[Combat] {caster.CharacterName}의 {target.CharacterName}에 대한 공격이 빗나갔습니다!");
                        damageList.Add((0, false, true));
                        statusEffectsToApply.Add(null);
                    }
                    else
                    {
                        Debug.Log($"[Combat] {caster.CharacterName}이(가) {target.CharacterName}에게 {result.damage} 데미지 예정 (Timeline 재생 후 적용)");

                        // 데미지 정보 수집
                        damageList.Add((result.damage, result.isCrit, false));

                        // 상태이상 계산
                        if (skill.appliesStatusEffect)
                        {
                            int roll = Random.Range(0, 100);
                            if (roll < skill.statusEffectChance)
                            {
                                var effect = new StatusEffect(
                                    skill.statusEffectType,
                                    skill.statusEffectDuration,
                                    skill.statusEffectValue,
                                    skill.description
                                );
                                statusEffectsToApply.Add(effect);

                                Debug.Log($"[StatusEffect] {target.CharacterName}에게 {effect.type} 효과 적용 예정");
                            }
                            else
                            {
                                statusEffectsToApply.Add(null);
                            }
                        }
                        else
                        {
                            statusEffectsToApply.Add(null);
                        }
                    }
                }

                // Multi 타겟 FloatingText 설정 (Timeline 재생 전에 값 설정)
                SetMultiFloatingTexts(damageList);
            }

            // 4. Timeline 재생 (값 설정 후)
            if (skill.timelineAsset != null)
            {
                yield return StartCoroutine(PlaySkillAnimation(caster, skill, targets));
            }
            else
            {
                // Fallback: TimelineAsset이 없으면 기본 대기 시간 사용
                Debug.Log($"[Skill] TimelineAsset 없음, 기본 대기 시간 사용: {skill.description}");
                yield return new WaitForSeconds(0.5f);
            }

            // 5. 데미지/힐 적용 (Timeline 재생 후!)
            List<CharacterUnit> deadUnits = new List<CharacterUnit>();
            if (IsSupportiveSkill(skill))
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null || !targets[i].IsAlive) continue;

                    if (skill.isHealing && i < healAmounts.Count && healAmounts[i] > 0)
                    {
                        int healedAmount = targets[i].CharacterData.Heal(healAmounts[i]);

                        Debug.Log($"[Heal] {caster.CharacterName}이(가) {targets[i].CharacterName}을(를) {healedAmount} 회복!");
                        Debug.Log($"[Heal] {targets[i].CharacterName} HP: {targets[i].CharacterData.CurrentHealth}/{targets[i].CharacterData.MaxHealth}");

                        // HP바 업데이트
                        InDungeonManager.Inst.UISubsystem.UpdateHpBar(targets[i]);
                    }

                    if (skill.appliesStatusEffect && i <  statusEffectsToApply.Count)
                    {
                        targets[i].CharacterData.AddStatusEffect(statusEffectsToApply[i]);
                        Debug.Log($"[Buff] {targets[i].CharacterName}에게 {statusEffectsToApply[i].type} 효과 적용 완료!");
                    }
                }
            }
            else
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null || !targets[i].IsAlive) continue;
                    if (i >= damageResults.Count) continue;

                    if (!damageResults[i].isMiss)
                    {
                        // 데미지 적용
                        targets[i].TakeDamage(damageResults[i].damage);

                        Debug.Log($"[Combat] {caster.CharacterName}이(가) {targets[i].CharacterName}에게 {damageResults[i].damage} 데미지 적용 완료!");
                        Debug.Log($"[Combat] {targets[i].CharacterName} HP: {targets[i].CharacterData.CurrentHealth}/{targets[i].CharacterData.MaxHealth}");

                        // HP바 업데이트
                        InDungeonManager.Inst.UISubsystem.UpdateHpBar(targets[i]);

                        if (skill.appliesStatusEffect && i <  statusEffectsToApply.Count)
                        {
                            targets[i].CharacterData.AddStatusEffect(statusEffectsToApply[i]);
                            Debug.Log($"[StatusEffect] {targets[i].CharacterName}에게 {statusEffectsToApply[i].type} 효과 적용 완료!");
                        }

                        // 사망 체크 (나중에 일괄 처리)
                        if (targets[i].IsDead && !deadUnits.Contains(targets[i]))
                        {
                            deadUnits.Add(targets[i]);
                        }
                    }
                }
            }

            // 6. 사망 처리 (Timeline 재생 및 데미지 적용 후)
            if (deadUnits.Count > 0)
            {
                yield return StartCoroutine(HandleMultipleUnitDeaths(deadUnits));
            }

            yield return new WaitForSeconds(0.5f);
        }

        public void SetSelectedSkill(SkillBase skill)
        {
            SelectedSkill = skill;

            // 스킬 선택 시 타겟 가능한 유닛 표시
            if (skill != null && isBattleActive && battleState == BattleState.PlayerTurn)
            {
                bool isSupportive = IsSupportiveSkill(skill);
                var targetableUnits = isSupportive
                    ? GetTargetableAllies(skill, SelectedPlayerUnit)
                    : GetTargetableEnemies(skill);

                if (IsMultiTargetSkill(skill))
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowConnectionIndicators(
                        targetableUnits,
                        skill.canTargetFront,
                        skill.canTargetBack,
                        isSupportive);
                }
                else
                {
                    InDungeonManager.Inst.UISubsystem.SelectedUnitBarController.ShowTargetableUnits(
                        targetableUnits,
                        isSupportive);
                }
            }
        }

        /// <summary>
        /// 스킬이 아군 대상 지원 스킬인지 확인
        /// </summary>
        private bool IsSupportiveSkill(SkillBase skill)
        {
            if (skill == null) return false;
            return skill.isHealing ||
                   (skill.appliesStatusEffect && skill.statusEffectType == StatusEffectType.Buff);
        }

        /// <summary>
        /// 스킬이 Multi 타겟 스킬인지 확인
        /// </summary>
        private bool IsMultiTargetSkill(SkillBase skill)
        {
            if (skill == null) return false;
            return skill.targetType == TargetType.Multi;
        }
    }
}