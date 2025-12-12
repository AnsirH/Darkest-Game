using DarkestLike.InDungeon.Unit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DarkestLike.InDungeon.BattleSystem
{
    public static class EnemyAI
    {
        /// <summary>
        /// 적의 행동을 결정합니다.
        /// </summary>
        /// <param name="enemy">행동할 적 유닛</param>
        /// <param name="playerUnits">플레이어 유닛 목록</param>
        /// <param name="enemyUnits">적 유닛 목록 (버프/힐 스킬용)</param>
        /// <returns>AI의 결정</returns>
        public static AIDecision MakeDecision(CharacterUnit enemy, List<CharacterUnit> playerUnits, List<CharacterUnit> enemyUnits)
        {
            AIDecision decision = new AIDecision();

            // 1. 사용 가능한 스킬 필터링
            List<SkillBase> availableSkills = enemy.CharacterData.Base.skills
                .Where(skill => skill != null && ValidateSkillPosition(enemy, skill))
                .ToList();

            if (availableSkills.Count == 0)
            {
                Debug.LogError($"[AI] {enemy.CharacterName}이(가) 현재 위치({enemy.Position})에서 사용 가능한 스킬이 없습니다!");
                Debug.LogError($"[AI] 스킬 목록: {string.Join(", ", enemy.CharacterData.Base.skills.Select(s => s != null ? s.description : "null"))}");
                return null;
            }

            // 2. 스킬 선택 (현재는 랜덤, 추후 가중치 시스템 추가 가능)
            decision.selectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];

            // 3. 타겟 선택
            // 지원 스킬: 회복 또는 버프 (디버프는 적에게 사용하므로 제외)
            bool isSupportive = decision.selectedSkill.isHealing ||
                                (decision.selectedSkill.appliesStatusEffect &&
                                 decision.selectedSkill.statusEffectType == StatusEffectType.Buff);

            // 3-1. 자기 자신에게만 사용 가능한 스킬 (canTargetFront = false, canTargetBack = false)
            bool isSelfTargetOnly = isSupportive &&
                                    !decision.selectedSkill.canTargetFront &&
                                    !decision.selectedSkill.canTargetBack;

            if (isSelfTargetOnly)
            {
                // 자기 자신을 타겟으로 설정
                decision.target = enemy;
                Debug.Log($"[AI] {enemy.CharacterName}이(가) 자기 자신에게 {decision.selectedSkill.description} 사용");
            }
            // 3-2. 다른 유닛을 타겟으로 하는 스킬
            else
            {
                decision.target = SelectTarget(isSupportive ? enemyUnits : playerUnits, decision.selectedSkill, isSupportive);

                if (decision.target == null)
                {
                    string targetType = isSupportive ? "적 유닛(아군)" : "플레이어 유닛";
                    Debug.LogError($"[AI] {enemy.CharacterName}이(가) 스킬 {decision.selectedSkill.description}의 타겟을 찾지 못했습니다! (타겟: {targetType})");
                    Debug.LogError($"[AI] 플레이어 유닛 수: {playerUnits.Count}, 생존: {playerUnits.Count(u => u.IsAlive)}");
                    Debug.LogError($"[AI] 적 유닛 수: {enemyUnits.Count}, 생존: {enemyUnits.Count(u => u.IsAlive)}");
                    return null;
                }
            }

            return decision;
        }

        /// <summary>
        /// 타겟을 선택합니다.
        /// </summary>
        /// <param name="targetUnits">타겟 후보 유닛 리스트 (플레이어 또는 적 유닛)</param>
        /// <param name="skill">사용할 스킬</param>
        /// <param name="isSupportive">지원 스킬 여부</param>
        private static CharacterUnit SelectTarget(List<CharacterUnit> targetUnits, SkillBase skill, bool isSupportive)
        {
            // 타겟 가능한 유닛 필터링
            List<CharacterUnit> validTargets = targetUnits
                .Where(unit => unit.IsAlive)
                .Where(unit => ValidateTarget(unit, skill))
                .ToList();

            if (validTargets.Count == 0) return null;

            // 우선순위 로직
            if (isSupportive)
            {
                // 회복/버프: HP가 가장 낮은 유닛 우선
                validTargets.Sort((a, b) => a.CharacterData.CurrentHealth.CompareTo(b.CharacterData.CurrentHealth));
            }
            else
            {
                // 공격: HP가 가장 낮은 유닛 우선 (마무리 타격)
                validTargets.Sort((a, b) => a.CharacterData.CurrentHealth.CompareTo(b.CharacterData.CurrentHealth));
            }

            return validTargets[0];
        }

        /// <summary>
        /// 스킬을 해당 포지션에서 사용할 수 있는지 검증합니다.
        /// </summary>
        private static bool ValidateSkillPosition(CharacterUnit caster, SkillBase skill)
        {
            if (caster.Position == UnitPosition.Front && !skill.canUseFromFront) return false;
            if (caster.Position == UnitPosition.Back && !skill.canUseFromBack) return false;
            return true;
        }

        /// <summary>
        /// 타겟이 유효한지 검증합니다.
        /// </summary>
        private static bool ValidateTarget(CharacterUnit target, SkillBase skill)
        {
            if (target.Position == UnitPosition.Front && !skill.canTargetFront) return false;
            if (target.Position == UnitPosition.Back && !skill.canTargetBack) return false;
            return true;
        }
    }

    public class AIDecision
    {
        public SkillBase selectedSkill;
        public CharacterUnit target;
    }
}
