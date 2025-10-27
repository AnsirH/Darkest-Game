using Cysharp.Threading.Tasks.Triggers;
using DarkestLike.SceneLoad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonInfoUI : MonoBehaviour
{
    public DungeonPanel[] dungeonPanels = new DungeonPanel[5];

    public Button startGameButton;

    private void Awake()
    {
        for (int i = 0; i < dungeonPanels.Length; i++)
            dungeonPanels[i].dungeon = null;

        startGameButton.onClick.AddListener(() => { SceneLoadManager.Inst.LoadDungeon(DungeonDataManager.Inst.CurrentMap).Forget(); });
    }

    private void Start()
    {
        DungeonDataManager.Inst.UpdateDungeonInfo();

        for (int i = 0; i < DungeonDataManager.Inst.dungeons.Count; i++)
        {
            dungeonPanels[i].dungeon = DungeonDataManager.Inst.dungeons[i];
            dungeonPanels[i].UpdateInfo();
        }

    }

    private void Update()
    {
        if (DungeonDataManager.Inst.CurrentMap != null)
        {
            if (!startGameButton.gameObject.activeSelf) startGameButton.gameObject.SetActive(true);
        }
        else
        {
            if (startGameButton.gameObject.activeSelf) startGameButton.gameObject.SetActive(false);
        }
    }
}
