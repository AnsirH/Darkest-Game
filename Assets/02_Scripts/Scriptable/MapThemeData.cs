using DarkestLike.InDungeon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.Map
{
    [CreateAssetMenu(fileName = "ThemeData", menuName = "Create Data Asset/Create Theme Data")]
    public class MapThemeData : ScriptableObject
    {
        [Header("테마 이름")]
        public string Name;

        [Header("테마 배경 요소")]
        public GameObject groundPrefab;
        public RandomProp[] bigProps;
        public RandomProp[] middleProps;
        public RandomProp[] smallProps;

        [Header("몬스터")]
        public GameObject monsterPrefab;
    }
}