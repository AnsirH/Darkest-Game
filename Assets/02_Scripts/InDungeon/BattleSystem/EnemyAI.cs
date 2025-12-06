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
        /// <returns>AI의 결정</returns>
        public static AIDecision MakeDecision(CharacterUnit enemy, List<CharacterUnit> playerUnits)
        {
            AIDecision decision = new AIDecision();

            // 1. 사용 가능한 스킬 필터링
            List<SkillBase> availableSkills = enemy.CharacterData.Base.skills
                .Where(skill => skill != null && ValidateSkillPosition(enemy, skill))
                .ToList();

            if (availableSkills.Count == 0)
            {
                Debug.LogError($"Enemy {enemy.CharacterName} has no available skills!");
                return null;
            }

            // 2. 스킬 선택 (현재는 랜덤, 추후 가중치 시스템 추가 가능)
            decision.selectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];

            // 3. 타겟 선택
            decision.target = SelectTarget(playerUnits, decision.selectedSkill);

            return decision;
        }

        /// <summary>
        /// 타겟을 선택합니다.
        /// </summary>
        private static CharacterUnit SelectTarget(List<CharacterUnit> playerUnits, SkillBase skill)
        {
            // 타겟 가능한 유닛 필터링
            List<CharacterUnit> validTargets = playerUnits
                .Where(unit => unit.IsAlive)
                .Where(unit => ValidateTarget(unit, skill))
                .ToList();

            if (validTargets.Count == 0) return null;

            // 우선순위 로직: HP가 가장 낮은 유닛 우선
            validTargets.Sort((a, b) => a.CharacterData.CurrentHealth.CompareTo(b.CharacterData.CurrentHealth));

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
