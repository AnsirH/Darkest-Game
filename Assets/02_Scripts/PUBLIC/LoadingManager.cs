using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : Singleton<LoadingManager>
{
    public static readonly string SCENENAME_MAIN = "Main Scene";
    public static readonly string SCENENAME_STARTANDREADY = "Start And Ready Scene";
    public static readonly string SCENENAME_PLAYING = "Playing Scene";
    
    public GameObject pressAnyKeyObj;


    public async UniTaskVoid LoadSceneWithDungeonSetup(string sceneName)
    {
        await SceneManager.LoadSceneAsync("Loading Scene").ToUniTask();

        pressAnyKeyObj.SetActive(false);

        // �� �񵿱� �ε� (Ȱ��ȭ�� ���߿�)
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        loadOperation.allowSceneActivation = false;

        DungeonManager.Inst.UpdateDungeonInfo();

        while (DungeonManager.Inst.dungeonDatas.Count != DungeonManager.Inst.dungeons.Count)
        {
            await UniTask.Yield();
        }

        // �� �ε� �Ϸ� ���·� ����
        while (loadOperation.progress < 0.9f)
        {
            await UniTask.Yield();
        }
                
        pressAnyKeyObj.SetActive(true);

        // �ƹ� Ű �Է� ���
        await WaitForAnyKey();

        pressAnyKeyObj.SetActive(false);

        // �� Ȱ��ȭ
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
