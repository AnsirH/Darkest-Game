using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneUI : MonoBehaviour
{
    public Button startGameButton;

    private void Awake()
    {
        startGameButton.onClick.AddListener(() => { LoadingManager.Inst.LoadSceneWithDungeonSetup(LoadingManager.SCENENAME_STARTANDREADY).Forget(); });
    }
}
