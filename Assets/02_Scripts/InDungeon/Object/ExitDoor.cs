using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using UnityEngine;

namespace DarkestLike.InDungeon.Object
{
    public class ExitDoor : MonoBehaviour
    {
        private readonly int openID = Animator.StringToHash("Open");

        [Header("References")]
        [SerializeField] Animator animator;
        [SerializeField] Transform exitPoint;

        [Header("Variables")] public bool exitTrigger = false;
        
        // Properties
        public Transform ExitPoint => exitPoint;

        void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            DungeonEventBus.Subscribe(DungeonEventType.ExitHallway, OpenDoor);
            DungeonEventBus.Subscribe(DungeonEventType.EnterHallway, CloseDoor);
        }

        public void OpenDoor()
        {
            animator.SetBool(openID, true);
        }
        public void CloseDoor()
        {
            animator.SetBool(openID, false);
        }
    }
}