using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using DarkestLike.InDungeon.Unit;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    // hp바 동적 생성
    // hp바 딕셔너리 관리
    // hp바 업데이트
    public class HpBarController : MonoBehaviour
    {
        public Vector3 offset;
        [SerializeField] private HpBar hpBarPrefab;
        private Dictionary<CharacterUnit, HpBar> hpBars = new();
        private List<CharacterUnit> removedCharacterUnits = new();
        
        private void Update()
        {
            foreach (var hpBar in hpBars.Values)
            {
                hpBar.UpdatePosition();
            }
            
            // 삭제된 유닛의 체력바 삭제
            while (removedCharacterUnits.Count > 0)
                hpBars.Remove(removedCharacterUnits[^1]);
        }
        
        public void CreateHpBar(CharacterUnit targetUnit)
        {
            HpBar newHpBar = Instantiate(hpBarPrefab, transform);
            hpBars[targetUnit] = newHpBar;
            newHpBar.SetOffset(offset);
            newHpBar.SetViewCamera(InDungeonManager.Inst.ViewCamera);
            newHpBar.SetTarget(targetUnit.transform);
            newHpBar.UpdatePosition();
            UpdateHpBarValue(targetUnit);
        }

        public void RemoveHpBar(CharacterUnit targetUnit)
        {
            removedCharacterUnits.Add(targetUnit);
        }

        private void UpdateHpBarValue(CharacterUnit targetUnit)
        {
            if (targetUnit != null && hpBars.ContainsKey(targetUnit))
                hpBars[targetUnit].SetHpBarPercent((float)targetUnit.CharacterData.CurrentHealth / targetUnit.CharacterData.MaxHealth);
            else
            {
                Debug.LogError("No Target Unit HpBar");
            }
        }
    }
}
