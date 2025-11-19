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

        void OnEnable()
        {
            DungeonEventBus.Subscribe(DungeonEventType.EnterRoom, EnterRoomHandler);
            DungeonEventBus.Subscribe(DungeonEventType.EnterHallway, EnterHallwayHandler);
        }

        private void OnDisable()
        {
            DungeonEventBus.Unsubscribe(DungeonEventType.EnterRoom, EnterRoomHandler);
            DungeonEventBus.Unsubscribe(DungeonEventType.EnterHallway, EnterHallwayHandler);
        }

        public void SetCameraTarget(Transform target) { camPositionMaintainer.SetTarget(target); }

        public void SetCameraMovementLimit(float limit) { camPositionMaintainer.SetLimitXPosition(limit); }

        #region Event Methods
        void EnterRoomHandler()
        {
            SetCameraTarget(roomCamTarget);
            camPositionMaintainer.SetPosition(roomCamTarget.position);
        }

        void EnterHallwayHandler()
        {
            SetCameraTarget(partyCamTarget);
            camPositionMaintainer.SetPosition(partyCamTarget.position);
        }
        #endregion
    }
}