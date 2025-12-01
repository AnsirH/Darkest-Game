using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkestLike.Character;
using DarkestLike.InDungeon.BattleSystem;
using DarkestLike.ScriptableObj;

namespace DarkestLike.Map
{
    /// <summary>
    /// 적 그룹 생성을 담당하는 유틸리티 클래스
    /// MapGenerator와 동일한 형식으로 구현
    /// </summary>
    public static class EnemyGroupGenerator
    {
        /// <summary>
        /// 방 타입에 따른 적 그룹을 생성합니다.
        /// </summary>
        /// <param name="roomType">방 타입</param>
        /// <param name="mapData">맵 데이터 (적 정보 포함)</param>
        /// <param name="dungeonLevel">던전 레벨</param>
        /// <returns>생성된 EnemyGroup</returns>
        public static EnemyGroup GenerateEnemyGroupForRoom(MapSOData mapData, int dungeonLevel)
        {
            List<CharacterData> enemies = new();
            
            // 몬스터 방: 무조건 4마리 생성 (원거리 1~2명 포함)
            enemies = GenerateBalancedEnemyGroup(4, mapData, dungeonLevel);
            
            return new EnemyGroup(enemies);
        }
        
        /// <summary>
        /// 타일 타입에 따른 적 그룹을 생성합니다. (복도 몬스터)
        /// </summary>
        /// <param name="tileType">타일 타입</param>
        /// <param name="mapData">맵 데이터 (적 정보 포함)</param>
        /// <param name="dungeonLevel">던전 레벨</param>
        /// <returns>생성된 EnemyGroup (적이 없으면 빈 그룹)</returns>
        public static EnemyGroup GenerateEnemyGroupForTile(MapSOData mapData, int dungeonLevel)
        {
            List<CharacterData> enemies = new();
            
            // 복도 몬스터: 2~4마리 (확률 4:4:2, 원거리 1~2명 포함)
            int enemyCount = GetRandomHallwayEnemyCount();
            enemies = GenerateBalancedEnemyGroup(enemyCount, mapData, dungeonLevel);
            
            return new EnemyGroup(enemies);
        }
        
        /// <summary>
        /// 복도 몬스터 수를 확률에 따라 결정합니다.
        /// 4마리: 40%, 3마리: 40%, 2마리: 20%
        /// </summary>
        /// <returns>몬스터 수 (2~4)</returns>
        private static int GetRandomHallwayEnemyCount()
        {
            int randomValue = Random.Range(1, 11); // 1~10
            
            if (randomValue <= 4)
            {
                return 4; // 40% 확률
            }
            else if (randomValue <= 8)
            {
                return 3; // 40% 확률
            }
            else
            {
                return 2; // 20% 확률
            }
        }
        
        /// <summary>
        /// 균형잡힌 적 그룹을 생성합니다.
        /// 근거리: 최소 2명 이상
        /// 원거리: 최소 1명 이상 (단, 2명 그룹일 때는 근거리만)
        /// </summary>
        /// <param name="totalCount">총 적 수</param>
        /// <param name="mapData">맵 데이터 (적 정보 포함)</param>
        /// <param name="dungeonLevel">던전 레벨</param>
        /// <returns>생성된 적 리스트</returns>
        private static List<CharacterData> GenerateBalancedEnemyGroup(int totalCount, MapSOData mapData, int dungeonLevel)
        {
            List<CharacterData> enemies = new();
            
            int meleeCount, rangedCount;
            
            if (totalCount == 2)
            {
                // 2명: 근거리 2명
                meleeCount = 2;
                rangedCount = 0;
            }
            else if (totalCount == 3)
            {
                // 3명: 근거리 2명, 원거리 1명
                meleeCount = 2;
                rangedCount = 1;
            }
            else if (totalCount == 4)
            {
                // 4명: 근거리 2~3명, 원거리 1~2명
                meleeCount = Random.Range(2, 4); // 2 또는 3
                rangedCount = totalCount - meleeCount; // 1 또는 2
            }
            else
            {
                // 기본: 근거리 최소 2명, 나머지는 랜덤
                meleeCount = Mathf.Max(2, totalCount - 2); // 최소 2명
                rangedCount = totalCount - meleeCount;
            }
            
            // 근거리 적 생성
            for (int i = 0; i < meleeCount; i++)
            {
                CharacterData enemy = GenerateRandomEnemy(mapData, dungeonLevel, CharacterType.Melee);
                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }
            
            // 원거리 적 생성
            for (int i = 0; i < rangedCount; i++)
            {
                CharacterData enemy = GenerateRandomEnemy(mapData, dungeonLevel, CharacterType.Ranged);
                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }
            
            return enemies;
        }
        
        /// <summary>
        /// 랜덤한 적을 생성합니다.
        /// MapSOData의 enemeies 배열에서 적절한 타입의 적을 선택하여 생성
        /// </summary>
        /// <param name="mapData">맵 데이터 (적 정보 포함)</param>
        /// <param name="dungeonLevel">던전 레벨</param>
        /// <param name="characterType">캐릭터 타입 (Melee/Ranged)</param>
        /// <returns>생성된 적 CharacterData</returns>
        private static CharacterData GenerateRandomEnemy(MapSOData mapData, int dungeonLevel, CharacterType characterType = CharacterType.Melee)
        {
            if (mapData.enemeies == null || mapData.enemeies.Length == 0)
            {
                // 적 데이터가 없으면 null 반환
                return null;
            }
            
            // 해당 타입의 적들을 필터링
            List<CharacterBase> matchingEnemies = new();
            foreach (var enemy in mapData.enemeies)
            {
                if (enemy != null && enemy.type == characterType)
                {
                    matchingEnemies.Add(enemy);
                }
            }
            
            if (matchingEnemies.Count == 0)
            {
                // 해당 타입의 적이 없으면 null 반환
                return null;
            }
            
            // 랜덤하게 적 선택
            CharacterBase selectedEnemy = matchingEnemies[Random.Range(0, matchingEnemies.Count)];
            
            // CharacterBase를 기반으로 CharacterData 생성
            CharacterData newEnemy = new(selectedEnemy, $"Lv.{dungeonLevel} {selectedEnemy.name}", dungeonLevel);
            
            return newEnemy;
        }
    }
}
