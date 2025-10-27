using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon
{
    public class PlayerInput : MonoBehaviour
    {
        private float moveHorizontalInput = 0;

        public float MoveHorizontalInput => moveHorizontalInput;

        private float moveHorizontalInputRaw = 0;

        public float MoveHorizontalInputRaw => moveHorizontalInputRaw;

        public bool Up => Input.GetKeyDown(KeyCode.W);

        void Update()
        {
            moveHorizontalInput = Input.GetAxis("Horizontal");
            moveHorizontalInputRaw = Input.GetAxisRaw("Horizontal");
        }
    }
}
