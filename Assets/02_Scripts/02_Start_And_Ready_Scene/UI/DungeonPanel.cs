using DarkestLike.Map;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonPanel : MonoBehaviour
{
    [System.Serializable]
    public class MapButton
    {
        public MapData map;
        public GameObject buttonObj;
        public Image buttonImage;

        public void UpdateInfo()
        {
            if (map == null) { buttonObj.SetActive(false); }
            else
            {
                buttonImage.color = map.MapSOData.buttonColor;
                buttonObj.SetActive(true);
            }
        }
    }

    [Header("UI")]
    public TextMeshProUGUI dungeonNameText;
    public TextMeshProUGUI dungeonLevelText;
    public RectTransform bossGaugeRect;
    public MapButton[] mapButtons = new MapButton[3];

    [HideInInspector]
    public Dungeon dungeon;

    public void UpdateInfo()
    {
        if (dungeonNameText.text != dungeon.data.DungeonName) dungeonNameText.text = dungeon.data.DungeonName;
        if (dungeonLevelText.text != dungeon.level.ToString()) dungeonLevelText.text = dungeon.level.ToString();

        float bossGaugeRate = dungeon.exp / dungeon.data.RequireEXP[dungeon.level];
        Vector3 bossGaugeScale = new Vector3(bossGaugeRate, 1, 1);

        bossGaugeRect.localScale = bossGaugeScale;
        for (int i = 0; i < mapButtons.Length; i++)
        {
            mapButtons[i].map = null;
            mapButtons[i].UpdateInfo();
        }
        for (int i = 0; i < dungeon.selectedMap.Count; i++)
        {
            mapButtons[i].map = dungeon.selectedMap[i];
            mapButtons[i].UpdateInfo();
        }
    }

    public void OnClickMapButton(int index)
    {
        DungeonDataManager.Inst.SetCurrentMap(dungeon.selectedMap[index]);
    }
}
