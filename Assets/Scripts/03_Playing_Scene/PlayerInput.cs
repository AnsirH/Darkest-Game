using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private float moveHorizontalInput = 0;

    public float MoveHorizontalInput => moveHorizontalInput;

    private float moveHorizontalInputRaw = 0;

    public float MoveHorizontalInputRaw => moveHorizontalInputRaw;


    void Update()
    {
        moveHorizontalInput = Input.GetAxis("Horizontal");
        moveHorizontalInputRaw = Input.GetAxisRaw("Horizontal");
    }
}
