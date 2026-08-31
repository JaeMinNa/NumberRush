using UnityEngine;
using Cysharp.Threading.Tasks;

public class BattleModule : MonoBehaviour
{
    #region Member Property
    protected GameObject m_CharacterRoot = null;
    protected GameObject m_CameraRoot = null;
    protected GameObject m_EnvironmentRoot = null;
    #endregion

    #region Instance
    // 인스턴스
    private static BattleModule m_Instance;

    public static BattleModule Instance
    {
        get
        {
            return m_Instance;
        }
    }

    public static T CreateModule<T>() where T : BattleModule
    {
        GameObject obj = GameObject.Find("BattleModule");

        if (obj == null)
        {
            obj = new GameObject("BattleModule");
            DontDestroyOnLoad(obj);
        }

        var Module = obj.GetComponent<T>();
        if (Module != null)
        {
            DestroyModule();
        }

        Module = obj.AddComponent<T>();
        m_Instance = Module;

        return Module;
    }

    public static void DestroyModule()
    {
        DestroyImmediate(m_Instance);
        m_Instance = null;
    }
    #endregion

    // BattleModule을 상속받는 Module에서 대부분 공통으로 사용하는 기능을 구현
    // 자식 Module에서 구현하지 않아도 된다.
    #region Virtual Method
    // 게임 시작
    // 로드 할 내용이 많이 때문에 Delay를 준다.
    public async virtual UniTask StartGame()
    {
        // 1. 모든 UI 닫기
        //UIManager.Instance.CloseAll();

        await UniTask.Delay(100);

        // 2.

        await UniTask.Delay(100);

        // 3.

        await UniTask.Delay(100);

        // ....
    }

    // 게임 끝
    public virtual void EndGame()
    {
        //if (MonsterMovementSystem != null)
        //    Destroy(MonsterMovementSystem);

        //if (m_AutoPlayController != null)
        //    Destroy(m_AutoPlayController);

        //SetStartGame(false);
        //m_IsPause = true;
        //m_IsEndGame = true;

        // Module 제거
        DestroyModule();
    }

    // 게임 재시작
    public async virtual UniTask RestartGame()
    {
        // 일시정지 해제
        SetPause(false);

        await UniTask.Yield();
    }
    #endregion

    // BattleModule을 상속받는 module에서 별다른 구현 없이 공통적으로 사용하는 기능을 구현
    #region Public Method
    public bool IsModule<T>() where T : BattleModule
    {
        return this is T;
    }

    public void SetRootObject(GameObject cameraRoot, GameObject environmentRoot, GameObject characterRoot)
    {
        m_CameraRoot = cameraRoot;
        m_EnvironmentRoot = environmentRoot;
        m_CharacterRoot = characterRoot;
    }

    public void SetPause(bool isOn, bool isShowPauseUI = false)
    {
        if (isOn)
        {
            Time.timeScale = 0f;

            if (isShowPauseUI)
                UIManager.Instance.Open<Popup_Pause>(UI.Popup, "Prefabs/UI/Popup/Popup_Pause");
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
    #endregion
}