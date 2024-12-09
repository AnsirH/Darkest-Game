using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 캐릭터 애니메이션 관리 </summary>
public class CharacterAnimationController : MonoBehaviour
{
    [Header("애니메이터")]
    public Animator anim;

    private const string PARAMETER_MOVESPEED = "MoveSpeed";

    [Header("캐릭터 이동 관련 스크립트")]
    public PositionMaintainer positionMaintainer;

    private float maxSpeed = 1.0f;

    private void Update()
    {
        float moveSpeed = positionMaintainer.MoveSpeedX;

        if (moveSpeed != 0)
        {
            if (moveSpeed > maxSpeed) { maxSpeed = moveSpeed; } // 위치 유지장치의 속도를 단정지을 수 없어서

            float moveSpeedRatioValue = moveSpeed / maxSpeed;   // 이동 속도 비율 값으로 계산

            anim.SetFloat(PARAMETER_MOVESPEED, moveSpeedRatioValue);
        } // 이동 중일 떄

        else
        {
            anim.SetFloat(PARAMETER_MOVESPEED, 0.0f);
        } // 멈춰있을 때
    }
}
