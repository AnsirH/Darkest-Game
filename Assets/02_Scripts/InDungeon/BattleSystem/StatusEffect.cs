using System;

namespace DarkestLike.InDungeon.BattleSystem
{
    public enum StatusEffectType
    {
        Poison,     // 독 (턴당 HP 감소)
        Bleed,      // 출혈 (턴당 HP 감소)
        Stun,       // 기절 (행동 불가)
        Buff,       // 스탯 증가
        Debuff      // 스탯 감소
    }

    [Serializable]
    public class StatusEffect
    {
        public StatusEffectType type;
        public int duration;        // 남은 턴 수
        public int value;          // 효과 수치 (독 데미지, 스탯 변화량 등)
        public string effectName;

        public StatusEffect(StatusEffectType type, int duration, int value, string name)
        {
            this.type = type;
            this.duration = duration;
            this.value = value;
            this.effectName = name;
        }

        /// <summary>
        /// 턴 종료시 호출하여 지속시간을 감소시킵니다.
        /// </summary>
        public void DecreaseDuration()
        {
            duration--;
        }

        /// <summary>
        /// 효과가 만료되었는지 확인합니다.
        /// </summary>
        public bool IsExpired => duration <= 0;
    }

    [Serializable]
    public struct StatusEffectResult
    {
        public StatusEffect effect;
        public int damageDealt;

        public StatusEffectResult(StatusEffect effect, int damage)
        {
            this.effect = effect;
            this.damageDealt = damage;
        }
    }
}
