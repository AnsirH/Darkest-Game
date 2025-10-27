using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon
{
    public class PositionMaintainer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform targetPoint;

        [Header("Variables")]
        public float moveTime = 1.0f;
        public float maxSpeed = 10.0f;
        public float allowedDistance = 0.05f;

        // Variables
        Vector3 currentSpeedVector = Vector3.zero;
        float limitXPosition = 0.0f;

        public float MoveSpeedX => currentSpeedVector.x;

        void LateUpdate()
        {
            if (Mathf.Approximately(Mathf.Abs(Vector3.Distance(transform.position, targetPoint.position)), allowedDistance))
            {
                return;
            }

            Vector3 resultPosition = Vector3.SmoothDamp(transform.position, targetPoint.position, ref currentSpeedVector, moveTime, maxSpeed);
            if (limitXPosition > 0)
                resultPosition.x = (resultPosition.x > limitXPosition) ? limitXPosition : resultPosition.x;

            transform.position = resultPosition;
        }

        public void SetLimitXPosition(float value)
        {
            if (value <= 0) return;
            limitXPosition = value;
        }

        public void SetTarget(Transform newTarget) { targetPoint = newTarget; }
        public void SetMoveTime(float moveTime) { this.moveTime = moveTime; }
    }
}
