using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon.Object
{
    public class TriggerAnimationObj : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Animator anim;

        [Header("Variables")]
        [SerializeField] string triggerName;

        void Awake()
        {
            if (anim == null) anim = GetComponent<Animator>();
        }

        public void TriggerAnimation()
        {
            anim.SetTrigger(triggerName);
        }
    }
}
