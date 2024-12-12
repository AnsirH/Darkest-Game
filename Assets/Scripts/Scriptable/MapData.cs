using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestGame.Map
{
    [CreateAssetMenu(fileName = "MapData", menuName = "Create Data Asset/Create Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("�׸�")]
        public MapThemeData ThemeData;

        [Header("��ư ����")]
        public Color buttonColor;

        [Header("�� ����")]
        public int RoomCount = 0;
        public int RoomTypeNonePercent = 25;
        public int RoomTypeItemPercent = 25;
        public int RoomTypeMonsterPercent = 25;
        public int RoomTypeMonsterAndItemPercent = 25;


        [Header("Ÿ�� ����")]
        public int TileCount = 0;
        public int TileTypeNonePercent = 50;
        public int TileTypeItemPercent = 25;
        public int TileTypeMonsterPercent = 25;


        [Header("Ŭ���� �� �ع� �� ������")]
        public MapData[] unlockMaps;

        [Header("Ŭ���� �� ����ġ")]
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