using DarkestGame.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    public RoomData roomData;
    public Button button;
    public Image image;

    public Color defaultColor;
    public Color highlightColor;

    public void UpdateButton()
    {
        switch (roomData.type)
        {
            case RoomType.None:
                image.color = Color.black;
                break;
            case RoomType.Item:
                image.color = Color.yellow;
                break;
            case RoomType.Monster:
                image.color = Color.red;
                break;
            case RoomType.MonsterAndItem:
                image.color = Color.magenta;
                break;
        }
    }

    public void SetButtonHighlight(bool active)
    {
        button.image.color = active ? highlightColor : defaultColor;
    }
}
