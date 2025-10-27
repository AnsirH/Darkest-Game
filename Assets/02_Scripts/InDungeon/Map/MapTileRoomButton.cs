using DarkestLike.InDungeon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.Map
{
    public class MapTileRoomButton : MapTileUI
    {
        // variables
        RoomData roomData;

        public void MoveToThisRoom()
        {
            InDungeonManager.Inst.EnterHallway(roomData);
        }

        public void SetRoomData(RoomData roomData)
        {
            this.roomData = roomData;
        }
    }
}