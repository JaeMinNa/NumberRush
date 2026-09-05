using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public enum UI
{
    Root,
    BackGround,
    Menu,
    Main,
    Top,
    Popup,
    Mask,
    Fade,
    TouchBlock,
    Max
}


public class UIManager : Singleton<UIManager>
{
    public static UIManager Instance
    {
        get
        {
            if (m_Instance == null && Application.isPlaying)
            {
                GameObject Obj = GameObject.Find("[Managers]");
                if (Obj == null)
                {
                    Obj = new GameObject("[Managers]");
                    DontDestroyOnLoad(Obj);
                }

                GameObject managerObj = GameObject.Find("[Managers]/UIManager");
                if (managerObj == null)
                {
                    managerObj = new GameObject("UIManager");
                    managerObj.transform.SetParent(Obj.transform);
                }

                m_Instance = managerObj.GetComponent<UIManager>();
                if (m_Instance == null)
                {
                    m_Instance = managerObj.AddComponent<UIManager>();
                }

                m_Instance.CreateInstance();
            }

            return m_Instance;
        }
    }

    #region Member Property
    private Dictionary<UI, Canvas> m_UIRootObjects;
    private ConcurrentDictionary<Type, UIElement> m_UIDictionary;
    private EventSystem m_EventSystem;
    public static event Action<UI, UIElement> OnOpen;
    private readonly Dictionary<Type, bool> m_AsyncOpeningStatus = new();
    #endregion

    #region Override Method
    protected override void CreateInstance()
    {
        m_UIRootObjects = new Dictionary<UI, Canvas>();
        m_UIDictionary = new ConcurrentDictionary<Type, UIElement>();

        SpriteAtlasManager.atlasRequested += RequestAtlas;
    }

    public override void DestroyInstance()
    {
        m_UIRootObjects = null;
    }
    #endregion

    private void RequestAtlas(string Tag, Action<SpriteAtlas> Callback)
    {
        Callback(ResourceLoader.LoadAsset<SpriteAtlas>($"Atlas/{Tag}", Tag));
    }

    public void SetUIRoot(GameObject Obj)
    {
        m_UIRootObjects.Clear();

        m_UIRootObjects.Add(UI.Root, Obj.GetComponent<Canvas>());

        Transform Panel = null;
        Panel = Obj.transform.Find("Background");
        if (Panel != null)
            m_UIRootObjects.Add(UI.BackGround, Panel.GetComponent<Canvas>());

        Panel = Obj.transform.Find("Main");
        if (Panel != null)
            m_UIRootObjects.Add(UI.Main, Panel.GetComponent<Canvas>());

        Panel = Obj.transform.Find("Top");
        if (Panel != null)
            m_UIRootObjects.Add(UI.Top, Panel.GetComponent<Canvas>());

        Panel = Obj.transform.Find("Popup");
        if (Panel != null)
            m_UIRootObjects.Add(UI.Popup, Panel.GetComponent<Canvas>());

        Panel = Obj.transform.Find("Mask");
        if (Panel != null)
            m_UIRootObjects.Add(UI.Mask, Panel.GetComponent<Canvas>());

        Panel = Obj.transform.Find("Fade");
        if (Panel != null)
            m_UIRootObjects.Add(UI.Fade, Panel.GetComponent<Canvas>());

        Panel = Obj.transform.Find("TouchBlock");
        if (Panel != null)
            m_UIRootObjects.Add(UI.TouchBlock, Panel.GetComponent<Canvas>());

        m_EventSystem = Obj.transform.Find("EventSystem").GetComponent<EventSystem>();

        SwitchRoot();
    }

    public void SetEventSystem(bool IsActive)
    {
        m_EventSystem.gameObject.SetActive(IsActive);
    }

    public Canvas GetRootCanvas(UI Type)
    {
        if (m_UIRootObjects.ContainsKey(Type))
            return m_UIRootObjects[Type];
        else
            return null;
    }

