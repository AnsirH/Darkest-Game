using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon.Movement
{
    public class Movement : MonoBehaviour
{
    [Header("Variables")]
    public float moveSpeed = 10.0f;


    public virtual void MoveForDeltaTime(Vector3 direction)
    {
        transform.position += moveSpeed * Time.deltaTime * direction;
    }
    }
}
