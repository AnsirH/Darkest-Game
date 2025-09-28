using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterContainerController : MonoBehaviour
{
    [Header("이동")]
    public Movement movement;

    [Header("입력")]
    public PlayerInput input;

    Vector3 horizontalVector = Vector3.right;

    void LateUpdate()
    {
        if (input.MoveHorizontalInput > 0)
        {
            movement.MoveForDeltaTime(input.MoveHorizontalInputRaw * horizontalVector);
        }
        else if (input.MoveHorizontalInput < 0)
        {
            movement.MoveForDeltaTime(input.MoveHorizontalInputRaw * 0.5f * horizontalVector);
        }
    }
}
