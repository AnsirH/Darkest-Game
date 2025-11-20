using DarkestLike.InDungeon;
using DarkestLike.InDungeon.Hallway;
using DarkestLike.SceneLoad;
using System;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Object;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace DarkestLike.Map
{
    public enum CurrentLocation
    {
        Room,
        Hallway
    }

    // 맵과 현재 플레이어의 위치를 관리하는 객체
    // 맵 전환
    // 현재 플레이어 위치 업데이트
    public class MapSubsystem : InDungeonSubsystem
    {
        [Header("References")] 
        [SerializeField] private GameObject roomEnvironment;
        [SerializeField] private GameObject hallwayEnvironment;
        [SerializeField] private ExitDoor exitDoor;
        [SerializeField] private PartyController party;
        
        [Header("Variables")]
        [SerializeField] float tileLength = 5.0f;
        
        [Header("Map Transition Setting")]
        [SerializeField] float transitionTime = 0.5f;
        
        // Variables
        private MapData map;
        private HallwayData currentHallway;
        private RoomData currentRoom;
        private TileData currentTile;
        private CurrentLocation currentLocation;

        // Properties
        public MapData MapData => map;
        public HallwayData CurrentHallway => currentLocation == CurrentLocation.Hallway ? currentHallway : null;
        public RoomData CurrentRoom => currentLocation == CurrentLocation.Room ? currentRoom : null;
        public TileData CurrentTile => currentLocation == CurrentLocation.Hallway ? currentTile : null;

        public CurrentLocation CurrentLocation => currentLocation;
        public float TileLength => tileLength;

        private void Update()
        {
            if (currentLocation == CurrentLocation.Hallway)
            {
                float partyInterval = party.transform.position.x - currentTile.Position.x;
                if (partyInterval > tileLength)
                {
                    SetCurrentTile(currentHallway.GetNextTile(currentTile));
                }
                else if (partyInterval < -tileLength)
                {
                    SetCurrentTile(currentHallway.GetPreviousTile(currentTile));
                }
            }
        }

        protected override void OnInitialize()
        {
        }

        public void SetRoomData(RoomData roomData)
        {
            currentLocation = CurrentLocation.Room;
            currentRoom = roomData;
            roomEnvironment.SetActive(true);
            hallwayEnvironment.SetActive(false);
            exitDoor.gameObject.SetActive(false);
            party.ResetPosition();
        }

        public void SetHallwayData(HallwayData hallwayData)
        {
            currentLocation = CurrentLocation.Hallway;
            currentHallway = hallwayData;
            currentTile = currentHallway.Tiles[0];
            for (int i = 0; i < currentHallway.Tiles.Length; ++i)
                currentHallway.Tiles[i].SetPosition(Vector3.right * (tileLength * i + tileLength * 0.5f));
            
            exitDoor.gameObject.SetActive(true);
            float moveableDistance = tileLength * currentHallway.Tiles.Length + tileLength * 0.5f;
            exitDoor.transform.position = Vector3.right * moveableDistance;
            party.SetMovableLimit(moveableDistance);
            party.ResetPosition();
        }

        public override void Shutdown()
        {
            base.Shutdown();
            // SceneManager.sceneLoaded -= SceneLoadedHandler;
        }

        public void SetMapData(MapData mapData)
        {
            map = mapData;
            currentRoom = map.Rooms[0];
            currentLocation = CurrentLocation.Room;
        }

        private bool CanEnterableTile(TileData tileData)
        {
            if (currentLocation != CurrentLocation.Hallway) return false;

            for (int i = 0; i < currentHallway.Tiles.Length; ++i)
            {
                //if (currentHallway.Tiles[i] == currentTile) continue;
                if (currentHallway.Tiles[i] == tileData) return true;
            }

            return false;
        }

        public bool SetCurrentTile(TileData tileData)
        {
            if (CanEnterableTile(tileData))
            {
                currentTile = tileData;
                DungeonEventBus.Publish(DungeonEventType.EnterTile);
                
                // 몬스터 타일 타입 체크
                if (CurrentTile.type == TileType.Monster)
                {
                    InDungeonManager.Inst.StartBattle(CurrentTile.EnemyGroup);
                }
                
                return true;
            }
            else return false;
        }
        
        public void EnterTheNextRoom(PartyController characterController)
        {
            if (currentLocation != CurrentLocation.Hallway) return;
            
            currentLocation = CurrentLocation.Room;
            currentRoom = currentHallway.ExitRoom;
            currentHallway = null;
            currentTile = null;
        }

        private IEnumerator EnterTheRoomCoroutine(PartyController characterController)
        {
            // todo: fade out 
            yield return new WaitForSeconds(transitionTime);
            
            
            currentLocation = CurrentLocation.Room;
            currentRoom = currentHallway.ExitRoom;
            currentHallway = null;
            currentTile = null;
            
            hallwayEnvironment.SetActive(false);
            roomEnvironment.SetActive(true);
            
            yield return new WaitForSeconds(transitionTime);
            
            characterController.ResetPosition();
            characterController.SetMovableLimit(tileLength);
        }
        
        // public void MoveToHallway()
    }
}