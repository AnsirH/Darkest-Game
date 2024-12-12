using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> ĳ���� �ִϸ��̼� ���� </summary>
public class CharacterAnimationController : MonoBehaviour
{
    [Header("�ִϸ�����")]
    public Animator anim;

    private const string PARAMETER_MOVESPEED = "MoveSpeed";

    [Header("ĳ���� �̵� ���� ��ũ��Ʈ")]
    public PositionMaintainer positionMaintainer;

    public float maxMoveSpeed = 1.0f;

    private void Update()
    {
        float moveSpeed = positionMaintainer.MoveSpeedX;

        if (moveSpeed != 0)
        {
            float moveSpeedRatioValue = moveSpeed / maxMoveSpeed;   // �̵� �ӵ� ���� ������ ���

            anim.SetFloat(PARAMETER_MOVESPEED, moveSpeedRatioValue);
        } // �̵� ���� ��

        else
        {
            anim.SetFloat(PARAMETER_MOVESPEED, 0.0f);
        } // �������� ��
    }
}
