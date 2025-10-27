using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragableMap : MonoBehaviour, IDragHandler
{
    [Header("References")]
    [SerializeField] RectTransform rectTransform;
    [SerializeField] RectTransform parentRectTransform;

    [Header("Settings")]
    [SerializeField] float returnSpeed = 5f; // 범위 밖으로 나갔을 때 돌아오는 속도 배율
    [SerializeField] float boundaryOffset = 20f;

    private bool isOutOfBounds = false;
    private Vector2 centerPosition;
    private Vector3 targetPosition;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (parentRectTransform == null)
            parentRectTransform = transform.parent.GetComponent<RectTransform>();
        centerPosition = rectTransform.position;
    }

    // 마우스 드래그로 이동 가능
    // 이동 범위는 부모의 좌표와 넓이, 높이 내에서만 가능
    // 이동 범위를 벗어나게 되면 이동 범위 내로 부드럽게 자동 이동
    // 이동 범위를 벗어난 정도가 클수록 자동 이동 속도가 빨라짐

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 델타만큼 이동
        rectTransform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0);
        
        // 범위 체크
        CheckBounds();
    }

    private void Update()
    {
        // 범위를 벗어났을 때 자동으로 범위 안으로 이동
        if (isOutOfBounds)
        {
            // 벗어난 거리에 비례하여 속도 증가
            float distance = Vector3.Distance(rectTransform.position, targetPosition);
            float speed = returnSpeed * (1 + distance * 0.01f);
            
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, speed * Time.deltaTime);
            
            // 목표 위치에 충분히 가까워지면 정확히 맞춤
            if (distance < 0.1f)
            {
                rectTransform.position = targetPosition;
                isOutOfBounds = false;
            }
        }
    }

    private void CheckBounds()
    {
        Vector3 currentPos = rectTransform.position;
        Vector3 clampedPos = currentPos;
        
        // 경계 계산
        float minX = centerPosition.x - (rectTransform.rect.width * 0.5f + boundaryOffset);
        float maxX = centerPosition.x + rectTransform.rect.width * 0.5f + boundaryOffset;
        float minY = centerPosition.y - (rectTransform.rect.height * 0.5f + boundaryOffset);
        float maxY = centerPosition.y + rectTransform.rect.height * 0.5f + boundaryOffset;
        
        // 현재 RectTransform의 크기 고려
        float halfWidth = parentRectTransform.rect.width * 0.5f;
        float halfHeight = parentRectTransform.rect.height * 0.5f;
        
        // 범위 내로 제한
        clampedPos.x = Mathf.Clamp(currentPos.x, minX + halfWidth, maxX - halfWidth);
        clampedPos.y = Mathf.Clamp(currentPos.y, minY + halfHeight, maxY - halfHeight);
        
        // 범위를 벗어났는지 확인
        if (currentPos != clampedPos)
        {
            isOutOfBounds = true;
            targetPosition = clampedPos;
        }
        else
        {
            isOutOfBounds = false;
        }
    }
}
