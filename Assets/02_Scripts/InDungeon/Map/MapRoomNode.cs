using DarkestLike.InDungeon;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace DarkestLike.Map
{
    public class MapRoomNode : MapNode
    {
        [Header("button")]
        public Button button;
        
        // variables
        RoomData roomData;

        public void MoveToThisRoom()
        {
            var manager = InDungeonManager.Inst;

            // 검증 1: 배틀 중 체크
            if (manager.BattleSubsystem.IsBattleActive)
            {
                Debug.LogWarning("[MapRoomNode] Cannot move during battle");
                return;
            }

            // 검증 2: 이미 전환 중 체크
            if (manager.IsTransitioning)
            {
                Debug.LogWarning("[MapRoomNode] Already transitioning to another room");
                return;
            }

            // 검증 3: 현재 위치가 방인지 체크 (방어적 검증)
            if (manager.CurrentLocation != CurrentLocation.Room)
            {
                Debug.LogWarning("[MapRoomNode] Can only move from a room");
                return;
            }

            // 검증 4: 인접한 방인지 체크
            if (!manager.CurrentRoom.CheckIsMoveableRoom(roomData))
            {
                Debug.LogWarning($"[MapRoomNode] {roomData.Position} is not adjacent to current room");
                return;
            }

            InDungeonManager.Inst.StartEnteringHallway(roomData);
        }

        public void SetRoomData(RoomData newRoomData)
        {
            this.roomData = newRoomData;
        }
    }
}