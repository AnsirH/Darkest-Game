using System.Collections.Generic;
using _02_Scripts.InDungeon.UI.Interface;
using DarkestLike.InDungeon.BattleSystem;
using DarkestLike.InDungeon.Unit;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    /// <summary>
    /// 캐릭터의 상태 이상 아이콘을 표시하는 UI
    /// IFollowingBar 인터페이스를 구현하여 캐릭터를 따라다님
    /// </summary>
    public class StatusEffectBar : MonoBehaviour, IFollowingBar
    {
        public RectTransform rectTransform;
        [SerializeField] private RectTransform iconContainer;

        private Vector3 offset;
        private Transform target;
        private Camera viewCamera;

        private CharacterUnit targetUnit;
        private StatusEffectIconPool iconPool;
        private Dictionary<StatusEffectType, StatusEffectIcon> activeIcons = new Dictionary<StatusEffectType, StatusEffectIcon>();
        private Dictionary<StatusEffectType, int> cachedDurations = new Dictionary<StatusEffectType, int>();

        private void Awake()
        {
            rectTransform ??= GetComponent<RectTransform>();
        }

        /// <summary>
        /// StatusEffectBar를 초기화합니다.
        /// </summary>
        public void Initialize(CharacterUnit unit, StatusEffectIconPool pool)
        {
            targetUnit = unit;
            iconPool = pool;
            cachedDurations.Clear();
            activeIcons.Clear();
        }

        /// <summary>
        /// 위치를 업데이트합니다 (IFollowingBar 구현)
        /// </summary>
        public void UpdatePosition()
        {
            if (target == null || viewCamera == null) return;

            rectTransform.position = viewCamera.WorldToScreenPoint(target.position) + offset;
        }

        /// <summary>
        /// 상태 이상 아이콘을 업데이트합니다.
        /// </summary>
        public void UpdateIcons()
        {
            // 유닛이 죽었거나 없으면 아이콘 제거
            if (targetUnit == null || !targetUnit.IsAlive)
            {
                ClearIcons();
                return;
            }

            // Dirty flag: activeEffects 변경 시에만 업데이트
            if (HasEffectsChanged())
            {
                RefreshIcons(targetUnit.CharacterData.ActiveEffects);
            }
        }

        /// <summary>
        /// 상태 이상이 변경되었는지 확인 (Dirty Flag 패턴)
        /// </summary>
        private bool HasEffectsChanged()
        {
            var currentEffects = targetUnit.CharacterData.ActiveEffects;

            // 빠른 체크: 개수 변경
            if (currentEffects.Count != cachedDurations.Count)
                return true;

            // 상세 체크: 타입별 duration 비교
            foreach (var effect in currentEffects)
            {
                // 새로운 타입이 추가되었거나
                if (!cachedDurations.ContainsKey(effect.type))
                    return true;

                // 지속시간이 변경되었으면
                if (cachedDurations[effect.type] != effect.duration)
                    return true;
            }

            // 타입이 제거되었는지 확인
            foreach (var cachedType in cachedDurations.Keys)
            {
                bool found = false;
                foreach (var effect in currentEffects)
                {
                    if (effect.type == cachedType)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 아이콘을 새로고침합니다.
        /// </summary>
        private void RefreshIcons(List<StatusEffect> effects)
        {
            // 타입별로 최대 지속시간 효과만 수집 (중복 제거)
            Dictionary<StatusEffectType, StatusEffect> currentEffects = new Dictionary<StatusEffectType, StatusEffect>();
            foreach (var effect in effects)
            {
                if (!currentEffects.ContainsKey(effect.type) ||
                    currentEffects[effect.type].duration < effect.duration)
                {
                    currentEffects[effect.type] = effect;
                }
            }

            // 제거된 타입의 아이콘 제거
            List<StatusEffectType> typesToRemove = new List<StatusEffectType>();
            foreach (var type in activeIcons.Keys)
            {
                if (!currentEffects.ContainsKey(type))
                {
                    typesToRemove.Add(type);
                }
            }
            foreach (var type in typesToRemove)
            {
                iconPool.Return(activeIcons[type]);
                activeIcons.Remove(type);
                cachedDurations.Remove(type);
            }

            // 아이콘 업데이트 또는 생성
            foreach (var kvp in currentEffects)
            {
                StatusEffectType type = kvp.Key;
                StatusEffect effect = kvp.Value;

                if (activeIcons.ContainsKey(type))
                {
                    // 기존 아이콘 duration 업데이트
                    Sprite sprite = iconPool.GetSpriteForType(type);
                    activeIcons[type].SetEffect(effect, sprite);
                    cachedDurations[type] = effect.duration;
                }
                else
                {
                    // 새 아이콘 생성
                    StatusEffectIcon icon = iconPool.Get(type);
                    if (icon != null)
                    {
                        Sprite sprite = iconPool.GetSpriteForType(type);
                        icon.SetEffect(effect, sprite);
                        icon.transform.SetParent(iconContainer, false);  // Horizontal Layout Group이 자동 정렬
                        activeIcons[type] = icon;
                        cachedDurations[type] = effect.duration;
                    }
                }
            }
        }

        /// <summary>
        /// 모든 아이콘을 제거하고 풀로 반환합니다.
        /// </summary>
        public void ClearIcons()
        {
            foreach (var icon in activeIcons.Values)
            {
                if (icon != null)
                {
                    iconPool.Return(icon);
                }
            }
            activeIcons.Clear();
            cachedDurations.Clear();
        }

        // IFollowingBar 인터페이스 구현
        public void SetOffset(Vector3 newOffset) { offset = newOffset; }
        public void SetTarget(Transform newTarget) { target = newTarget; }
        public void SetViewCamera(Camera newCamera) { viewCamera = newCamera; }
    }
}
