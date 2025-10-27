using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkestLike.Character;

namespace DarkestLike.InDungeon.BattleSystem
{
    /// <summary>
    /// 적 그룹을 관리하는 클래스
    /// 한 방에 등장하는 적들의 CharacterData를 관리합니다.
    /// </summary>
    public class EnemyGroup
    {
        [Header("Enemy Data")]
        public List<CharacterData> Enemies = new();
        
        /// <summary>
        /// 적 리스트로 EnemyGroup을 생성합니다.
        /// </summary>
        /// <param name="enemies">적들의 CharacterData 리스트</param>
        public EnemyGroup(List<CharacterData> enemies)
        {
            Enemies = enemies ?? new List<CharacterData>();
        }
        
        /// <summary>
        /// 적을 그룹에 추가합니다.
        /// </summary>
        /// <param name="enemy">추가할 적의 CharacterData</param>
        public void AddEnemy(CharacterData enemy)
        {
            if (enemy != null)
            {
                Enemies.Add(enemy);
            }
        }
        
        /// <summary>
        /// 특정 인덱스의 적을 가져옵니다.
        /// </summary>
        /// <param name="index">적의 인덱스</param>
        /// <returns>CharacterData (없으면 null)</returns>
        public CharacterData GetEnemy(int index)
        {
            if (index >= 0 && index < Enemies.Count)
            {
                return Enemies[index];
            }
            return null;
        }
        
        /// <summary>
        /// 모든 적을 제거합니다.
        /// </summary>
        public void Clear()
        {
            Enemies.Clear();
        }
    }
}