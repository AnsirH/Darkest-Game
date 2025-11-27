using UnityEngine;

namespace _02_Scripts.InDungeon.UI.Interface
{
    public interface IFollowingBar
    {
        void UpdatePosition();
        void SetOffset(Vector3 newOffset);
        void SetTarget(Transform newTarget);
        void SetViewCamera(Camera newCamera);
    }
}
