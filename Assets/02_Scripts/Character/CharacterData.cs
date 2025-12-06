using DarkestLike.ScriptableObj;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkestLike.InDungeon.BattleSystem;

namespace DarkestLike.Character
{

    public class CharacterData
    {
        public CharacterStats Stats;
        public int level;
        private List<StatModifier> ActiveModifiers = new();
        private List<StatusEffect> activeEffects = new List<StatusEffect>();

        // CharacterBase 참조 추가
        public CharacterBase Base { get; private set; }

        public int MaxHealth => Stats.MaxHealth;
        public int Attack => Stats.TotalAttack;
        public int Defense => Stats.TotalDefense;
        public int Speed => Stats.TotalSpeed;
        public int Evasion => Stats.TotalEvasion;

        // 현재 HP 관리
        public int CurrentHealth 
        { 
            get => Stats.CurrentHealth; 
            set => Stats.CurrentHealth = Mathf.Clamp(value, 0, MaxHealth); 
        }
        
        // 생존 상태
        public bool IsAlive => CurrentHealth > 0;
        public bool IsDead => CurrentHealth <= 0;

        // 캐릭터 이름
        public string CharacterName => Stats.Name;

        /// <summary>
        /// CharacterBase를 기반으로 CharacterData를 생성합니다.
        /// </summary>
        /// <param name="characterBase">캐릭터의 기본 데이터</param>
        /// <param name="characterName">캐릭터 이름</param>
        /// <param name="startingLevel">시작 레벨 (기본값: 1)</param>
        public CharacterData(CharacterBase characterBase, string characterName = "", int startingLevel = 1)
        {
            Base = characterBase;
            level = startingLevel;
            
            // CharacterBase의 데이터를 CharacterStats로 변환
            string name = string.IsNullOrEmpty(characterName) ? characterBase.name : characterName;
            Stats = new CharacterStats(
                name,
                characterBase.maxHp,
                characterBase.attack,
                characterBase.defense,
                characterBase.speed,
                characterBase.evasion
            );
        }

    public void ApplyModifier(StatModifier modifier)
    {
        ActiveModifiers.Add(modifier);
        UpdateStats(modifier, true);
    }

    public void UpdateTurn()
    {
        for (int i = ActiveModifiers.Count - 1; i >= 0; i--)
        {
            ActiveModifiers[i].TurnsRemaining--;
            if (ActiveModifiers[i].TurnsRemaining <= 0)
            {
                UpdateStats(ActiveModifiers[i], false);
                ActiveModifiers.RemoveAt(i);
            }
        }
    }

    private void UpdateStats(StatModifier modifier, bool isApplying)
    {
        int value = isApplying ? modifier.ModifierValue : -modifier.ModifierValue;

        switch (modifier.StatName)
        {
            case "Attack":
                Stats.BuffedAttack += value;
                break;
            case "Defense":
                Stats.BuffedDefense += value;
                break;
            case "Speed":
                Stats.BuffedSpeed += value;
                break;
            case "Evasion":
                Stats.BuffedEvasion += value;
                break;
        }
    }

        public void LevelUp()
        {
            level++;
        }

        /// <summary>
        /// HP를 회복합니다.
        /// </summary>
        /// <param name="healAmount">회복량</param>
        /// <returns>실제로 회복된 HP</returns>
        public int Heal(int healAmount)
        {
            int previousHealth = CurrentHealth;
            CurrentHealth += healAmount;
            return healAmount;
        }

        /// <summary>
        /// 캐릭터를 완전히 회복시킵니다.
        /// </summary>
        public void FullHeal()
        {
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// 데미지를 받습니다.
        /// </summary>
        /// <param name="damage">받을 데미지</param>
        public void TakeDamage(int damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        }

        #region Status Effects

        /// <summary>
        /// 상태이상을 추가합니다.
        /// </summary>
        public void AddStatusEffect(StatusEffect effect)
        {
            activeEffects.Add(effect);
            ApplyEffectImmediate(effect);
        }

        /// <summary>
        /// 상태이상을 제거합니다.
        /// </summary>
        public void RemoveStatusEffect(StatusEffect effect)
        {
            RemoveEffectImmediate(effect);
            activeEffects.Remove(effect);
        }

        /// <summary>
        /// 턴 종료시 상태이상을 처리합니다.
        /// </summary>
        public List<StatusEffectResult> ProcessEndOfTurn()
        {
            List<StatusEffectResult> results = new List<StatusEffectResult>();

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffects[i];

                // DOT 데미지 적용
                if (effect.type == StatusEffectType.Poison || effect.type == StatusEffectType.Bleed)
                {
                    TakeDamage(effect.value);
                    results.Add(new StatusEffectResult(effect, effect.value));
                }

                // 지속시간 감소
                effect.DecreaseDuration();

                if (effect.IsExpired)
                {
                    RemoveStatusEffect(effect);
                }
            }

            return results;
        }

        /// <summary>
        /// 특정 타입의 상태이상을 가지고 있는지 확인합니다.
        /// </summary>
        public bool HasEffect(StatusEffectType type)
        {
            return activeEffects.Exists(e => e.type == type);
        }

        /// <summary>
        /// 상태이상의 즉시 효과를 적용합니다 (버프/디버프).
        /// </summary>
        private void ApplyEffectImmediate(StatusEffect effect)
        {
            if (effect.type == StatusEffectType.Buff)
            {
                // 버프 효과 적용 (StatModifier 시스템 활용)
                ApplyModifier(new StatModifier("Attack", effect.value, effect.duration));
            }
            else if (effect.type == StatusEffectType.Debuff)
            {
                // 디버프 효과 적용
                ApplyModifier(new StatModifier("Attack", -effect.value, effect.duration));
            }
        }

        /// <summary>
        /// 상태이상의 즉시 효과를 제거합니다.
        /// </summary>
        private void RemoveEffectImmediate(StatusEffect effect)
        {
            // 만료시 StatModifier는 UpdateTurn()에서 자동으로 제거됨
        }

        #endregion

        /// <summary>
        /// 캐릭터 정보를 문자열로 반환합니다.
        /// </summary>
        /// <returns>캐릭터 정보 문자열</returns>
        public override string ToString()
        {
            return $"{CharacterName} (Lv.{level}) HP: {CurrentHealth}/{MaxHealth} ATK:{Attack} DEF:{Defense} SPD:{Speed}";
        }
    }
}
