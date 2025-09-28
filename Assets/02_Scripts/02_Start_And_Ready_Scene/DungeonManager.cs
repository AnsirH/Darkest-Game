using DarkestGame.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dungeon
{
    public DungeonData data;

    public int level;

    public int exp;

    public List<Map> currentMaps;

    private List<Map> clearedMaps;

    public Dungeon(DungeonData data)
    {
        this.data = data;
        level = 0;
        exp = 0;
        currentMaps = new() { MapGenerator.GenerateMap(data.MapDatas[0]) };
    }

    public Dungeon(DungeonData data, int level, int exp, List<Map> currentMaps)
    {
        this.data = data;
        this.level = level;
        this.exp = exp;
        this.currentMaps = currentMaps;
    }

    public void Complete(Map completedMap)
    {
        exp += completedMap.MapData.EXP;

        currentMaps.Remove(completedMap);
        clearedMaps.Add(completedMap);

        for (int i = 0; i < completedMap.MapData.unlockMaps.Length; i++)
        {
            Map unlockMap = MapGenerator.GenerateMap(completedMap.MapData.unlockMaps[i]);
            currentMaps.Add(unlockMap);
        }

        if (exp < data.RequireEXP[0]) { level = 0; }
        else if (exp < data.RequireEXP[1]) { level = 1; }
        else if (exp < data.RequireEXP[2])
        {
            level = 2;
            if (currentMaps.Find(x => x.MapData == data.BossMap) == null && clearedMaps.Find(x => x.MapData == data.BossMap) == null) currentMaps.Insert(0, MapGenerator.GenerateMap(data.BossMap));
        }
        else { level = 3; }
    }
}

public class DungeonManager : Singleton<DungeonManager>
{
    // 게임에 있는 모든 던전을 관리한다.
    // 던전은 맵들을 가지고 있고, 맵은 방과 다리, 타일의 정보를 가지고 있기 때문에 MapThemeManager에 접근하는 것은
    // 모든 장소에 접근할 수 있음을 의미한다.    

    public List<DungeonData> dungeonDatas = new();
    public List<Dungeon> dungeons = new();

    [HideInInspector]
    public Map currentMap = null;

    public override void Awake()
    {
        base.Awake();
        
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
}