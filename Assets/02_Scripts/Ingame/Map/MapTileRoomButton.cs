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
            if (MapManager.Inst.CurrentLocation != CurrentLocation.Room) return;
            if (MapManager.Inst.CurrentRoom != roomData && MapManager.Inst.CurrentRoom.CheckIsMoveableRoom(roomData))
                MapManager.Inst.ExitRoom(roomData);
        }

        public void SetRoomData(RoomData roomData)
        {
            this.roomData = roomData;
        }
    }
}