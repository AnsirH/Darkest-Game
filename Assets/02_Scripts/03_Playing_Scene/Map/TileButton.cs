using DarkestGame.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileButton : MonoBehaviour
{
    public TileData tileData;
    public Button button;
    public Image image;

    public Color defaultColor;
    public Color highlightColor;

    public void UpdateButton()
    {
        switch (tileData.type)
        {
            case TileType.None:
                image.color = Color.black;
                break;
            case TileType.Item:
                image.color = Color.yellow;
                break;
            case TileType.Monster:
                image.color = Color.red;
                break;
        }
    }

    public void SetButtonHighlight(bool active)
    {
        button.image.color = active ? highlightColor : defaultColor;
    }
}
