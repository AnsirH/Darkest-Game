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

        private void Update()
        {
            foreach (var hpBar in hpBars.Values)
            {
                hpBar.UpdatePosition();
            }
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
            if (hpBars.TryGetValue(targetUnit, out HpBar hpBar))
            {
                Destroy(hpBar.gameObject);
                hpBars.Remove(targetUnit);
            }
        }

        /// <summary>
        /// 특정 유닛의 HP 바를 현재 HP에 맞춰 업데이트합니다.
        /// </summary>
        public void UpdateHpBarValue(CharacterUnit targetUnit)
        {
            if (targetUnit != null && hpBars.ContainsKey(targetUnit))
                hpBars[targetUnit].SetHpBarPercent((float)targetUnit.CharacterData.CurrentHealth / targetUnit.CharacterData.MaxHealth);
            else
            {
                Debug.LogError("No Target Unit HpBar");
            }
        }

        /// <summary>
        /// 특정 유닛의 HP 바를 활성화/비활성화합니다.
        /// </summary>
        public void SetHpBarActive(CharacterUnit targetUnit, bool isActive)
        {
            if (targetUnit != null && hpBars.TryGetValue(targetUnit, out HpBar hpBar))
            {
                hpBar.gameObject.SetActive(isActive);
            }
        }

        /// <summary>
        /// 여러 유닛의 HP 바를 활성화/비활성화합니다.
        /// </summary>
        public void SetMultipleHpBarsActive(List<CharacterUnit> units, bool isActive)
        {
            if (units == null) return;

            foreach (var unit in units)
            {
                if (unit != null && hpBars.TryGetValue(unit, out HpBar hpBar))
                {
                    hpBar.gameObject.SetActive(isActive);
                }
            }
        }
    }
}