    public RectTransform GetRootTransform(UI Type)
    {
        if (m_UIRootObjects.ContainsKey(Type))
            return m_UIRootObjects[Type].GetComponent<RectTransform>();
        else
            return null;
    }

    public void SetActiveRoot(UI Type, bool IsActive)
    {
        if (m_UIRootObjects.ContainsKey(Type))
            m_UIRootObjects[Type].gameObject.SetActive(IsActive);
    }

    private void SwitchRoot()
    {
        // UI
        foreach (var pair in m_UIDictionary)
        {
            //if (pair.Key != typeof(FadeWindow))
            //{
            //    pair.Value?.OnClose();
            //    continue;
            //}

            if (pair.Value == null)
                continue;

            var Trans = GetPanel(pair.Value.UIParent);
            if (Trans != null)
                pair.Value.RectTransform.SetParent(Trans, false);
        }
    }

    public Transform GetPanel(UI Type)
    {
        if (m_UIRootObjects.ContainsKey(Type))
            return m_UIRootObjects[Type].transform;
        else
            return null;
    }

    public void PreLoad<T>(UI Depth, string PrefabPath, bool SetFirst = false, bool IsBundle = false) where T : UIElement
    {
        if (m_UIDictionary.ContainsKey(typeof(T)))
            return;

        GameObject prefab;
        if (IsBundle)
        {
            PrefabPath = $"Prefab/{PrefabPath}";
            string AssetName = PrefabPath.Split('/').Last();
            prefab = ResourceLoader.LoadAsset<GameObject>(PrefabPath, AssetName);
        }
        else
        {
            prefab = ResourceLoader.LoadAssetResources<GameObject>(PrefabPath);
        }

        if (prefab == null)
            return;

        GameObject obj = Instantiate(prefab, GetRootTransform(Depth));
        obj.SetActive(false);

        T comp = obj.GetComponent<T>();

        if (comp == null)
            return;

        m_UIDictionary.TryAdd(typeof(T), comp);
        m_UIDictionary[typeof(T)].UIParent = Depth;
        m_UIDictionary[typeof(T)].UIName = PrefabPath;
        m_UIDictionary[typeof(T)].RectTransform = obj.GetComponent<RectTransform>();

        if (SetFirst)
            m_UIDictionary[typeof(T)].RectTransform.SetAsFirstSibling();

        m_UIDictionary[typeof(T)].Init();

        return;
    }

    public T Open<T>(UI Depth, string PrefabPath, List<object> Args = null, bool SetFirst = false, bool IsBundle = false) where T : UIElement
    {
        if (m_UIDictionary.ContainsKey(typeof(T)))
        {
            if (m_UIDictionary[typeof(T)] != null)
            {
                m_UIDictionary[typeof(T)].gameObject.SetActive(true);
                m_UIDictionary[typeof(T)].OnOpen(Args);
                OnOpen?.Invoke(Depth, m_UIDictionary[typeof(T)]);
                return m_UIDictionary[typeof(T)] as T;
            }
            else
            {
                m_UIDictionary.TryRemove(typeof(T), out _);
            }
        }

        GameObject prefab;

        if (IsBundle)
        {
            PrefabPath = $"Prefab/{PrefabPath}";
            string AssetName = PrefabPath.Split('/').Last();
            prefab = ResourceLoader.LoadAsset<GameObject>(PrefabPath, AssetName);
        }
        else
        {
            prefab = ResourceLoader.LoadAssetResources<GameObject>(PrefabPath);
        }

        if (prefab == null)
            return null;

        GameObject obj = Instantiate(prefab, GetRootTransform(Depth));
        T comp = obj.GetComponent<T>();

        if (comp == null)
            return null;

        m_UIDictionary.TryAdd(typeof(T), comp);
        m_UIDictionary[typeof(T)].UIParent = Depth;
        m_UIDictionary[typeof(T)].UIName = PrefabPath;
        m_UIDictionary[typeof(T)].RectTransform = obj.GetComponent<RectTransform>();

        if (SetFirst)
            m_UIDictionary[typeof(T)].RectTransform.SetAsFirstSibling();

        m_UIDictionary[typeof(T)].Init();
        m_UIDictionary[typeof(T)].OnOpen(Args);
        OnOpen?.Invoke(Depth, m_UIDictionary[typeof(T)]);

        return comp;
    }

