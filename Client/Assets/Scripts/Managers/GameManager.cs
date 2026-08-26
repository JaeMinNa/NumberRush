using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private string m_AccountCode = string.Empty;

    public static GameManager Instance
    {
        get
        {
            if (m_Instance == null && Application.isPlaying)
            {
                GameObject obj = GameObject.Find("[Managers]");
                if (obj == null)
                {
                    obj = new GameObject("[Managers]");
                    DontDestroyOnLoad(obj);
                }

                GameObject managerObj = GameObject.Find("[Managers]/GameManager");
                if (managerObj == null)
                {
                    managerObj = new GameObject("GameManager");
                    managerObj.transform.SetParent(obj.transform);
                }

                m_Instance = managerObj.GetComponent<GameManager>();
                if (m_Instance == null)
                {
                    m_Instance = managerObj.AddComponent<GameManager>();
                }

                m_Instance.CreateInstance();
            }

            return m_Instance;
        }
    }

    #region Get / Set
    public string AccountCode
    {
        get
        {
            return m_AccountCode;
        }
        set
        {
            m_AccountCode = value;
        }
    }
    #endregion

    #region Override Method
    protected override void CreateInstance()
    {

    }

    public override void DestroyInstance()
    {

    }
    #endregion

    #region Public Method
    public void InitDefaultManager()
    {
        //_ = NetworkManager.Instance;
        //_ = PatchManager.Instance;
        _ = UIManager.Instance;
        //_ = SoundManager.Instance;
        //_ = PoolManager.Instance;
        //_ = BackKeyManager.Instance;
        //_ = RedPointManager.Instance;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region Scene
    //public async Task LoadTitleScene()
    //{
    //    if (m_RoutineCheckResumeGame != null)
    //    {
    //        StopCoroutine(m_RoutineCheckResumeGame);
    //        m_RoutineCheckResumeGame = null;
    //    }

    //    if (m_RoutineCheckStamina != null)
    //    {
    //        StopCoroutine(m_RoutineCheckStamina);
    //        m_RoutineCheckStamina = null;
    //    }

    //    if (m_Routine_ServerTime != null)
    //    {
    //        StopCoroutine(m_Routine_ServerTime);
    //        m_Routine_ServerTime = null;
    //    }

    //    await ResourceLoader.UnloadAsset(true);

    //    SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
    //}

    //public async UniTask LoadLobbyScene()
    //{
    //    IsEndLoad = false;

    //    var FadeWindow = UIManager.Instance.GetOpened<FadeWindow>();

    //    UIManager.Instance.CloseAll();

    //    await UniTask.Delay(1000);

    //    var LoadSceneOperation = SceneManager.LoadSceneAsync("LobbyScene", LoadSceneMode.Additive);
    //    while (!LoadSceneOperation.isDone)
    //    {
    //        FadeWindow.LoadSceneProgress(LoadSceneOperation.progress);
    //        await UniTask.Yield();
    //    }

    //    await UniTask.Yield();

    //    var PrevScene = SceneManager.GetActiveScene();
    //    await SceneManager.UnloadSceneAsync(PrevScene);

    //    var CurrentScene = SceneManager.GetSceneByName("LobbyScene");
    //    SceneManager.SetActiveScene(CurrentScene);

    //    FadeWindow.LoadSceneProgress(1f);

    //    IsEndLoad = true;

    //    SoundManager.Instance.Play_BGM("bgm_lobby");
    //    UIManager.Instance.PreLoad<TouchBlockWindow>(UI.TouchBlock, "UI/Ingame/TouchBlockWindow");
    //    UIManager.Instance.Open<TopWindow>(UI.Top, "UI/Outgame/TopWindow", new List<object>() { eTopWindowType.Main });
    //    UIManager.Instance.Open<LobbyWindow>(UI.Main, "UI/Outgame/LobbyWindow", new List<object>() { eLobbyPanel.Battle });

    //    //await ResourceLoader.UnloadAsset(false);

    //    if (IAPSystem.IsInitialized)
    //    {
    //        await IAPSystem.CheckPending();
    //    }
    //    else
    //    {
    //        if (await IAPSystem.Init())
    //        {
    //            await IAPSystem.GetProductList();

    //            await IAPSystem.CheckPending();
    //        }
    //    }
    //}

    //public async UniTask LoadGameScene()
    //{
    //    IsEndLoad = false;

    //    var FadeWindow = UIManager.Instance.GetOpened<FadeWindow>();

    //    if (UIManager.Instance.GetOpened<LobbyWindow>() != null)
    //        UIManager.Instance.Close<LobbyWindow>();

    //    if (UIManager.Instance.GetOpened<TopWindow>() != null)
    //        UIManager.Instance.Close<TopWindow>();

    //    UIManager.Instance.CloseAll();

    //    await UniTask.Delay(1000);

    //    var LoadSceneOperation = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
    //    while (!LoadSceneOperation.isDone)
    //    {
    //        FadeWindow.LoadSceneProgress(LoadSceneOperation.progress);
    //        await UniTask.Yield();
    //    }

    //    await UniTask.Yield();

    //    var PrevScene = SceneManager.GetActiveScene();
    //    await SceneManager.UnloadSceneAsync(PrevScene);

    //    var CurrentScene = SceneManager.GetSceneByName("GameScene");
    //    SceneManager.SetActiveScene(CurrentScene);

    //    FadeWindow.LoadSceneProgress(1f);

    //    if (BattleModule.Instance != null)
    //        BattleModule.Instance.InitStage();

    //    IsEndLoad = true;
    //}

    public void LoadLobbyScenen()
    {
        SceneManager.LoadScene("LobbyScene");

        // todo
    }

    public void LoadGameScenen()
    {
        SceneManager.LoadScene("GameScene");

        // todo
    }

    #endregion

    #region UserData
    public void LoadUserData(string Data)
    {
        List<string> DataList = Data.Split('&').ToList();

        for (int count = 0; count < DataList.Count; ++count)
        {
            string[] Datas = DataList[count].Split('$');

            switch (Datas[0])
            {
                // 유저 기본 데이터
                case nameof(UserData_Common): User.SetUserCommonData(Util.ToObjectJson<UserData_Common>(Datas[1])); break;

                // 유저 숫자 데이터
                case nameof(UserData_Number): User.SetUserNumberData(Util.ToObjectJson<UserData_Number>(Datas[1])); break;
            }
        }
    }
    #endregion
}