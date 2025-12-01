using DarkestLike.ScriptableObj;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.Character
{
    
    public class CharacterData
    {
        public CharacterStats Stats;
        public int level;
        private List<StatModifier> ActiveModifiers = new();

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
        /// 캐릭터 정보를 문자열로 반환합니다.
        /// </summary>
        /// <returns>캐릭터 정보 문자열</returns>
        public override string ToString()
        {
            return $"{CharacterName} (Lv.{level}) HP: {CurrentHealth}/{MaxHealth} ATK:{Attack} DEF:{Defense} SPD:{Speed}";
        }
    }
}
