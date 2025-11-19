using System;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.Character;
using DarkestLike.InDungeon.Manager;
using DarkestLike.Map;
using DarkestLike.ScriptableObj;
using UnityEngine;

namespace DarkestLike.InDungeon.Test
{
    public class DungeonTester : MonoBehaviour
    {
        [SerializeField] private MapSOData mapSOData;
        [SerializeField] private List<CharacterBase> characterBases = new();
        private void Start()
        {
            List<CharacterData> characterDatas = new();
            foreach (CharacterBase characterBase in characterBases)
            {
                characterDatas.Add(new CharacterData(characterBase));
            }
            InDungeonManager.Inst.EnterDungeon(MapGenerator.GenerateMap(mapSOData, 0), characterDatas);
        }
    }
}
