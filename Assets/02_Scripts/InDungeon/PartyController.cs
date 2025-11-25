using DarkestLike.Map;
using DarkestLike.InDungeon.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DarkestLike.Character;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Object;
using DarkestLike.InDungeon.Unit;

namespace DarkestLike.InDungeon
{
    public class PartyController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] LimitedMovement movement;
        [SerializeField] PlayerInput input;
        [SerializeField] Transform camTrf;
        [SerializeField] Transform exitPoint;
        
        [Header("Character")]
        [SerializeField] CharacterUnit characterUnitPrefab;
        [SerializeField] Transform[] positionTargets;
        // Properties
        public Transform CamTrf => camTrf;
        public List<CharacterUnit> CharacterUnits => characterUnits;

        // Variables
        List<CharacterUnit> characterUnits = new();
        Vector3 horizontalVector = Vector3.right;
        bool isSetMoveGround = false;
        bool isInEndPoint = false;
        bool isFreeze = false;
        int currentTileIndex = 0;
        float endDistance;
        

        public void InitCharacterUnits(List<CharacterData> characterDatas)
        {
            for (int i = 0; i < characterDatas.Count; ++i)
            {
                CharacterUnit newUnit = Instantiate(characterUnitPrefab);
                newUnit.transform.position = positionTargets[i].position;
                newUnit.Initialize(characterDatas[i], positionTargets[i], false);
                characterUnits.Add(newUnit);
            }
        }
        
        private void Update()
        {
            if (isInEndPoint && input.Up && !isFreeze)
            {
                StartCoroutine(MoveToExitDoor());
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
                movement.moveSpeed = 10.0f;
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                movement.moveSpeed = 1.5f;
        }

        void LateUpdate()
        {
            if (isFreeze) return;
            if (input.MoveHorizontalInput > 0)
            {
                movement.MoveForDeltaTime(input.MoveHorizontalInputRaw * horizontalVector);
            }
            else if (input.MoveHorizontalInput < 0)
            {
                movement.MoveForDeltaTime(input.MoveHorizontalInputRaw * 0.5f * horizontalVector);
            }
        }
        
        public void SetMovableLimit(float movableDistance)
        {
            Vector3 startPoint = Vector3.zero, endPoint = Vector3.zero;
            endPoint.x = movableDistance;
            movement.UpdateLimit(startPoint, endPoint);

            isSetMoveGround = true;
        }

        public void ActiveFreeze(bool active)
        {
            isFreeze = active;
        }

        public void ResetPosition()
        {
            transform.position = Vector3.zero;
            
        }

        private IEnumerator MoveToExitDoor()
        {
            DungeonEventBus.Publish(DungeonEventType.ExitHallway);
            isFreeze = true;
            Vector3[] newPositions = new Vector3[positionTargets.Length];
            for (int i = 0; i < newPositions.Length; ++i)
                newPositions[i] = positionTargets[i].localPosition;

            InDungeonManager.Inst.FadeOut(2);
            float timer = 0.0f;
            while (timer < 3.0f)
            {
                timer += Time.deltaTime;
                for (int i = 0; i < positionTargets.Length; ++i)
                    positionTargets[i].position = Vector3.MoveTowards(positionTargets[i].position, exitPoint.position,
                        movement.moveSpeed * Time.deltaTime);
                yield return null;
            }

            ResetPosition();
            for (int i = 0; i < positionTargets.Length; ++i)
                positionTargets[i].localPosition = newPositions[i];
            ResetMembersPosition();
            isFreeze = false;
            InDungeonManager.Inst.EnterExitRoom();
        }

        public void ResetMembersPosition()
        {
            for (int i = 0; i < characterUnits.Count; ++i)
                characterUnits[i].SetPositionToTarget();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("ExitDoor"))
            {
                isInEndPoint = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("ExitDoor"))
            {
                isInEndPoint = false;
            }
        }
    }
}