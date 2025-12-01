using System.Text;
using DarkestLike.InDungeon.Unit;
using TMPro;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public partial class BattleHud : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI characterInfoText;

        private StringBuilder sb;

        private void Awake()
        {
            sb = new StringBuilder();
        }

        public void SetCharacterInfo(CharacterUnit characterUnit)
        {
            sb.Clear();
            sb.AppendLine($"이름: {characterUnit.CharacterName}");
            sb.AppendLine($"체력: {characterUnit.CharacterData.CurrentHealth}/{characterUnit.CharacterData.MaxHealth}");
            sb.AppendLine($"공격: {characterUnit.CharacterData.Attack}");
            sb.AppendLine($"방어: {characterUnit.CharacterData.Defense}");
            sb.AppendLine($"속도: {characterUnit.CharacterData.Speed}");
            sb.AppendLine($"회피: {characterUnit.CharacterData.Evasion}");
            
            characterInfoText.text = sb.ToString();
        }
    }
}