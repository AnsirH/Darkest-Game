using DarkestLike.Singleton;
using DarkestLike.ScriptableObj;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.Character
{
    /// <summary>
    /// 캐릭터 데이터를 관리하는 매니저 클래스
    /// 4개 캐릭터(헌터, 마법사, 의사, 역병의사)의 기본 정보를 저장하고 관리합니다.
    /// </summary>
    public class CharacterDataManager : Singleton<CharacterDataManager>
    {
        [Header("Character Bases")]
        [SerializeField] CharacterBase hunterBase;
        [SerializeField] CharacterBase magicianBase;  
        [SerializeField] CharacterBase PriestBase;
        [SerializeField] CharacterBase plagueDoctorBase;

        [Header("Character Data")]
        private List<CharacterData> characterDatas = new();
        
        // Properties - 정보 접근만
        public List<CharacterData> CharacterDatas => characterDatas;
        public int CharacterCount => characterDatas.Count;

        protected override void Awake()
        {
            base.Awake();
            InitializeCharacters();
        }

        /// <summary>
        /// 초기 캐릭터 생성 (정보만)
        /// 4개 캐릭터를 레벨 1로 초기화합니다.
        /// </summary>
        private void InitializeCharacters()
        {
            characterDatas.Clear();

            // 헌터 생성
            if (hunterBase != null)
            {
                CharacterData hunter = new CharacterData(hunterBase, "헌터", 0);
                characterDatas.Add(hunter);
            }

            // 마법사 생성
            if (magicianBase != null)
            {
                CharacterData mage = new CharacterData(magicianBase, "마법사", 0);
                characterDatas.Add(mage);
            }

            // 의사 생성
            if (PriestBase != null)
            {
                CharacterData healer = new CharacterData(PriestBase, "의사", 0);
                characterDatas.Add(healer);
            }

            // 역병의사 생성
            if (plagueDoctorBase != null)
            {
                CharacterData plagueDoctor = new CharacterData(plagueDoctorBase, "역병의사", 0);
                characterDatas.Add(plagueDoctor);
            }

            Debug.Log($"CharacterDataManager: {characterDatas.Count}개 캐릭터가 초기화되었습니다.");
        }

        /// <summary>
        /// 특정 인덱스로 캐릭터 데이터를 가져옵니다.
        /// </summary>
        /// <param name="index">캐릭터 인덱스 (0-3)</param>
        /// <returns>캐릭터 데이터 (없으면 null)</returns>
        public CharacterData GetCharacterData(int index)
        {
            if (index >= 0 && index < characterDatas.Count)
            {
                return characterDatas[index];
            }
            
            Debug.LogWarning($"CharacterDataManager: 인덱스 {index}는 유효하지 않습니다. (범위: 0-{characterDatas.Count - 1})");
            return null;
        }

        /// <summary>
        /// 모든 캐릭터 데이터를 가져옵니다.
        /// </summary>
        /// <returns>모든 캐릭터 데이터 리스트</returns>
        public List<CharacterData> GetAllCharacterDatas()
        {
            return new List<CharacterData>(characterDatas);
        }
    }
}