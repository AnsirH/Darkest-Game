using DarkestLike.ScriptableObj;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.Map
{
    [CreateAssetMenu(fileName = "MapData", menuName = "Create Data Asset/Create Map Data")]
    public class MapSOData : ScriptableObject
    {
        [Header("테마")]
        public MapThemeData ThemeData;

        [Header("버튼 색상")]
        public Color buttonColor;

        [Header("방 정보")]
        public int RoomCount = 0;
        public int RoomTypeNonePercent = 25;
        public int RoomTypeItemPercent = 25;
        public int RoomTypeMonsterPercent = 25;
        public int RoomTypeMonsterAndItemPercent = 25;

        [Header("타일 정보")]
        public int TileCount = 0;
        public int TileTypeNonePercent = 50;
        public int TileTypeItemPercent = 25;
        public int TileTypeMonsterPercent = 25;

        [Header("몬스터 정보")]
        public CharacterBase[] enemeies;

        [Header("클리어 시 해방 맵 데이터")]
        public MapSOData[] unlockMaps;

        [Header("클리어 시 경험치")]
        public int EXP;

        public RoomType GetRandomRoomType()
        {
            float randomNum = Random.Range(0, 101f);
            if (randomNum <= RoomTypeNonePercent) { return RoomType.None; }
            else if (randomNum <= RoomTypeNonePercent + RoomTypeItemPercent) { return RoomType.Item; }
            else if (randomNum <= RoomTypeNonePercent + RoomTypeItemPercent + RoomTypeMonsterPercent) { return RoomType.Monster; }
            else { return RoomType.MonsterAndItem; }
        }

        public TileType GetRandomTileType()
        {
            float randomNum = Random.Range(0, 101f);
            if (randomNum <= TileTypeNonePercent) { return TileType.None; }
            else if (randomNum <= TileTypeNonePercent + TileTypeItemPercent) { return TileType.Item; }
            else { return TileType.Monster; }
        }
    }
}