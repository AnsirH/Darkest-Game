using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartAndReadySceneUI : MonoBehaviour
{
    public DungeonPanel[] dungeonPanels = new DungeonPanel[5];

    public Button startGameButton;

    private void Awake()
    {
        for (int i = 0; i < dungeonPanels.Length; i++)
        {
            dungeonPanels[i].dungeon = null;
        }

        for (int i = 0; i < DungeonManager.Inst.dungeons.Count; i++)
        {
            dungeonPanels[i].dungeon = DungeonManager.Inst.dungeons[i];
            dungeonPanels[i].UpdateInfo();
        }

        startGameButton.onClick.AddListener(() => { LoadingManager.Inst.LoadSceneWithDungeonSetup(LoadingManager.SCENENAME_PLAYING).Forget(); });
    }

    private void Update()
    {
        if (DungeonManager.Inst.currentMap != null)
        {
            if (!startGameButton.gameObject.activeSelf) startGameButton.gameObject.SetActive(true);
        }
        else
        {
            if (startGameButton.gameObject.activeSelf) startGameButton.gameObject.SetActive(false);
        }
    }
}
