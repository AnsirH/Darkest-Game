using DarkestLike.InDungeon.Unit;
using UnityEngine;

namespace DarkestLike.InDungeon.BattleSystem
{
    public static class DamageCalculator
    {
        /// <summary>
        /// 데미지를 계산합니다.
        /// </summary>
        /// <param name="attacker">공격자</param>
        /// <param name="target">대상</param>
        /// <param name="skill">사용한 스킬</param>
        /// <returns>데미지 계산 결과</returns>
        public static DamageResult CalculateDamage(CharacterUnit attacker, CharacterUnit target, SkillBase skill)
        {
            DamageResult result = new DamageResult();

            // 1. 명중 판정
            int hitChance = skill.accuracy;
            // TODO: 횃불 시스템 연동시 보정 추가

            int roll = Random.Range(0, 100);
            if (roll >= hitChance)
            {
                result.isMiss = true;
                return result;
            }

            // 2. 크리티컬 판정 (추후 구현)
            // int critChance = attacker.CharacterData.Stats.CritChance;
            result.isCrit = false;

            // 3. 기본 데미지 계산
            float baseDamage = attacker.CharacterData.Attack * (skill.attackRatio / 100f);

            // 4. 방어력 적용
            float finalDamage = Mathf.Max(1, baseDamage - target.CharacterData.Defense);

            // 5. 랜덤 변동 (±15%)
            finalDamage *= Random.Range(0.85f, 1.15f);

            result.damage = Mathf.RoundToInt(finalDamage);
            result.isMiss = false;

            return result;
        }
    }

    public struct DamageResult
    {
        public int damage;
        public bool isMiss;
        public bool isCrit;
    }

    public struct DamageInfo
    {
        public CharacterUnit target;
        public int damage;

        public DamageInfo(CharacterUnit target, int damage)
        {
            this.target = target;
            this.damage = damage;
        }
    }
}
