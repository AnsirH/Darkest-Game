using System;
using DarkestLike.Character;
using DarkestLike.InDungeon.Unit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DarkestLike.InDungeon.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace DarkestLike.InDungeon
{
    public class UnitSubsystem : InDungeonSubsystem
    {
        public CharacterUnit characterUnitPrefab;
        
        // Variables
        private Dictionary<CharacterData, CharacterUnit> playerCharacters = new();
        private Dictionary<CharacterData, CharacterUnit> enemyCharacters = new();
        // Properties
        public List<CharacterUnit> PlayerUnits => playerCharacters.Values.ToList();
        public List<CharacterUnit> EnemyUnits => enemyCharacters.Values.ToList();

        protected override void OnInitialize()
        {
        }

        public bool AddPlayerCharacter(CharacterData newCharacterData, Transform positionTarget, out CharacterUnit newCharacterUnit)
        {
            newCharacterUnit = null;
            if (playerCharacters.ContainsKey(newCharacterData)) return false;
            newCharacterUnit = CreateCharacterUnit(newCharacterData, positionTarget, false);
            playerCharacters[newCharacterData] = newCharacterUnit;
            return true;
        }

        public bool AddEnemyCharacter(CharacterData newCharacterData, Transform positionTarget, out CharacterUnit newCharacterUnit)
        {
            newCharacterUnit = null;
            if (enemyCharacters.ContainsKey(newCharacterData)) return false;
            newCharacterUnit = CreateCharacterUnit(newCharacterData, positionTarget, true);
            enemyCharacters[newCharacterData] = newCharacterUnit;
            return true;
        }

        private CharacterUnit CreateCharacterUnit(CharacterData newCharacterData, Transform positionTarget, bool isEnemy)
        {
            CharacterUnit newUnit = Instantiate(characterUnitPrefab, transform);
            newUnit.Initialize(newCharacterData, positionTarget, isEnemy);
            return newUnit;
        }
    }
}
