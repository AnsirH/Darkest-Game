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
        public List<CharacterUnit> PlayerUnits => playerCharacters.Values
            .OrderBy(unit => unit.PositionIndex)
            .ToList();
        public List<CharacterUnit> EnemyUnits => enemyCharacters.Values
            .OrderBy(unit => unit.PositionIndex)
            .ToList();

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

        /// <summary>
        /// 모든 적 캐릭터를 제거합니다 (배틀 종료 시 호출)
        /// </summary>
        public void ClearEnemyCharacters()
        {
            enemyCharacters.Clear();
        }

        /// <summary>
        /// 플레이어 캐릭터를 제거합니다 (사망 시 호출)
        /// </summary>
        public void RemovePlayerUnit(CharacterUnit unit)
        {
            if (unit == null) return;

            // Dictionary에서 해당 유닛 찾아서 제거
            var entry = playerCharacters.FirstOrDefault(kvp => kvp.Value == unit);
            if (entry.Key != null)
            {
                playerCharacters.Remove(entry.Key);
                Debug.Log($"[UnitSubsystem] {unit.CharacterName} 제거됨. 남은 플레이어: {playerCharacters.Count}명");
            }
        }
    }
}
