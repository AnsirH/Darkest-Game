using DarkestLike.Character;
using DarkestLike.InDungeon.Unit;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using UnityEngine;

namespace DarkestLike.InDungeon
{
    public class UnitSubsystem : InDungeonSubsystem
    {
        // Variables
        List<CharacterData> characterDatas = new();
        
        // Properties

        protected override void OnInitialize()
        {
        }

        public void SetCharacterDatas(List<CharacterData> characterDatas)
        {
            this.characterDatas = characterDatas;
            
            InDungeonManager.Inst.PartyCtrl.InitCharacterUnits(characterDatas);
        }
    }
}
