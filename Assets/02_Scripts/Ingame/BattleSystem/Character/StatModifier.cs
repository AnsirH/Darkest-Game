using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifier
{
    public string StatName; // 예: "Attack", "Defense"
    public int ModifierValue; // 증가 또는 감소 값
    public int TurnsRemaining; // 남은 턴 수

    public StatModifier(string statName, int modifierValue, int turnsRemaining)
    {
        StatName = statName;
        ModifierValue = modifierValue;
        TurnsRemaining = turnsRemaining;
    }
}
