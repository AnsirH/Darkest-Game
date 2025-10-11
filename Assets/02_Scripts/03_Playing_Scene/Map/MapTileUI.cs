using DarkestGame.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapTileUI : MonoBehaviour
{
    [Header("references")]
    [SerializeField] RectTransform rect;
    [SerializeField] Image tileImage;
    [SerializeField] Image frameImage;
    [SerializeField] Sprite fightImage;
    [SerializeField] Sprite rewardImage;

    [Header("variables")]
    [SerializeField] float highlightScaleRatio;
    [SerializeField] Color hightlightColor;

    // properties
    public Vector2 AnchoredPosition => rect.anchoredPosition;

    public void UpdateImage(TileType type)
    {
        tileImage.color = Color.white;
        switch (type)
        {
            case TileType.None:
                tileImage.color = Color.black;
                tileImage.sprite = null;
                break;
            case TileType.Item:
                tileImage.sprite = rewardImage;
                break;
            case TileType.Monster:
                tileImage.sprite = fightImage;
                break;
        }
    }

    public void UpdateImage(RoomType type)
    {
        tileImage.color = Color.white;
        switch (type)
        {
            case RoomType.None:
                tileImage.color = Color.black;
                tileImage.sprite = null;
                break;
            case RoomType.MonsterAndItem:
            case RoomType.Item:
                tileImage.sprite = rewardImage;
                break;
            case RoomType.Monster:
                tileImage.sprite = fightImage;
                break;
        }
    }

    public void SetTileSize(Vector2 size)
    {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public void SetPosition(Vector2 position)
    {
        rect.anchoredPosition = position;
    }

    public void ActiveTileHighlight_Scale(bool active)
    {
        StartCoroutine(ChangeRectSize_Coroutine(active ? highlightScaleRatio : 1.0f));
    }

    IEnumerator ChangeRectSize_Coroutine(float sizeRatio)
    {
        Vector3 currentSize = rect.localScale;
        Vector3 targetSize = Vector3.one * sizeRatio;
        float duration = 1.0f;
        float timer = 0.0f;
        while (timer < duration)
        {
            rect.localScale = Vector3.Lerp(currentSize, targetSize, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        rect.localScale = targetSize;
    }

    public void ActiveTileHighlight_Color(bool active)
    {
        StartCoroutine(ChangeColor_Coroutine(active ? hightlightColor : Color.black));
    }

    IEnumerator ChangeColor_Coroutine(Color targetColor)
    {
        Color currentColor = frameImage.color;
        float duration = 1.0f;
        float timer = 0.0f;
        while (timer < duration)
        {
            frameImage.color = Color.Lerp(currentColor, targetColor, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        frameImage.color = targetColor;
    }
}
