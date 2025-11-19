using DarkestLike.Map;
using DarkestLike.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dungeon > Map > Room, Hallway

[System.Serializable]
public class Dungeon
{
    public DungeonData data;

    public int level;

    public int exp;

    /// <summary> 현재 맵 /// </summary>
    public List<MapData> selectedMap;

    /// <summary> 클리어 한 맵 /// </summary>
    private List<MapData> clearedMaps;

    public Dungeon(DungeonData data, int level = 0, int exp = 0)
    {
        this.data = data;
        this.level = level;
        this.exp = exp;
        // 던전은 첫 번째 맵을 기본적으로 선택한다.
        selectedMap = new() { MapGenerator.GenerateMap(data.MapSODatas[0], level) };
    }

    public void Complete(MapData completedMap)
    {
        exp += completedMap.MapSOData.EXP;

        selectedMap.Remove(completedMap);
        clearedMaps.Add(completedMap);

        for (int i = 0; i < completedMap.MapSOData.unlockMaps.Length; i++)
        {
            MapData unlockMap = MapGenerator.GenerateMap(completedMap.MapSOData.unlockMaps[i], level);
            selectedMap.Add(unlockMap);
        }

        if (exp < data.RequireEXP[0]) { level = 0; }
        else if (exp < data.RequireEXP[1]) { level = 1; }
        else if (exp < data.RequireEXP[2])
        {
            level = 2;
            if (selectedMap.Find(x => x.MapSOData == data.BossMap) == null && clearedMaps.Find(x => x.MapSOData == data.BossMap) == null) selectedMap.Insert(0, MapGenerator.GenerateMap(data.BossMap, level));
        }
        else { level = 3; }
    }
}

public class DungeonDataManager : Singleton<DungeonDataManager>
{
    // 게임에 있는 모든 던전을 관리한다.
    // 던전은 맵들을 가지고 있고, 맵은 방과 다리, 타일의 정보를 가지고 있기 때문에 MapThemeManager에 접근하는 것은
    // 모든 장소에 접근할 수 있음을 의미한다.    

    public List<DungeonData> dungeonDatas = new();
    public List<Dungeon> dungeons = new();

    // Properties
    public MapData CurrentMap { get; private set; } = null;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < dungeonDatas.Count; i++)
        {
            dungeons.Add(new Dungeon(dungeonDatas[i]));
        }
    }

    public void UpdateDungeonInfo()
    {
        if (dungeons.Count == 0)
        {
            for (int i = 0; i < dungeonDatas.Count; i++)
            {
                dungeons.Add(new Dungeon(dungeonDatas[i]));
            }
        }
    }

    public void SetCurrentMap(MapData mapData) { CurrentMap = mapData; }
}