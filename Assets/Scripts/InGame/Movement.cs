using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("�̵� ���� ��ġ")]
    public float moveSpeed = 10.0f;


    public void MoveForDeltaTime(Vector3 direction)
    {
        transform.position += moveSpeed * Time.deltaTime * direction;
    }
}
