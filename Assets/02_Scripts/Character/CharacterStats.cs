using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    public string Name;
    public int MaxHealth;
    public int CurrentHealth;
    public int Attack;
    public int Defense;
    public int Speed;
    public int Evasion;

    public int BuffedAttack;
    public int BuffedDefense;
    public int BuffedSpeed;
    public int BuffedEvasion;

    public int TotalAttack => Attack + BuffedAttack;
    public int TotalDefense => Defense + BuffedDefense;
    public int TotalSpeed => Speed + BuffedSpeed;
    public int TotalEvasion => Evasion + BuffedEvasion;


    public CharacterStats(string name, int maxHealth, int attack, int defense, int speed, int evasion)
    {
        Name = name;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        Attack = attack;
        Defense = defense;
        Speed = speed;
        Evasion = evasion;

        BuffedAttack = 0;
        BuffedDefense = 0;
        BuffedSpeed = 0;
        BuffedEvasion = 0;
    }
}
