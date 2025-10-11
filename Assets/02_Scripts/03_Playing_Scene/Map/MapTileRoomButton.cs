using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestGame.Map
{
    public class MapTileRoomButton : MapTileUI
    {
        // variables
        RoomData roomData;

        public void MoveToThisRoom()
        {
            MapManager.Inst.ExitRoom(roomData);
        }

        public void SetRoomData(RoomData roomData)
        {
            this.roomData = roomData;
        }
    }
}