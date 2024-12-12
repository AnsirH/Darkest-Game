using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LimitedMovement : Movement
{
    [Header("Limit Point")]
    public Vector3 startPoint;
    public Vector3 endPoint;

    public override void MoveForDeltaTime(Vector3 direction)
    {
        base.MoveForDeltaTime(direction);

        Vector3 currentPosition = transform.position;
        if (currentPosition.x > endPoint.x) { currentPosition.x = endPoint.x; }
        else if (currentPosition.x < startPoint.x) { currentPosition.x = startPoint.x; }

        transform.position = currentPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(startPoint, endPoint);
    }
}
