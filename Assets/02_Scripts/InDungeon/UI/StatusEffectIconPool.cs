using System.Collections.Generic;
using DarkestLike.InDungeon.BattleSystem;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    /// <summary>
    /// 상태 이상 아이콘 오브젝트 풀 관리
    /// </summary>
    public class StatusEffectIconPool : MonoBehaviour
    {
        [SerializeField] private StatusEffectIcon iconPrefab;
        [SerializeField] private Transform poolParent;

        [Header("Icon Sprites")]
        [SerializeField] private Sprite poisonSprite;
        [SerializeField] private Sprite bleedSprite;
        [SerializeField] private Sprite stunSprite;
        [SerializeField] private Sprite buffSprite;
        [SerializeField] private Sprite debuffSprite;

        private Queue<StatusEffectIcon> availableIcons = new Queue<StatusEffectIcon>();

        private void Start()
        {
            // 초기 풀 생성 (20개) - Return을 활용하여 큐에 추가
            for (int i = 0; i < 20; i++)
            {
                StatusEffectIcon icon = Instantiate(iconPrefab, poolParent);
                Return(icon);
            }
        }

        /// <summary>
        /// 풀에서 아이콘을 가져옵니다. 필요 시 새로 생성합니다.
        /// </summary>
        public StatusEffectIcon Get(StatusEffectType type)
        {
            StatusEffectIcon icon;

            if (availableIcons.Count > 0)
            {
                icon = availableIcons.Dequeue();
            }
            else
            {
                // 풀이 비었을 때만 새로 생성 (바로 사용하므로 큐에 안 넣음)
                icon = Instantiate(iconPrefab, poolParent);
            }

            icon.Show();
            Sprite sprite = GetSpriteForType(type);
            return icon;
        }

        /// <summary>
        /// 아이콘을 풀로 반환합니다.
        /// </summary>
        public void Return(StatusEffectIcon icon)
        {
            if (icon == null) return;

            icon.Hide();
            icon.transform.SetParent(poolParent);
            availableIcons.Enqueue(icon);
        }

        /// <summary>
        /// 상태 이상 타입에 따른 스프라이트를 반환합니다.
        /// </summary>
        public Sprite GetSpriteForType(StatusEffectType type)
        {
            return type switch
            {
                StatusEffectType.Poison => poisonSprite,
                StatusEffectType.Bleed => bleedSprite,
                StatusEffectType.Stun => stunSprite,
                StatusEffectType.Buff => buffSprite,
                StatusEffectType.Debuff => debuffSprite,
                _ => null
            };
        }
    }
}
