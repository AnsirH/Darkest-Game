using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionMaintainer : MonoBehaviour
{
    [Header("���� Ÿ��")]
    public Transform targetPoint;

    [Header("��ġ")]
    public float moveTime = 1.0f;
    public float maxSpeed = 10.0f;
    public float allowedDistance = 0.05f;

    Vector3 currentSpeedVector = Vector3.zero;

    public float MoveSpeedX => currentSpeedVector.x;

    void LateUpdate()
    {
        if (Mathf.Approximately(Mathf.Abs(Vector3.Distance(transform.position, targetPoint.position)), allowedDistance))
        {
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPoint.position, ref currentSpeedVector, moveTime, maxSpeed);
    }    
}
