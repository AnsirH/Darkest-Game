using System;
using DarkestLike.Map;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using UnityEngine;

namespace DarkestLike.InDungeon.CameraControl
{
    public class CameraSubsystem : InDungeonSubsystem
    {
        [Header("Referenses")]
        [SerializeField] PositionMaintainer camPositionMaintainer;
        [SerializeField] Transform roomCamTarget;
        [SerializeField] Transform partyCamTarget;

        protected override void OnInitialize()
        {
        }

        public void SetCameraTarget(Transform target) { camPositionMaintainer.SetTarget(target); }

        public void SetCameraMovementLimit(float limit) { camPositionMaintainer.SetLimitXPosition(limit); }

        public void SetToRoomTarget()
        {
            SetCameraTarget(roomCamTarget);
            camPositionMaintainer.isLimited = false;
            camPositionMaintainer.SetPositionToTarget();
        }

        public void SetToPartyTarget()
        {
            SetCameraTarget(partyCamTarget);
            camPositionMaintainer.isLimited = true;
            camPositionMaintainer.SetPositionToTarget();
        }
    }
}