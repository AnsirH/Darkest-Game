using DarkestLike.InDungeon.CharacterUnit;
using DarkestLike.InDungeon.Object;
using DarkestLike.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkestLike.InDungeon.Hallway
{
    public class HallwaySubsystem : InDungeonSubsystem
    {
        [Header("References")]
        [SerializeField] ExitDoor exitDoor;
        
        [Header("Variables")]
        [SerializeField] float fadeDuration;

        // Variables
        private CharacterContainerController characterContainer;

        public void ActiveExitDoor(bool active) { exitDoor.gameObject.SetActive(active); }

        public void SetHallway(CharacterContainerController characterContainer, float moveableDistance, float tileDistance)
        {
            ActiveExitDoor(true);
            exitDoor.transform.position = Vector3.right * (moveableDistance + tileDistance * 0.5f);
            this.characterContainer = characterContainer;
            characterContainer.SetMoveGround(moveableDistance, tileDistance);
        }

        public IEnumerator EnterRoomProcess(List<CharacterUnit.CharacterUnit> characterUnits)
        {
            exitDoor.TriggerAnimation();

            float moveTime = 1.0f;
            for (int i = 0; i < characterUnits.Count; ++i)
            {
                characterUnits[i].ChangePositionMaintainerTarget(exitDoor.ExitPoint, moveTime);
                moveTime += 0.5f;
                yield return null;
            }

            while (Vector3.Distance(characterUnits[1].transform.position, exitDoor.ExitPoint.position) >= 1.5f)
            {
                yield return null;
            }
        }

        protected override void OnInitialize()
        {
        }
    }
}