using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkestLike.Map
{
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
        [SerializeField] Color loopHightlightColor;

        // variables
        float highlightDuration = 0.5f;

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

        // 크기 설정
        public void SetTileSize(Vector2 size)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public void SetPosition(Vector2 position)
        {
            rect.anchoredPosition = position;
        }

        // 하이라이트
        public void ActiveTileHighlight_Scale(bool active)
        {
            StartCoroutine(ChangeRectSize_Coroutine(active ? highlightScaleRatio : 1.0f));
        }

        IEnumerator ChangeRectSize_Coroutine(float sizeRatio)
        {
            Vector3 currentSize = rect.localScale;
            Vector3 targetSize = Vector3.one * sizeRatio;
            float timer = 0.0f;
            while (timer < highlightDuration)
            {
                rect.localScale = Vector3.Lerp(currentSize, targetSize, timer / highlightDuration);
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
            float timer = 0.0f;
            while (timer < highlightDuration)
            {
                frameImage.color = Color.Lerp(currentColor, targetColor, timer / highlightDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            frameImage.color = targetColor;
        }

        public void ActiveLoopHighlight_Color()
        {
            StartCoroutine(ChangeColorLoop_Coroutine(loopHightlightColor));
        }

        IEnumerator ChangeColorLoop_Coroutine(Color targetColor)
        {
            Color currentColor = frameImage.color;
            while (true)
            {
                yield return ChangeColor_Coroutine(targetColor);
                yield return ChangeColor_Coroutine(currentColor);
            }
        }

        public void ResetHighlight()
        {
            StopAllCoroutines();
            frameImage.color = Color.black;
        }
    }
}