    public T GetOpened<T>() where T : UIElement
    {
        // async opening 중인 경우
        if (m_AsyncOpeningStatus.ContainsKey(typeof(T)) && m_AsyncOpeningStatus[typeof(T)])
            return null;

        if (m_UIDictionary.ContainsKey(typeof(T)) && m_UIDictionary[typeof(T)] != null)
            return m_UIDictionary[typeof(T)] as T;

        return null;
    }

    public bool IsAnyOpened(UI Depth) => m_UIDictionary.Any(Pair => m_UIRootObjects.ContainsKey(Depth)
                                                                    && Pair.Value != null
                                                                    && Pair.Value.gameObject.activeInHierarchy
                                                                    && Pair.Value.UIParent == Depth);

    public void Close<T>(bool IsDestroy = true) where T : UIElement
    {
        if (m_UIDictionary.ContainsKey(typeof(T)) && m_UIDictionary[typeof(T)] != null)
        {
            UIElement temp = m_UIDictionary[typeof(T)];
            temp.OnClose();

            if (IsDestroy)
            {
                m_UIDictionary.TryRemove(typeof(T), out _);
                Destroy(temp.gameObject);
            }
            else
                temp.gameObject.SetActive(false);
        }
    }

    //public void CloseAll()
    //{
    //    Dictionary<Type, UIElement> DontRemove = new Dictionary<Type, UIElement>();

    //    if (m_UIDictionary.ContainsKey(typeof(LobbyWindow)))
    //    {
    //        DontRemove.Add(typeof(LobbyWindow), m_UIDictionary[typeof(LobbyWindow)]);
    //        m_UIDictionary.TryRemove(typeof(LobbyWindow), out _);
    //    }

    //    if (m_UIDictionary.ContainsKey(typeof(TopWindow)))
    //    {
    //        DontRemove.Add(typeof(TopWindow), m_UIDictionary[typeof(TopWindow)]);
    //        m_UIDictionary.TryRemove(typeof(TopWindow), out _);
    //    }

    //    if (m_UIDictionary.ContainsKey(typeof(FadeWindow)))
    //    {
    //        DontRemove.Add(typeof(FadeWindow), m_UIDictionary[typeof(FadeWindow)]);
    //        m_UIDictionary.TryRemove(typeof(FadeWindow), out _);
    //    }

    //    foreach (var UI in m_UIDictionary)
    //    {
    //        if (UI.Value != null)
    //        {
    //            UI.Value.OnClose();
    //            Destroy(UI.Value.gameObject);
    //        }
    //        else
    //        {
    //            Debug.LogWarning(UI.Key.ToString());
    //        }
    //    }

    //    m_UIDictionary.Clear();

    //    foreach (var UI in DontRemove)
    //        m_UIDictionary.TryAdd(UI.Key, UI.Value);

    //    BackKeyManager.Instance.UnRegistAll();
    //    //RedPointManager.Instance.RemoveAllAction();
    //}

    public void Refresh()
    {
        foreach (var Elements in m_UIDictionary)
        {
            if (Elements.Value != null && Elements.Value.gameObject.activeInHierarchy)
                Elements.Value.OnRefresh();
        }

        // 네트워크 통신 후 Refresh 되면서 사라진 리스너 재등록
        //TutorialManager.Instance.AddTargetUIListeners().Forget();
    }

    public void OpenSystemPopup(MessageData Data)
    {
        Open<Popup_System>(UI.Popup, "Prefabs/UI/Popup/Popup_System", new List<object> { Data }, IsBundle: false);
    }

    //public async UniTask OpenPurchasePopup(List<object> Args)
    //{
    //    Open<Popup_PackageInfo>(UI.Popup, "UI/Popup/Popup_PackageInfo", Args);
    //}

