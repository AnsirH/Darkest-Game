using DarkestLike.Character;
using DarkestLike.InDungeon.CharacterUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon
{
    public class UnitSubsystem : InDungeonSubsystem
    {
        [Header("References")]
        [SerializeField] List<CharacterUnit.CharacterUnit> characterUnits;

        // Properties
        public List<CharacterUnit.CharacterUnit> CharacterUnits => characterUnits;

        protected override void OnInitialize()
        {
            for (int i = 0; i < characterUnits.Count; ++i)
            {
                characterUnits[i].Initialize(CharacterDataManager.Inst.GetCharacterData(i));
            }
        }
    }
}
