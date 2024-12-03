using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("이동 관련 수치")]
    public float moveSpeed = 10.0f;


    public void MoveForDeltaTime(Vector3 direction)
    {
        transform.position += moveSpeed * Time.deltaTime * direction;
    }
}
