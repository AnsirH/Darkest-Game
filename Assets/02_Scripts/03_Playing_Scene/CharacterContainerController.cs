using DarkestGame.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterContainerController : MonoBehaviour
{
    [Header("이동")]
    public LimitedMovement movement;

    [Header("입력")]
    public PlayerInput input;

    Vector3 horizontalVector = Vector3.right;

    private void Start()
    {
        SetLimit();
    }

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

    void SetLimit()
    {
        Vector3 startPoint = Vector3.zero, endPoint = Vector3.zero;
        if (MapManager.Inst.CurrentLocation == CurrentLocation.Room)
            endPoint.x = MapManager.Inst.TileWorldDistance;
        else
            endPoint.x = MapManager.Inst.CurrentHallway.tiles.Length * MapManager.Inst.TileWorldDistance;
        movement.UpdateLimit(startPoint, endPoint);
    }
}
