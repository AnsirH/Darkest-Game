using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IngameData : ScriptableObject
{
    [Header("캐릭터 컨테이너 이동 속도")]
    public float ContainerMoveSpeed = 1.5f;

    [Header("Position Maintainer 수치")]
    public float MoveTime = 0.5f;
    public float MaxSpeed = 10;
    public float AllowedDistance = 0.05f;
}