    //public void ShowToastMessage(string Str)
    //{
    //    Open<Ef_Message>(UI.Popup, "UI/Popup/Ef_Message", new List<object> { Str });
    //}

    //// 결제 터치 블락 최우선 순위
    //public void SetPurchaseTouchBlock(bool isActive)
    //{
    //    if (isActive)
    //    {
    //        SetTouchBlock(isActive);
    //        IsPurchaseTouchBlock = isActive;
    //    }
    //    else
    //    {
    //        IsPurchaseTouchBlock = isActive;
    //        SetTouchBlock(isActive);
    //    }
    //}

    //public void SetTouchBlock(bool isActive)
    //{
    //    // TitleScene이면 무시
    //    if (SceneManager.GetActiveScene().name == "TitleScene")
    //        return;

    //    if (IsPurchaseTouchBlock)
    //        return;

    //    if (isActive)
    //    {
    //        var touchBlockWindow = GetOpened<TouchBlockWindow>();
    //        if (touchBlockWindow != null)
    //            touchBlockWindow.gameObject.SetActive(true);
    //    }
    //    else
    //    {
    //        var touchBlockWindow = GetOpened<TouchBlockWindow>();
    //        if (touchBlockWindow != null)
    //            touchBlockWindow.gameObject.SetActive(false);
    //    }
    //}

    //public FadeWindow ShowFade(UnityAction Action = null)
    //{
    //    FadeWindow Window = GetOpened<FadeWindow>();

    //    if (Window != null)
    //        return Window;

    //    if (Action != null)
    //        Window = Open<FadeWindow>(UI.Fade, "Prefab/UI/Common/FadeWindow", new List<object>() { Action }, IsBundle: false);
    //    else
    //        Window = Open<FadeWindow>(UI.Fade, "Prefab/UI/Common/FadeWindow", IsBundle: false);

    //    return Window;
    //}

    #region Async
    public async UniTask<T> OpenAsync<T>(UI Depth, string PrefabPath, List<object> Args = null, bool SetFirst = false, bool IsBundle = true) where T : UIElement
    {
        m_AsyncOpeningStatus[typeof(T)] = true;

        if (m_UIDictionary.ContainsKey(typeof(T)))
        {
            if (m_UIDictionary[typeof(T)] != null)
            {
                m_UIDictionary[typeof(T)].gameObject.SetActive(true);
                await m_UIDictionary[typeof(T)].OnOpenAsync(Args);
                return m_UIDictionary[typeof(T)] as T;
            }
            else
            {
                m_UIDictionary.TryRemove(typeof(T), out _);
            }
        }

        GameObject prefab;
        if (IsBundle)
        {
            PrefabPath = $"Prefab/{PrefabPath}";
            string AssetName = PrefabPath.Split('/').Last();
            prefab = await ResourceLoader.LoadAssetAsync<GameObject>(PrefabPath, AssetName);
        }
        else
        {
            prefab = await ResourceLoader.LoadResourcesAsync<GameObject>(PrefabPath);
        }

        if (prefab == null)
            return null;

        var Objs = Instantiate(prefab, GetRootTransform(Depth));
        T comp = Objs.GetComponent<T>();

        if (comp == null)
            return null;

        m_UIDictionary.TryAdd(typeof(T), comp);
        m_UIDictionary[typeof(T)].UIParent = Depth;
        m_UIDictionary[typeof(T)].UIName = PrefabPath;
        m_UIDictionary[typeof(T)].RectTransform = comp.GetComponent<RectTransform>();

        if (SetFirst)
            m_UIDictionary[typeof(T)].RectTransform.SetAsFirstSibling();

        await m_UIDictionary[typeof(T)].InitAsync();
        await m_UIDictionary[typeof(T)].OnOpenAsync(Args);

        m_AsyncOpeningStatus[typeof(T)] = false;
        return comp;
    }
    #endregion

}