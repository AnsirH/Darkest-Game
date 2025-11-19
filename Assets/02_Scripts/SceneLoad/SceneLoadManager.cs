using Cysharp.Threading.Tasks;
using DarkestLike.InDungeon;
using DarkestLike.Map;
using DarkestLike.Singleton;
using System;
using System.Collections;
using System.Collections.Generic;
using DarkestLike.InDungeon.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkestLike.SceneLoad
{
    public class SceneLoadManager : Singleton<SceneLoadManager>
    {
        public static readonly string SCENENAME_MAIN = "Main Scene";
        public static readonly string SCENENAME_STARTANDREADY = "Select Dungeon Scene";
        public static readonly string SCENENAME_PLAYING = "Playing Scene";

        public GameObject pressAnyKeyObj;

        public event Action<string> OnLoadedSceneActivated;

        public async UniTaskVoid LoadSceneWithDungeonSetup(string sceneName)
        {
            await SceneManager.LoadSceneAsync("Loading Scene").ToUniTask();

            pressAnyKeyObj.SetActive(false);

            // 씬 비동기 로드 (활성화는 나중에)
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
            loadOperation.allowSceneActivation = false;

            DungeonDataManager.Inst.UpdateDungeonInfo();

            while (DungeonDataManager.Inst.dungeonDatas.Count != DungeonDataManager.Inst.dungeons.Count)
            {
                await UniTask.Yield();
            }

            // 씬 로드 완료 상태로 설정
            while (loadOperation.progress < 0.9f)
            {
                await UniTask.Yield();
            }

            pressAnyKeyObj.SetActive(true);

            // 아무 키 입력 대기
            await WaitForAnyKey();

            pressAnyKeyObj.SetActive(false);

            // 씬 활성화
            loadOperation.allowSceneActivation = true;

            OnLoadedSceneActivated?.Invoke(sceneName);
        }

        private async UniTask WaitForAnyKey()
        {
            while (!Input.anyKeyDown)
            {
                await UniTask.Yield();
            }
        }

        public async UniTaskVoid LoadDungeon(MapData mapData)
        {
            await SceneManager.LoadSceneAsync("Loading Scene").ToUniTask();

            pressAnyKeyObj.SetActive(false);

            // 씬 비동기 로드 (활성화는 나중에)
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync("Room Scene");
            loadOperation.allowSceneActivation = false;

            // 씬 로드 완료 상태까지 대기
            while (loadOperation.progress < 0.9f)
            {
                await UniTask.Yield();
            }

            pressAnyKeyObj.SetActive(true);

            // 아무 키 입력 대기
            await WaitForAnyKey();

            pressAnyKeyObj.SetActive(false);

            // 씬 활성화
            loadOperation.allowSceneActivation = true;

            await UniTask.Yield();

            // InDungeonManager.Inst.EnterDungeon(mapData, ); // 유닛을 관리하는 매니저 추가 필요
        }
    }
}