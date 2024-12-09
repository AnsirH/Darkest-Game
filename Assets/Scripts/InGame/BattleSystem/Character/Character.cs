using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public CharacterStats Stats;
    public int level;
    private List<StatModifier> ActiveModifiers = new();

    public int MaxHealth => Stats.MaxHealth + (level - 1) * (int)(Stats.MaxHealth * 1.5f);
    public int Attack => Stats.TotalAttack + (level - 1) * (int)(Stats.Attack * 1.5f);
    public int Defense => Stats.TotalDefense + (level - 1) * (int)(Stats.Defense * 1.5f);
    public int Speed => Stats.TotalSpeed + (level - 1) * (int)(Stats.Speed * 1.5f);
    public int Accuracy => Stats.TotalAccuracy + (level - 1) * (int)(Stats.Accuracy * 1.5f);
    public int Evasion => Stats.TotalEvasion + (level - 1) * (int)(Stats.Evasion * 1.5f);

    public Character(string name, int maxHealth, int attack, int defense, int speed, int accuracy, int evasion)
    {
        level = 1;
        Stats = new CharacterStats(name, maxHealth, attack, defense, speed, accuracy, evasion);
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
            case "Accuracy":
                Stats.BuffedAccuracy += value;
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
}
