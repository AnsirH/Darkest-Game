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
        public int MaxHP;
        public int Attack;
        public int Defense;
        public int Speed;
        public int Evasion;
        public CharacterType Type;

        [Header("Model")]
        public GameObject ModelPrefab;

        //[Header("Skill")]
        //public MoveData
    }
}