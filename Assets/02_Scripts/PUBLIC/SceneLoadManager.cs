using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    public static readonly string SCENENAME_MAIN = "Main Scene";
    public static readonly string SCENENAME_STARTANDREADY = "Start And Ready Scene";
    public static readonly string SCENENAME_PLAYING = "Playing Scene";
    
    public GameObject pressAnyKeyObj;


    public async UniTaskVoid LoadSceneWithDungeonSetup(string sceneName)
    {
        await SceneManager.LoadSceneAsync("Loading Scene").ToUniTask();

        pressAnyKeyObj.SetActive(false);

        // 씬 비동기 로드 (활성화는 나중에)
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        loadOperation.allowSceneActivation = false;

        DungeonManager.Inst.UpdateDungeonInfo();

        while (DungeonManager.Inst.dungeonDatas.Count != DungeonManager.Inst.dungeons.Count)
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
    }

    private async UniTask WaitForAnyKey()
    {
        while (!Input.anyKeyDown)
        {
            await UniTask.Yield();
        }
    }
}
