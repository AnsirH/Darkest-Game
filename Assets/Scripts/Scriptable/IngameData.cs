using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IngameData : ScriptableObject
{
    [Header("ĳ���� �����̳� �̵� �ӵ�")]
    public float ContainerMoveSpeed = 1.5f;

    [Header("Position Maintainer ��ġ")]
    public float MoveTime = 0.5f;
    public float MaxSpeed = 10;
    public float AllowedDistance = 0.05f;
}
