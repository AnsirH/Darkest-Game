using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon.Object
{
    public class ExitDoor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] TriggerAnimationObj triggerAnimationObj;
        [SerializeField] Transform exitPoint;

        // Properties
        public Transform ExitPoint => exitPoint;

        void Awake()
        {
            if (triggerAnimationObj == null) triggerAnimationObj = GetComponent<TriggerAnimationObj>();
        }

        public void TriggerAnimation()
        {
            triggerAnimationObj.TriggerAnimation();
        }
    }
}