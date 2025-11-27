using _02_Scripts.InDungeon.UI.Interface;
using UnityEngine;

namespace _02_Scripts.InDungeon.UI
{
    public class SelectedUnitBar : MonoBehaviour, IFollowingBar
    {
        private Vector3 offset;
        private Camera viewCamera;
        private Transform target;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform ??= GetComponent<RectTransform>();
        }

        public void UpdatePosition()
        {
            if (target is null || viewCamera is null) return;
            
            rectTransform.position = viewCamera.WorldToScreenPoint(target.position) + offset;
        }
        
        public void SetOffset(Vector3 newOffset) { offset = newOffset; }

        public void SetTarget(Transform newTarget) { target = newTarget; }

        public void SetViewCamera(Camera newCamera) { viewCamera = newCamera; }
    }
}
