using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon
{
    public class CharacterAnimationController : MonoBehaviour
    {
        [Header("References")]
        public Animator anim;
        public PositionMaintainer positionMaintainer;

        private const string PARAMETER_MOVESPEED = "MoveSpeed";

        public float maxMoveSpeed = 1.0f;

        private void Update()
        {
            if (anim == null) return;
            float moveSpeed = positionMaintainer.MoveSpeedX;

            if (moveSpeed != 0)
            {
                float moveSpeedRatioValue = moveSpeed / maxMoveSpeed;

                anim.SetFloat(PARAMETER_MOVESPEED, moveSpeedRatioValue);
            } 

            else
            {
                anim.SetFloat(PARAMETER_MOVESPEED, 0.0f);
            } 
        }

        public void SetAnimator(Animator animator) { anim = animator; }

        public void ActiveIsBattle(bool active) { anim.SetBool("IsBattle", active); }
    }
}
