using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestGame.Map
{
    [CreateAssetMenu(fileName = "ThemeData", menuName = "Create Data Asset/Create Theme Data")]
    public class MapThemeData : ScriptableObject
    {
        [Header("�׸� �̸�")]
        public string Name;

        [Header("�׸� ��� ���")]
        public GameObject groundPrefab;
        public RandomProp[] bigProps;
        public RandomProp[] middleProps;
        public RandomProp[] smallProps;

        [Header("����")]
        public GameObject monsterPrefab;
    }
}