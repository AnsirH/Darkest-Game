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

        public Camera MainCamera { get; private set; }
        protected override void OnInitialize()
        {
            MainCamera = Camera.main;
        }

        public void SetCameraTarget(Transform target) { camPositionMaintainer.SetTarget(target); }

        public void SetCameraMovementLimit(float limit) { camPositionMaintainer.SetLimitXPosition(limit); }

        public void SetToRoomTarget(bool isImmediate = true)
        {
            SetCameraTarget(roomCamTarget);
            camPositionMaintainer.isLimited = false;
            
            if (isImmediate) 
                camPositionMaintainer.SetPositionToTarget();
        }

        public void SetToPartyTarget(bool isImmediate = true)
        {
            SetCameraTarget(partyCamTarget);
            camPositionMaintainer.isLimited = true;
            
            if (isImmediate) 
                camPositionMaintainer.SetPositionToTarget();
        }
    }
}