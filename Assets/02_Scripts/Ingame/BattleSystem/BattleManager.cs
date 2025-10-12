using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public List<Character> Characters = new();

    public void NextTurn()
    {
        foreach (Character character in Characters)
        {
            character.UpdateTurn();
        }
    }
}
