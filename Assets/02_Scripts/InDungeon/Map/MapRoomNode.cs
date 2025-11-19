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
            InDungeonManager.Inst.StartEnteringHallway(roomData);
        }

        public void SetRoomData(RoomData newRoomData)
        {
            this.roomData = newRoomData;
        }
    }
}