using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkestLike.InDungeon
{
    /// <summary>
    /// DungeonManager가 관리하는 모든 Subsystem의 베이스 클래스
    /// 초기화 순서, 이벤트 구독/해제 등 공통 기능을 제공합니다.
    /// </summary>
    public abstract class InDungeonSubsystem : MonoBehaviour
    {
        /// <summary>
        /// 초기화 완료 여부
        /// </summary>
        protected bool isInitialized = false;

        /// <summary>
        /// Subsystem 초기화 (DungeonManager가 호출)
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning($"[{GetType().Name}] Already initialized.");
                return;
            }

            OnInitialize();
            isInitialized = true;
        }

        /// <summary>
        /// 실제 초기화 로직 (하위 클래스에서 구현)
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// Subsystem 정리 및 리소스 해제
        /// </summary>
        public virtual void Shutdown()
        {
            isInitialized = false;
            Debug.Log($"[{GetType().Name}] Shutdown");
        }
    }
}

