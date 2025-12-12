using _02_Scripts.InDungeon.UI.Interface;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class SelectedUnitBar : MonoBehaviour
    {
        private Vector3 offset;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetOffset(Vector3 newOffset) { offset = newOffset; }

        public void SetPosition(Vector3 screenPosition)
        {
            rectTransform ??= GetComponent<RectTransform>();
            rectTransform.position = screenPosition + offset;
        }
    }
}
