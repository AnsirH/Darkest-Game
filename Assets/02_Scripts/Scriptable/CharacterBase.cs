using DarkestLike.InDungeon.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.ScriptableObj
{
    public enum CharacterType
    {
        Melee,  // 근거리
        Ranged  // 원거리
    }

    [CreateAssetMenu(fileName = "NewCharacterBase", menuName = "Create Data Asset/Create Character Base")]
    public class CharacterBase : ScriptableObject
    {
        [Header("Stats")]
        public int maxHp;
        public int attack;
        public int defense;
        public int speed;
        public int evasion;
        public CharacterType type;

        [Header("Model")]
        public GameObject modelPrefab;

        //[Header("Skill")]
        public SkillBase[] skills;
    }
}