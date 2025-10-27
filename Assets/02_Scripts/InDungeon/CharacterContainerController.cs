using DarkestLike.Map;
using DarkestLike.InDungeon.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DarkestLike.InDungeon
{
    public class CharacterContainerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] LimitedMovement movement;
        [SerializeField] PlayerInput input;
        [SerializeField] Transform camTrf;

        // Properties
        public Transform CamTrf => camTrf;

        Vector3 horizontalVector = Vector3.right;
        bool isSetMoveGround = false;
        bool isInEndPoint = false;
        bool isFreeze = false;
        float tileDistance;
        int currentTileIndex = 0;
        float endDistance;

        private void Update()
        {
            if (isInEndPoint && input.Up)
            {
                InDungeonManager.Inst.EnterRoom();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
                movement.moveSpeed = 10.0f;
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                movement.moveSpeed = 1.5f;

        }

        void LateUpdate()
        {
            if (isFreeze) return;
            if (input.MoveHorizontalInput > 0)
            {
                movement.MoveForDeltaTime(input.MoveHorizontalInputRaw * horizontalVector);
            }
            else if (input.MoveHorizontalInput < 0)
            {
                movement.MoveForDeltaTime(input.MoveHorizontalInputRaw * 0.5f * horizontalVector);
            }
            if (isSetMoveGround)
                CheckLocation();
        }

        public void SetMoveGround(float moveableDistance, float tileDistance)
        {
            Vector3 startPoint = Vector3.zero, endPoint = Vector3.zero;
            endPoint.x = moveableDistance;
            movement.UpdateLimit(startPoint, endPoint);
            this.tileDistance = tileDistance;
            endDistance = moveableDistance - (tileDistance * 0.5f);

            isSetMoveGround = true;
        }

        void CheckLocation()
        {
            float currentXLocation = transform.position.x - tileDistance * 0.5f;
            if (currentXLocation < 0) currentXLocation = 0;
            if (currentTileIndex != (int)(currentXLocation / tileDistance))
            {
                currentTileIndex = (int)(currentXLocation / tileDistance);
                InDungeonManager.Inst.EnterTheTile(currentTileIndex);
            }

            if (currentXLocation >= endDistance)
                isInEndPoint = true;
            else
                isInEndPoint = false;
        }

        public void ActiveFreeze(bool active)
        {
            isFreeze = active;
        }

        public void ResetPosition()
        {
            transform.position = Vector3.zero;
        }
    }
}
