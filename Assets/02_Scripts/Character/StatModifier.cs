using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifier
{
    public string StatName; // ��: "Attack", "Defense"
    public int ModifierValue; // ���� �Ǵ� ���� ��
    public int TurnsRemaining; // ���� �� ��

    public StatModifier(string statName, int modifierValue, int turnsRemaining)
    {
        StatName = statName;
        ModifierValue = modifierValue;
        TurnsRemaining = turnsRemaining;
    }
}
