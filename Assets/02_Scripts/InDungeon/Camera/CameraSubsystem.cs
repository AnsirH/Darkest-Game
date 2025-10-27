using DarkestLike.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon.CameraControl
{
    public class CameraSubsystem : InDungeonSubsystem
    {
        [Header("Referenses")]
        [SerializeField] PositionMaintainer camPositionMaintainer;
        [SerializeField] Transform roomCamTarget;

        protected override void OnInitialize()
        {
            InDungeonManager.Inst.OnRoomEntered += OnRoomEnteredHandler;
        }

        public void SetCameraTarget(Transform target) { camPositionMaintainer.SetTarget(target); }

        public void SetCameraMovementLimit(float limit) { camPositionMaintainer.SetLimitXPosition(limit); }

        #region Event Methods
        void OnRoomEnteredHandler(RoomData roomData)
        {
            SetCameraTarget(roomCamTarget);
        }

        void OnHallwayEnteredHandler(HallwayData hallwayData)
        {
        }
        #endregion
    }
}