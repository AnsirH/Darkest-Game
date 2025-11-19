using DarkestLike.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DungeonData", menuName = "Create Data Asset/Create Dungeon Data")]
public class DungeonData : ScriptableObject
{
    public string DungeonName;

    public MapThemeData ThemeData;

    public List<MapSOData> MapSODatas;

    public MapSOData BossMap;

    public readonly int[] RequireEXP = { 100, 300, 600 };
}