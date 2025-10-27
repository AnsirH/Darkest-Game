using DarkestLike.ScriptableObj;
using DarkestLike.Character;
using DarkestLike.InDungeon.BattleSystem;
using DarkestLike.InDungeon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkestLike.InDungeon.CharacterUnit
{
    /// <summary>
    /// 캐릭터의 3D 표현과 데이터를 관리하는 클래스
    /// CharacterBase를 기반으로 CharacterData를 생성하고 관리합니다.
    /// </summary>
    public class CharacterUnit : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PositionMaintainer positionMaintainer;
        [SerializeField] CharacterAnimationController animationController;
        [SerializeField] CharacterUnitHUD hud;
        
        [Header("Variables")]
        [SerializeField] bool isEnemyUnit = false;
        
        // Variables
        CharacterData characterData;
        
        // Properties
        public CharacterData CharacterData => characterData;
        public CharacterAnimationController AnimController => animationController;
        public CharacterUnitHUD HUD => hud;
        public string CharacterName => characterData?.CharacterName ?? "Unknown";
        public int CurrentHealth => characterData?.CurrentHealth ?? 0;
        public int MaxHealth => characterData?.MaxHealth ?? 0;
        public bool IsAlive => characterData?.IsAlive ?? false;
        public bool IsEnemyUnit => isEnemyUnit;

        private void Awake()
        {
            if (positionMaintainer == null) positionMaintainer = GetComponent<PositionMaintainer>();
            if (animationController == null) animationController = GetComponent<CharacterAnimationController>();
        }

        /// <summary>
        /// CharacterData를 기반으로 CharacterUnit을 초기화합니다.
        /// </summary>
        /// <param name="characterData">캐릭터의 데이터</param>
        public void Initialize(CharacterData characterData)
        {
            if (characterData == null)
            {
                Debug.LogError("CharacterData가 null입니다.");
                return;
            }
            this.characterData = characterData;
            // 3D 모델 설정
            if (characterData.Base.ModelPrefab != null)
            {
                GameObject model = Instantiate(characterData.Base.ModelPrefab, transform);
                animationController.SetAnimator(model.GetComponent<Animator>());
            }
            else
            {
                Debug.LogWarning($"CharacterBase {characterData.Base.name}의 ModelPrefab이 설정되지 않았습니다.");
            }
        }

        /// <summary>
        /// PositionMaintainer의 타겟을 변경합니다.
        /// </summary>
        /// <param name="target">새로운 타겟</param>
        /// <param name="moveSpeed">이동 속도</param>
        public void ChangePositionMaintainerTarget(Transform target, float moveSpeed = 0.5f)
        {
            positionMaintainer.SetTarget(target);
            positionMaintainer.SetMoveTime(moveSpeed);
        }        

        /// <summary>
        /// 캐릭터를 회복시킵니다.
        /// </summary>
        /// <param name="healAmount">회복량</param>
        /// <returns>실제로 회복된 HP</returns>
        public int Heal(int healAmount)
        {
            if (characterData == null) return 0;
            return characterData.Heal(healAmount);
        }

        /// <summary>
        /// 캐릭터를 완전히 회복시킵니다.
        /// </summary>
        public void FullHeal()
        {
            characterData?.FullHeal();
        }

        /// <summary>
        /// 캐릭터를 레벨업시킵니다.
        /// </summary>
        public void LevelUp()
        {
            characterData?.LevelUp();
        }

        /// <summary>
        /// 스탯 모디파이어를 적용합니다.
        /// </summary>
        /// <param name="modifier">적용할 모디파이어</param>
        public void ApplyModifier(StatModifier modifier)
        {
            characterData?.ApplyModifier(modifier);
        }

        /// <summary>
        /// 턴을 진행합니다.
        /// </summary>
        public void UpdateTurn()
        {
            characterData?.UpdateTurn();
        }
    }
}