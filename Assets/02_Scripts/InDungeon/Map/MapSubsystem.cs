using DarkestLike.InDungeon;
using DarkestLike.InDungeon.Hallway;
using DarkestLike.SceneLoad;
using System;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Object;
using DarkestLike.InDungeon.Unit;
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

                // 타일 이동 시 플레이어 유닛들의 상태이상 효과 처리
                ProcessStatusEffectsOnTileMove();

                // 몬스터 타일 타입 체크
                if (CurrentTile.type == TileType.Monster)
                {
                    InDungeonManager.Inst.StartBattle(CurrentTile.EnemyGroup);
                }

                return true;
            }
            else return false;
        }

        /// <summary>
        /// 타일 이동 시 플레이어 유닛들의 상태이상 효과를 처리합니다.
        /// </summary>
        private void ProcessStatusEffectsOnTileMove()
        {
            // 전투 중이 아닐 때만 처리
            if (InDungeonManager.Inst.BattleSubsystem.IsBattleActive)
                return;

            var playerUnits = InDungeonManager.Inst.UnitSubsystem.PlayerUnits;
            if (playerUnits == null || playerUnits.Count == 0)
                return;

            List<CharacterUnit> deadUnits = new List<CharacterUnit>();

            // 모든 플레이어 유닛의 상태이상 효과 처리
            foreach (var unit in playerUnits)
            {
                if (unit == null || !unit.IsAlive)
                    continue;

                // 상태이상 효과 처리 (전투 중 턴 시작과 동일한 로직)
                var results = unit.CharacterData.ProcessStartOfTurn();
                foreach (var result in results)
                {
                    Debug.Log($"[TileMove DOT] {unit.CharacterName}이(가) {result.effect.effectName}(으)로 {result.damageDealt} 데미지를 받았습니다.");
                    InDungeonManager.Inst.UISubsystem.UpdateHpBar(unit);
                    InDungeonManager.Inst.UISubsystem.UpdateStatusEffectIcons(unit);
                }

                // DOT로 사망 체크
                if (unit.IsDead)
                {
                    deadUnits.Add(unit);
                }
            }

            // 사망한 유닛 처리
            if (deadUnits.Count > 0)
            {
                HandlePlayerDeathsOnTileMove(deadUnits);
            }
        }

        /// <summary>
        /// 타일 이동 중 플레이어 유닛이 사망했을 때 처리합니다.
        /// </summary>
        private void HandlePlayerDeathsOnTileMove(List<CharacterUnit> deadUnits)
        {
            if (deadUnits == null || deadUnits.Count == 0)
                return;

            // 사망한 유닛들의 인덱스를 HashSet에 저장
            HashSet<int> deadIndices = new HashSet<int>();
            foreach (var unit in deadUnits)
            {
                Debug.LogWarning($"[TileMove Death] {unit.CharacterName}이(가) 상태이상으로 사망했습니다!");
                deadIndices.Add(unit.PositionIndex);

                // HP바 및 상태이상 바 제거
                InDungeonManager.Inst.UISubsystem.RemoveHpBar(unit);
                InDungeonManager.Inst.UISubsystem.RemoveStatusEffectBar(unit);

                // UnitSubsystem에서 제거
                InDungeonManager.Inst.UnitSubsystem.RemovePlayerUnit(unit);

                // GameObject 비활성화
                unit.gameObject.SetActive(false);
            }

            // 남은 플레이어 유닛들의 포지션 인덱스 재정렬
            var remainingPlayers = InDungeonManager.Inst.UnitSubsystem.PlayerUnits;
            var positionTargets = InDungeonManager.Inst.PartyCtrl.PositionTargets;

            if (remainingPlayers.Count > 0)
            {
                // 각 생존 유닛의 최종 위치 계산 및 재배치
                foreach (var unit in remainingPlayers)
                {
                    int oldIndex = unit.PositionIndex;

                    // 현재 유닛보다 앞에(인덱스가 작은) 사망한 유닛 개수 카운트
                    int deadCountBefore = 0;
                    foreach (int deadIndex in deadIndices)
                    {
                        if (deadIndex < oldIndex)
                            deadCountBefore++;
                    }

                    // 앞에 죽은 유닛 수만큼 앞으로 이동
                    if (deadCountBefore > 0)
                    {
                        int newIndex = oldIndex - deadCountBefore;
                        unit.SetPositionIndex(newIndex);

                        // PartyController의 포지션 타겟으로 이동 (탐험 중이므로 기본 속도 0.5f 사용)
                        if (newIndex < positionTargets.Length)
                        {
                            unit.ChangePositionMaintainerTarget(positionTargets[newIndex]);
                            Debug.Log($"[TileMove Death] {unit.CharacterName} 위치 재배치: {oldIndex} → {newIndex}");
                        }
                    }
                }
            }

            // 모든 플레이어가 사망했는지 체크
            if (remainingPlayers.Count == 0)
            {
                Debug.LogError("[TileMove Death] 모든 파티원이 사망했습니다! 게임 오버!");
                // TODO: 게임 오버 처리 (씬 전환, UI 표시 등)
            }
            else
            {
                Debug.Log($"[TileMove Death] 사망 처리 완료. 남은 파티원: {remainingPlayers.Count}명");
            }
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