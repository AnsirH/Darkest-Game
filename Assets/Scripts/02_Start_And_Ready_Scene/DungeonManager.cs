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
    // ���ӿ� �ִ� ��� ������ �����Ѵ�.
    // ������ �ʵ��� ������ �ְ�, ���� ��� �ٸ�, Ÿ���� ������ ������ �ֱ� ������ MapThemeManager�� �����ϴ� ����
    // ��� ��ҿ� ������ �� ������ �ǹ��Ѵ�.    

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