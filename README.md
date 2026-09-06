# 🖥️ Number Rush
+ 당신의 뇌의 피지컬을 보여주세요!
+ 끊임없이 다가오는 블록을 사친연산으로 파괴하세요!
+ 숫자들을 수집하세요!
+ 랭킹을 올려보세요!
<br/>

## 📽️ 개인 프로젝트 소개
 - 게임 이름 : Number Rush
 - 플랫폼 : Android
 - 장르 : 2D 퍼즐 디펜스 캐주얼 수집
 - 개발 기간 : 25.07.28 ~ 25.09.05
<br/>

## 🎯 개발 목표
 - HTTP 서버 통신 구현
 - 구글 로그인 구현
 - AWS 라이브 서버 셋팅
 - AI 적극 활용
<br/>

## ⚙️ Environment
- `Unity 6000.3.13f1`
- **IDE** : Visual Studio 2026
- **VCS** : Git (GitHub Desktop)
- **Platform** : Android
- **Resolution** : 1080 x 1920 `FHD`
<br/>

## ▶️ 게임 스크린샷

<p align="center">
  <img src="https://github.com/user-attachments/assets/b6b0eef1-77d3-4b0a-82e5-209b516450b3" width="49%"/>
  <img src="https://github.com/user-attachments/assets/c977b0e8-aef5-4ada-a269-22c5fe00ac9b" width="49%"/>
</p>
<p align="center">
  <img src="https://github.com/user-attachments/assets/e43d0db6-50b6-40ae-9112-6872adc6f362" width="49%"/>
  <img src="https://github.com/user-attachments/assets/f8d3c142-dc84-4c4a-b6b9-f1a83ec4f3bb" width="49%"/>
</p>
<p align="center">
  <img src="https://github.com/user-attachments/assets/cb0af2da-6a81-47bd-842a-407c4ba263b8" width="49%"/>
  <img src="https://github.com/user-attachments/assets/b8993ae9-b307-49c2-910e-761e6f47782f" width="49%"/>
</p>
<br/>

## 🔳 초기 기획
![image](https://github.com/user-attachments/assets/dbc3b220-0872-460b-85b0-60fd00cb0f97)


## 🧩 클라이언트 <-> 서버 통신 방식
![image](https://github.com/user-attachments/assets/0d35107a-69f3-4f4d-aea8-13c93cb854e4)


## ✏️ 구현 기능

### 1. HTTP 서버 통신 구현

#### 구현 이유
- 프로젝트의 UI를 체계적으로 관리하기 위해
- 유지보수성과 확장성을 극대화하기 위해
- 게임의 규모가 커질수록 UI 요소를 개별적으로 제어하기 힘들기 때문에

#### 구현 방법
- 프로젝트 내 모든 UI 오브젝트가 공통적으로 가져야 할 동작을 추상화(Abstract)한 베이스 클래스
```C#
public abstract class UIElement : MonoBehaviour
{
    public string UIName = string.Empty;
    public RectTransform RectTransform;
    public UI UIParent;

    // 초기화
    public abstract void Init();
    // UI가 열릴 때 호출
    public abstract void OnOpen(List<object> Args);
    // UI가 닫힐 때 호출
    public abstract void OnClose();
    // UI 갱신
    public abstract void OnRefresh();

    #region Async
    public virtual async UniTask InitAsync()
    {
        await UniTask.Yield();
    }

    public virtual async UniTask OnOpenAsync(List<object> Args)
    {
        await UniTask.Yield();
    }

    public virtual async UniTask OnCloseAsync()
    {
        await UniTask.Yield();
    }

    public virtual async UniTask OpenAction()
    {
        await UniTask.Yield();
    }

    public virtual async UniTask CloseAction()
    {
        await UniTask.Yield();
    }
    #endregion
}
```
<br/>

- 캔버스들을 자동으로 찾아 Dictionary에 저장하여, UI 계층 구조를 명확하게 하고 정렬 순서 충돌 방지
```C#
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
```
<br/>

- 제네릭 기반의 UI 동적 로딩
```C#
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
```
<br/>

- UI 닫기 기능 관리
```C#
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
```
<br/>

- UI 전체 자동 업데이트를 위한 Refresh 기능
```C#
 public void Refresh()
 {
     foreach (var Elements in m_UIDictionary)
     {
         if (Elements.Value != null && Elements.Value.gameObject.activeInHierarchy)
             Elements.Value.OnRefresh();
     }
 }
```
<br/>

- UI를 레이어별로 구분하여 UIManager가 UI를 정확한 우선순위와 규칙에 따라 배치할 수 있도록 Root UI 구조를 설계
<img src="https://github.com/user-attachments/assets/1cee13a4-3d1a-4adb-a381-8b8ee7ef2b05" width="50%"/>
<br/>
<br/>

### 2. SystemPopup 구현
<img src="https://github.com/user-attachments/assets/e890102f-a2c2-4371-9139-193bb282d0f6" width="50%"/>

#### 구현 이유
- 반복적으로 사용되는 공통 팝업 UI를 호출하기 위해
- 일관된 팝업 생성 로직으로 UI 관리의 안정성을 높히기 위해

#### 구현 방법
- SystemPopup 호출
```C#
public void OpenSystemPopup(MessageData Data)
{
    Open<Popup_System>(UI.Popup, "Prefabs/UI/Popup/Popup_System", new List<object> { Data });
}
```
<br/>

- Popup 유형을 enum으로 정의
```C#
public enum PopupType
{
    None,

    OkOnly,
    OkCancel,

    Max
}
```
<br/>

- Popup 에 필요한 정보를 class로 정의
```C#
public class MessageData
{
    public PopupType Type;
    public string Title;
    public string Message;
    public UnityAction OkAction;
}
```
<br/>

- PopupType에 따른 UI 동적 구성 및 버튼 적용
```C#
public override void OnOpen(List<object> Args)
{
    if (Args.Count == 0)
    {
        Debug.LogWarning("MessageData is Null");
        return;
    }

    MyData = Args[0] as MessageData;

    if (!string.IsNullOrEmpty(MyData.Title))
    {
        Text_Title.text = MyData.Title;
    }
    else
    {
        Text_Title.text = "Notice";
    }
    
    Text_Message.text = MyData.Message;

    switch (MyData.Type)
    {
        case PopupType.OkOnly:
            {
                Btn_OK.gameObject.SetActive(true);
                Btn_Cancel.gameObject.SetActive(false);
            }
            break;
        case PopupType.OkCancel:
            {
                Btn_OK.gameObject.SetActive(true);
                Btn_Cancel.gameObject.SetActive(true);
            }
            break;
    }
}
```
<br/>

### 3. Photon을 이용한 멀티 플레이 구현
<p align="center">
  <img src="https://github.com/user-attachments/assets/acf17504-67c4-405c-9ca6-ee35ce4e7c47" width="49%"/>
  <img src="https://github.com/user-attachments/assets/493a5f77-5be8-4938-ba49-e5765738ac92" width="49%"/>
</p>

#### 구현 이유
- 서버를 직접 구축하지 않고도 실시간 멀티플레이 기능을 구현하기 위해
- 서버리스(Serverless) 구조 기반의 게임을 개발하기 위해

#### 구현 방법
- OnPhotonSerializeView를 통한 프레임 단위 동기화로 마스터클라이언트에서 계산한 게임 상태값을 상대 플레이어에게 빠르게 전달
- 실시간으로 데이터를 송수신해야하는 준비여부, 선택 스킬, 남은 턴 시간 데이터를 동기화 함
```C#
 public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
 {
     if (m_pvpModule == null)
     {
         m_pvpModule = BattleModule.Instance as PVPModule;
         if (m_pvpModule == null)
             return;
     }

     // 데이터를 전송할 때
     if (stream.IsWriting)
     {
         // Ingame Data
         stream.SendNext(m_pvpModule.IsMyReady);
         stream.SendNext(m_pvpModule.MySelectBtnNum);
         stream.SendNext(PhotonNetwork.IsMasterClient? m_pvpModule.CurTime : 0);
     }
     // 데이터를 받을 때
     else
     {
         // Ingame Data
         m_pvpModule.IsEnemyReady = (bool)stream.ReceiveNext();
         m_pvpModule.EnemySelectBtnNum = (int)stream.ReceiveNext();
         float curTime = (float)stream.ReceiveNext();

         if (!PhotonNetwork.IsMasterClient)
             m_pvpModule.CurTime = curTime;
     }
 }
```
<br/>

- 다른 클라이언트의 함수를 직접 호출하기 위해 RPC 함수 사용
- RPC 함수로 실시간 이모티콘 전송 기능을 구현
```C#
[PunRPC]
public void RPCPlayEmoticon(bool isLeft, int num)
{
    if (m_IngameWindow == null)
        m_IngameWindow = UIManager.Instance.GetOpened<IngameWindow>();

    if (m_IngameWindow != null)
        m_IngameWindow.SetEmoticon(isLeft, num);
}
```
<br/>

### 4. 금칙어 적용
<img src="https://github.com/user-attachments/assets/8650e8bd-839c-4ff8-9b9a-ee5ee70f5c0b" width="50%"/>

#### 구현 이유
- 부적절한 닉네임 사용을 제한하기 위해
- 유지보수와 업데이트가 쉽기 때문에 CSV 파일 기반 금칙어 구현

#### 구현 방법
- CSV 파일로 관리되는 금칙어 목록을 읽어와 메모리에 로드
```C#
private void LoadBannedWords()
{
    if (m_BannedWords.Count > 0) return;

    TextAsset csvFile = ResourceLoader.LoadAssetResources<TextAsset>("CSV/BannedWord/BannedWord");
    if (csvFile == null)
    {
        Debug.LogError("금칙어 CSV 파일을 찾을 수 없습니다.");
        return;
    }

    // 줄 단위로 분리
    string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

    foreach (var line in lines)
    {
        string word = line.Trim();
        if (!string.IsNullOrEmpty(word))
        {
            // 중복 방지
            if (!m_BannedWords.Contains(word))
                m_BannedWords.Add(word);
        }
    }

    Debug.Log($"금칙어 {m_BannedWords.Count}개 로드 완료");
}
```
<br/>

- 금칙어가 포함되어 있는지 체크
```C#
private bool IsBannedNickName(string nickname)
{
    foreach (var banned in m_BannedWords)
    {
        if (nickname.Contains(banned, StringComparison.OrdinalIgnoreCase))
            return true;
    }
    return false;
}
```
<br/>

### 5. 뒤끝 서버를 이용한 랭킹 구현
<img src="https://github.com/user-attachments/assets/ca5b51b5-cf98-40a8-a349-d212802c868f" width="50%"/>

#### 구현 이유
- 서버를 직접 구축하지 않고도 안정적이고 관리 가능한 랭킹 구현을 위해

#### 구현 방법
- 뒤끝 서버 설치 및 서버 접속
```C#
private void BackendSetup()
{
BackendReturnObject bro = Backend.Initialize(true);

if (bro.IsSuccess())
{
    Debug.Log("뒤끝 서버 연동 성공 : " + bro); // 성공일 경우 statusCode 204 Success
}
else
{
    Debug.LogError("뒤끝 서버 연동 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
}
}
```
<br/>

- 뒤끝 서버에서 비교할 데이터의 데이터 테이블 생성
<img src="https://github.com/user-attachments/assets/e9fc4d4c-2d22-4af5-bf05-ffef142f4600" width="50%"/>
<br/>
<br/>

```C#
// 데이터 테이블에 추가하는 함수
public void InsertData()
{
    Param param = GetUserDataParam();
    BackendReturnObject bro = Backend.GameData.Insert("USER_DATA", param); // USER_DATA 테이블 이름

    if (bro.IsSuccess())
    {
        Debug.LogWarning("뒤끝 서버 데이터 추가 성공!");
    }
    else
    {
        Debug.LogWarning("뒤끝 서버 데이터 추가 실패");
    }
}

// Param : 데이터를 송수신할 때 사용하는 class
private Param GetUserDataParam()
{
    Param param = new Param();
    param.Add("RankPoint", DataManager.Instance.GetMyUserData().UserCommonData.RankPoint);
    param.Add("CharacterImg", DataManager.Instance.GetMyUserData().UserCommonData.Image);

    return param;
}
```
<br/>

- 뒤끝 서버 랭킹 추가
<img src="https://github.com/user-attachments/assets/eca92b5d-5868-4f2e-ba13-717a0060c88d" width="50%"/>
<br/>
<br/>

- 랭킹 데이터 갱신
```C#
public void SaveMyRank()
{
    string rowInDate = string.Empty;

    // 랭킹 데이터를 업데이트하려면 게임 데이터에서 사용하는 데이터의 inDate 값 필요
    BackendReturnObject bro = Backend.GameData.GetMyData("USER_DATA", new Where());

    if (!bro.IsSuccess())
    {
        Debug.LogWarning("뒤끝 서버 랭킹 업데이트를 위한 데이터 조회 중 문제 발생");
        return;
    }

    Debug.LogWarning("뒤끝 서버 랭킹 업데이트를 위한 데이터 조회 성공!");

    if (bro.FlattenRows().Count > 0)
    {
        rowInDate = bro.FlattenRows()[0]["inDate"].ToString();
    }
    else
    {
        Debug.LogWarning("뒤끝 서버 랭킹 업데이트를 위한 데이터가 존재하지 않음");
    }

    Param param = new Param()
    {
        {"RankPoint",  DataManager.Instance.GetMyUserData().UserCommonData.RankPoint}
    };

    // 해당 데이터테이블의 데이터를 갱신하고, 랭킹 데이터 정보 갱신
    bro = Backend.URank.User.UpdateUserScore(RANK_UUID, "USER_DATA", rowInDate, param);

    if (bro.IsSuccess())
    {
        Debug.LogWarning("뒤끝 서버 랭킹 등록 성공!");
    }
    else
    {
        Debug.LogWarning("뒤끝 서버 랭킹 등록 실패");
    }
}
```
<br/>

- 뒤끝 서버 Json 데이터를 파싱해서 나의 랭킹 불러오기
```C#
public RankData GetMyRankData()
{
    // 내 랭킹 정보 불러오기 
    BackendReturnObject bro = Backend.URank.User.GetMyRank(RANK_UUID);

    if (bro.IsSuccess())
    {
        try
        {
            JsonData rankDataJson = bro.FlattenRows();

            // 받아온 데이터의 개수가 0 -> 데이터가 없음
            if (rankDataJson.Count <= 0)
            {
                Debug.LogWarning("뒤끝 서버 나의 랭킹 데이터가 존재하지 않음");
                return null;

            }
            else
            {
                Debug.LogWarning("뒤끝 서버 나의 랭킹 조회 성공!");

                RankData data = new RankData()
                {
                    NickName = rankDataJson[0]["nickname"].ToString(),
                    Rank = int.Parse(rankDataJson[0]["rank"].ToString()),
                    RankPoint = int.Parse(rankDataJson[0]["score"].ToString()),

                    // 추가 항목 데이터
                    Image = rankDataJson[0]["CharacterImg"].ToString()
                };

                return data;
            }
        }
        // 나의 랭킹 정보 JSON 데이터 파싱에 실패했을 때
        catch (System.Exception e)
        {
            Debug.LogWarning($"뒤끝 서버 나의 랭킹 데이터 파싱 실패 : {e}");
            return null;
        }
    }
    else
    {
        Debug.LogWarning("뒤끝 서버 나의 랭킹 데이터 불러오기 실패");
        return null;
    }
}
```
<br/>

- 뒤끝 서버 Json 데이터를 파싱해서 유저 랭킹 불러오기
```C#
public List<RankData> GetRankDataList()
{
    int maxRankList = ClientDef.RankingCount;

    // 랭킹 테이블에 있는 유저의 offset ~ offset + limit 순위 랭킹 정보를 불러옴
    BackendReturnObject bro = Backend.URank.User.GetRankList(RANK_UUID, maxRankList, 0);

    if (bro.IsSuccess())
    {
        try
        {
            JsonData rankDataJson = bro.FlattenRows();

            // 받아온 데이터의 개수가 0 -> 데이터가 없음
            if (rankDataJson.Count <= 0)
            {
                Debug.LogWarning("뒤끝 서버 랭킹 데이터가 존재하지 않음");

                return null;
            }
            else
            {
                Debug.LogWarning("뒤끝 서버 랭킹 조회 성공!");

                List<RankData> rankData = new List<RankData>();
                int rankCount = rankDataJson.Count;

                for (int index = 0; index < rankCount; ++index)
                {
                    RankData data = new RankData()
                    {
                        NickName = rankDataJson[index]["nickname"].ToString(),
                        Rank = int.Parse(rankDataJson[index]["rank"].ToString()),
                        RankPoint = int.Parse(rankDataJson[index]["score"].ToString()),

                        // 추가 항목 데이터
                        Image = rankDataJson[index]["CharacterImg"].ToString()
                    };

                    rankData.Add(data);
                }

                return rankData;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"뒤끝 서버 랭킹 데이터 파싱 실패 : {e}");
            return null;
        }
    }
    else
    {
        Debug.LogWarning("뒤끝 서버 랭킹 데이터 불러오기 실패");
        return null;
    }
}
```
<br/>

### 6. Google Admob 광고 구현
<img src="https://github.com/user-attachments/assets/5351db87-58f9-4f79-999d-e2a5df0e39c7" width="50%"/> 

#### 구현 이유
- 유저들이 광고를 시청함으로써, 게임의 수익화를 실현하기 위해

#### 구현 방법
- Google Admob에서 보상형 광고 구현
- SDK 초기화, 광고 로딩, 실패 처리, 보상 지급까지 모든 과정을 AdManager에서 통제
- 로딩 → 표시 → 보상 지급 → 재로딩 까지 한 사이클을 자동 처리하도록 설계
```C#
    private void Init()
    {
        if (IsTestMode)
        {
            // 테스트용 광고 단위 ID
#if UNITY_ANDROID
            m_AdRewardUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
            m_AdRewardUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
            m_AdRewardUnitId = "unused";
#endif
        }
        else
        {
            // 실제 배포용 광고 단위 ID (수정 필요)
#if UNITY_ANDROID
            m_AdRewardUnitId = "ca-app-pub-5906820670754550/8653741011";
#elif UNITY_IPHONE
            m_AdRewardUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
            m_AdRewardUnitId = "unused";
#endif
        }

        // Google Mobile Ads SDK 초기화
        MobileAds.Initialize((InitializationStatus initStatus) => { });
    }

// 리워드 광고 로드 및 표시
public void LoadRewardedAd(Action action)
{
    if (m_IsLoadingReward) return;
    m_IsLoadingReward = true;

    m_Action = action;

    // 이전 광고 객체가 남아 있다면 정리
    if (m_RewardedAd != null)
    {
        m_RewardedAd.Destroy();
        m_RewardedAd = null;
    }

    // 광고 요청 생성
    var adRequest = new AdRequest();

    // 광고 요청 전송
    RewardedAd.Load(m_AdRewardUnitId, adRequest,
        (RewardedAd ad, LoadAdError error) =>
        {
            m_IsLoadingReward = false;

            // 빌드에서 오류가 나기 때문에, 메인 스레드에서 실행해야 한다!
            UniTask.Post(() =>
            {
                // 에러 처리
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);

                    //광고 불러오기 실패
                    UIManager.Instance.OpenSystemPopup(new MessageData
                    {
                        Type = PopupType.OkOnly,
                        Title = "알림",
                        Message = "광고 불러오기를 실패 했습니다."
                    });

                    return;
                }

                Debug.LogWarning("Rewarded ad loaded with response : "
                      + ad.GetResponseInfo());

                m_RewardedAd = ad;
                RegisterEventHandlers(m_RewardedAd);
                ShowRewardedAd();
            });
        });
}

// 리워드 광고 표시
private void ShowRewardedAd()
{
    if (m_RewardedAd != null && m_RewardedAd.CanShowAd())
    {
        m_RewardedAd.Show((Reward reward) =>
        {
            Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");

            UniTask.Post(() =>
            {
                try
                {
                    // 보상 처리 로직
                    m_Action?.Invoke();
                    m_Action = null;

                    // 완료
                    UIManager.Instance.OpenSystemPopup(new MessageData
                    {
                        Type = PopupType.OkOnly,
                        Title = "알림",
                        Message = "광고 골드를 획득 했습니다."
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AdManager] Exception during reward handling: {e}");
                }
            });
        });
    }
    else
    {
        Debug.LogWarning("Rewarded ad is not ready yet.");
        LoadRewardedAd(m_Action);
    }
}

// 리워드 광고 이벤트 등록
private void RegisterEventHandlers(RewardedAd ad)
{
    ad.OnAdPaid += (AdValue adValue) =>
    {
        Debug.LogWarning(String.Format("Rewarded ad paid {0} {1}.",
            adValue.Value,
            adValue.CurrencyCode));
    };
    ad.OnAdImpressionRecorded += () =>
    {
        Debug.LogWarning("Rewarded ad recorded an impression.");
    };
    ad.OnAdClicked += () =>
    {
        Debug.LogWarning("Rewarded ad was clicked.");
    };
    ad.OnAdFullScreenContentOpened += () =>
    {
        Debug.LogWarning("Rewarded ad full screen content opened.");
    };
    ad.OnAdFullScreenContentClosed += () =>
    {
        Debug.LogWarning("Rewarded ad full screen content closed.");
    };
    ad.OnAdFullScreenContentFailed += (AdError error) =>
    {
        Debug.LogError("Rewarded ad failed to open full screen content " +
                       "with error : " + error);
        LoadRewardedAd(m_Action);
    };
}
```
<br/>

### 7. Mobile Notifications를 사용한 로컬 푸시 기능
<img src="https://github.com/user-attachments/assets/3f398d92-d012-4917-9791-0496ea45824e" width="50%"/>

#### 구현 이유
- 게임의 재접속 유도 및 플레이 지속률(리텐션)을 높이기 위해
- 서버 없이도 알림 기능 구현

#### 구현 방법
- 앱 시작 시 Notification 환경 초기화
```C#
private void RegisterAndroidChannel()
{
    var channel = new AndroidNotificationChannel()
    {
        Id = "my_channel_id",
        Name = "Real Fighter",
        Importance = Importance.High,
        Description = "Generic notifications",
    };
    AndroidNotificationCenter.RegisterNotificationChannel(channel);

    Debug.LogWarning("Register Android Channel");
}
```
<br/>

- 로컬 푸시의 종류 정의
```C#
public enum LocalPushType
{
    None,

    Test,
    FreeGold,

    Max
}
```
<br/>

- 로컬 푸시 예약 기능 구현
```C#
public void SchedulePushNotification(LocalPushType pushType, string title, string message, DateTime scheduleTime)
{
    // 예약 시간이 현재보다 미래인지 확인
    if (scheduleTime <= Util.DateTimeNow)
    {
        Debug.LogWarning("The time is earlier or equal to the current time. Please enter a valid future time.");
        return;
    }

    try
    {
        // Android: 알림 객체 생성 및 설정
        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = message;
        notification.FireTime = scheduleTime;
        notification.LargeIcon = "icon_0";
        notification.SmallIcon = "icon_1";
        notification.ShowInForeground = true;
        string channelId = "my_channel_id";

        int pushCode = AndroidNotificationCenter.SendNotification(notification, channelId);
        switch (pushType)
        {
            case LocalPushType.FreeGold:
                PlayerPrefs.SetInt(ClientDef.LOCALKEY_Push_FreeGold, pushCode);
                break;

            default:
                break;
        }
    }
    catch (Exception e)
    {
        Debug.LogWarning("푸시알람 예약 중 오류 발생: " + e.ToString());
    }
}
```
<br/>

- 로컬 푸시 취소 기능 구현
```C#
public void CancelPushNotification(LocalPushType pushType)
{
    int pushCode = 0;
    switch (pushType)
    {
        case LocalPushType.FreeGold:
            pushCode = PlayerPrefs.GetInt(ClientDef.LOCALKEY_Push_FreeGold, 0);
            break;

        default:
            break;
    }

    if (pushCode == 0)
        return;

    AndroidNotificationCenter.CancelScheduledNotification(pushCode);

    Debug.LogWarning("Complete Cancel to Push Notification.");
}
```
<br/>

### 8. 튜토리얼 구현
<img src="https://github.com/user-attachments/assets/462ee72c-3eae-4e0c-8efa-6e53d270ce69" width="50%"/>  

#### 구현 이유
- 캐주얼 게임 수준의 직관적인 UI 안내 시스템 구현을 위해
- 사용자가 처음 접하면 이해하기 어려운 규칙들을 설명하기 위해
- 유저가 쉽게 게임 플레이를 학습할 수 있도록 하기 위해
- 초반 이탈률을 줄이기 위해
- 대사 기반 튜토리얼, 클릭 기반 튜토리얼을 공용으로 사용할 수 있도록 설계
- 보상을 지급하여 게임의 성장 구조를 자연스럽게 맛보게하고, 초기 플레이 동기부여 강화를 위해

#### 구현 방법
- 튜토리얼 각 단계를 enum으로 정의
```C#
public enum TutorialStep
{
    None,

    LobbyChat_0,
    LobbyChat_1,
    ClickBattle,

    IngameChat_0,
    IngameChat_1,
    ClickAttack,
    ClickReady_0,
    IngameChat_2,
    IngameChat_3,
    IngameChat_4,
    IngameChat_5,
    IngameChat_6,
    IngameChat_7,
    ClickDeffence,
    ClickReady_1,
    IngameChat_8,
    IngameChat_9,
    IngameChat_10,
    IngameChat_11,
    IngameChat_12,
    IngameChat_13,
    IngameChat_14,
    ClickLobby,

    LobbyChat_2,
    LobbyChat_3,
    LobbyChat_4,
    LobbyChat_5,

    Max
}
```
<br/>

- 각각 튜토리얼 단계에 대한 정보를 class로 정의
```C#
public class TutorialData
{
    public float TimeScale = -1;
    public Vector2 MaskSize = new Vector2 (0, 0);
    public Vector2 MaskPos = new Vector2(0, 0);
    public Action Action_Mask = null;
    public string ChatText = string.Empty;
    public bool IsUp = false;
    public bool IsDown = false;
}
```
<br/>

- 공용으로 사용할 수 있는 TutorialMask 프리팹 생성
<img src="https://github.com/user-attachments/assets/6ef6e171-ad95-44d8-99e9-f2c664b77a34" width="50%"/>
<br/>

- 클릭 했을 때, 실행할 함수들을 TutorialController에 정리
```C#
public void OnClick_Ready()
{
    m_IngameWindow.OnClick_Ready();
}

public void OnClick_Deffence()
{
    m_IngameWindow.OnClick_MyDefences(0);
}
```
<br/>

- 각각 튜토리얼 스텝을 구현
```C#
public async UniTask StartTutorial(TutorialStep step)
{
    m_IsClickButton = false;

    switch (step)
    {

        case TutorialStep.LobbyChat_0:

            // TutorialController 가져올 때 까지 대기
            await UniTask.WaitUntil(() => UIManager.Instance.GetOpened<LobbyWindow>().GetComponent<TutorialController>() != null);
            m_TutorialController = UIManager.Instance.GetOpened<LobbyWindow>().GetComponent<TutorialController>();

            TutorialData data_0 = new TutorialData()
            {

                ChatText = "어서와라. 여긴 네가 실력을 증명해야 하는 결투장이다!",
                Action_Mask = async () =>
                {
                    if (m_IsClickButton)
                        return;

                    m_IsClickButton = true;

                    await StartTutorial(TutorialStep.LobbyChat_1);
                }
            };
            await SetTutorial(data_0);

            break;

        case TutorialStep.LobbyChat_1:

            TutorialData data_1 = new TutorialData()
            {
                ChatText = "긴 말 필요 없이 바로 실전으로 가보자고! BATTLE을 클릭해봐!",
                Action_Mask = async () =>
                {
                    if (m_IsClickButton)
                        return;

                    m_IsClickButton = true;

                    await StartTutorial(TutorialStep.ClickBattle);
                }
            };
            await SetTutorial(data_1);

            break;
    }
}
```
<br/>


## 💥 트러블 슈팅

### 1. 서버를 직접 구축하지 않은 이유
#### 문제 상황
- 1:1 실시간 PVP를 구현을 위해 플레이어 간 프레임 단위 실시간 동기화 요구
- 랭킹 시스템을 구현해야 함

#### 해결 방안
##### HTTP 통신 구현
- 실제로 서버를 구축하여, 원하는대로 커스텀하여 사용 가능
- 많은 유저들을 수용할 수 있음
- 서버 컴퓨터, AWS, DB 등 사용이 필요
- 1인 개발 규모에서는 부담이 크고 유지보수 난이도 높음
##### 뒤끝 서버 사용
- 이미 구현되어 있는 랭킹 시스템을 그대로 사용할 수 있음
- 유지보수 부담이 거의 없음
- 빠른 개발, 빠른 출시 가능
- 그 외, 간단한 유저 데이터 저장 가능
- 하지만, 일정 사용량 이상은 유료로 전환
##### 포톤 사용
- 실시간 동기화 영역을 안정적으로 해결
- Room 생성/입장, 이벤트 브로드캐스팅 자동화
- 하지만, 일정 인원 이상을 유저를 수용할 수 없음
##### Easy Save 에셋 사용
- 로컬에 AES 암호화된 데이터 저장 가능
- 서버 부하 없이 저장/로드 가능
- 데이터 조작과 같은 상황에는 대부분 방어할 수 있지만, 데이터의 삭제는 막을 수 없음
 
#### 의견 결정
##### 직접 서버 구축 없이, 뒤끝 서버 + 포톤 + Easy Save 조합 사용
- 서버 구축·운영에 드는 리소스를 절약
- 게임 개발 속도 향상
- 1:1 PVP이기 때문에 인프라 요구량 낮음
- 써드파티의 거부감만 없다면, 비교적 쉽게 구현할 수 있음
- 세 솔루션을 조합함으로써 서버 없이도 서버가 있는 게임처럼 완전한 기능 제공 가능
<br/>

### 2. OnPhotonSerializeView 동기화
<img src="https://github.com/user-attachments/assets/51f6349a-f468-4524-a1f6-8ad7160f9853" width="50%"/>
<br/>
<br/>

#### 문제 상황
- OnPhotonSerializeView은 Photon에서 실시간 동기화가 필요한 값(위치, 회전, 이동 상태 등)을 지속적으로 송수신하는 기능
- 플레이어 정보 같은 정적 데이터까지 전송하는 것은 비효율적
- 프레임 단위로 반복 호출로 성능 저하 및 지연(Latency) 증가 가능성
- 상대 유저의 기초 데이터는 초기 1회만 전달하면 충분

#### 해결 방안
##### RPC 사용
- 필요한 시점에 필요한 데이터만 직접 호출해서 상대방에게 전송
- 초기 매칭 후 상대 데이터 전달에 적합
##### Room / Player Properties 사용
- Photon의 Key-Value 값을 이용하는 방법
 
#### 의견 결정
##### RPC 사용
- Room / Player Properties 사용 방법은 전투 씬, 이동 전에 적용해야 함
- 이미 구성해놓은 데이터 로드 구조와 충돌
- 전투 씬 입장 후, 상대 데이터는 단 1회 전달하면 충분
- RPC 방식이 가장 간단하고 정확한 시점에 데이터를 보낼 수 있음
```C#
[PunRPC]
public void RPCSetMyData(string nick, int score, string image, string heroName,
                      int skill0, int skill1, int skill2, int level, int exp, int grade, int gradeExp)
{
    MyNickName = nick;
    MyScore = score;
    MyImage = image;
    MyHeroName = heroName;
    MyHeroSkillproficiencies[0] = skill0;
    MyHeroSkillproficiencies[1] = skill1;
    MyHeroSkillproficiencies[2] = skill2;
    MyHeroLevel = level;
    MyHeroExp = exp;
    MyHeroGrade = grade;
    MyHeroGradeExp = gradeExp;
}
```
<br/>

### 3. BattleModule의 상속 구조
<img src="https://github.com/user-attachments/assets/235adc0a-b97e-48d6-919a-af0580af8a6b" width="50%"/>
<br/>
<br/>

#### 문제 상황
- 성격이 비슷한 스크립트의 공통 코드가 모든 스크립트에 중복
- 한 로직을 수정할 때 모든 전투 컨텐츠마다 수정을 반복해야 했음
- 신규 전투 컨텐츠를 추가할 때 진입 장벽이 높고, 수정 시 오류가 잦음

#### 해결 방안
##### 전투 컨텐츠 간 공통 로직을 BattleModule에 통합
- Initialize, StartGame, EndGame 등 전투 컨텐츠는 흐름이 동일하기 때문에 반복되는 코드를 BattleModule
- 모든 전투 컨텐츠가 공유하도록 설계
##### 컨텐츠마다 다르게 동작해야 하는 구간은 virtual 메서드로 분리
- 컨텐츠별로 동작을 변경해야 하면 override로 오버라이딩할 수 있도록 설계
##### 모든 컨텐츠에서 동일하게 사용하는 기능은 public 메서드로 제공
- 일시정지, 모듈 타입 체크, SetRootObject 등 완전히 동일한 기능은 BattleModule에 정의

#### 설계 구조
##### 부모 클래스: BattleModule
- 공통 전투 로직 제공
- virtual 메서드로 확장 포인트 열어둠
- 싱글톤 + 모듈 생성/삭제 기능 포함
##### 자식 클래스: PVPModule / ChapterModule 등 
- 필요한 부분만 override
- 나머지는 BattleModule의 공통 구현 재사용
<br/>

### 4. RenderTexture 최적화 적용
<img src="https://github.com/user-attachments/assets/f07042c2-256d-4ef7-9f07-8a46ed06dce0" width="50%"/>
<br/>
<br/>

#### 문제 상황
- 프리팹 갯수만큼 RenderTexture과 전용 카메라를 생성하게 되면 높은 DrawCall, 높은 메모리 사용
- 특히 모바일 환경에서는 성능 저하가 심각
- 예전 프로젝트에서 사용했던 방법이지만, 위 문제를 개선하고 싶었음

#### 해결 방안
##### 월드 공간에 Hero 프리팹을 실제로 배치하고 UI에 그대로 보여주기
- 구현이 가장 간단
- 카메라 1개만 사용 가능
- 프로젝트의 UI 구조와 충돌로 인해 추가 설정 필요
##### 하나의 카메라만 사용하고 UV Rect로 화면을 분리하는 방식
- 카메라는 정적 위치에서 다수의 Hero 프리팹을 한 번에 촬영
- 각 Hero는 미리 일정한 간격으로 배치
- UV Rect를 조절하여 RenderTexture의 특정 영역만 잘라서 표시
- 화면상에서는 마치, 각 Hero를 따로 찍은 것처럼 보임
 
#### 의견 결정
<img src="https://github.com/user-attachments/assets/e9b31750-7089-4fb3-af81-f48a41644507" width="50%"/>
<br/>

##### UV Rect로 화면을 분리하는 방식 사용
- Hero 프리팹을 일정 간격으로 배치
- 카메라 1개를 이동시켜 모든 Hero가 한 화면에 들어오도록 구성
- UV Rect를 이용해 화면 분할 표시

#### 결과
##### 성능 개선
- 카메라 갯수: N개 → 1개 감소
- RenderTexture: N개 → 1개 감소
- DrawCall 감소 및 CPU/GPU 부하 저감
##### 시각 품질 개선
- 로비 전용 애니메이션 적용
- 캐릭터마다 다른 포즈 연출 가능
- 기존 UI 구조를 그대로 유지할 수 있었음
##### 유지보수성 증가
- UV Rect 방식은 캐릭터가 추가되어도 UI만 조정하면 되므로 구조가 단순
- 여러 개의 카메라 세팅 및 관리 과정이 필요 없어졌음
<br/>

### 5. 현재 시간 구하기
<img src="https://github.com/user-attachments/assets/ec3a9e46-4bd1-47ae-a043-cbf585fa717b" width="50%"/>
<br/>
<br/>

#### 문제 상황
- 하루 1회 보상 구현을 위해서는 정확한 현재 시간을 구해야 함
- 클라이언트에서 신뢰할 수 있는 시간을 구해야 함

#### 해결 방안
##### DateTime.Now
- 가장 간단하고 직관적으로 구현 가능
- 유저가 시간을 조작할 수 있음
##### DateTime.UtcNow.AddHours(9)
- UtcNow는 세계 표준시(UTC)를 기준으로 동작
- OS 기기 시간이 바뀌어도 UTC는 조작하기 어려움
- 한국(KST) 시간으로 맞추기 위해 +9시간 보정
  
#### 의견 결정
##### DateTime.UtcNow.AddHours(9) 사용
- 세계 표준시 기반이므로 조작 난이도 증가
- 전 세계가 동일하게 사용하는 시간 기준
- 클라이언트 단독 환경에서 구현할 수 있는 가장 안전한 방식
- 서버를 따로 사용하지 않는 구조이기 때문에 최선의 방법
- 더욱 신뢰 가능한 시간을 구하기 위해서는 서버가 필요
<br/>

### 6. aab 파일의 용량 줄이는 방법
<img src="https://github.com/user-attachments/assets/3e582859-ff77-45a0-8893-3fb8e8e317f4" width="50%"/>
<br/>
<br/>

#### 문제 상황
- 구글 플레이 업로드 aab 파일은 최대 200MB까지만 업로드 가능

#### 원인 분석 및 해결
<img src="https://github.com/user-attachments/assets/b7b98e9a-7d09-47fc-a630-48973cf34ca0" width="50%"/>
<br/>

##### Resources 폴더에 포함된 불필요한 파일
- Resources 폴더 안의 파일들은 전부 빌드 용량에 포함
- 사용하지 않는 파일들은 Resources에 절대 넣어선 안됨
- 사용하지 않는 Prefab, Texture 등 완전 제거
- 548MB → 414MB (약 134MB 감소)
##### 3D 캐릭터 모델 텍스처
- 빌드 리포트 분석 결과, 3D 캐릭터 텍스처가 전체 용량 대부분을 차지
- 빌드 시, ASTC 12x12 block으로 강한 압축 적용
- 이미지 품질 일부 저하를 감수하고 용량 최적화 우선 적용
- 414MB → 193MB (약 221MB 감소)
##### WAV 오디오 파일 압축
- 고용량 WAV BGM 3개를 사용
- Quality를 100 → 50으로 조정
- Load Type을 Streaming으로 변경하여 메모리 적재 방식 변경
- 193MB → 177MB (약 16MB 감소)
#### 프로젝트의 규모가 커진다면?
- 위 방법들로는 200MB의 aab 파일을 빌드할 수 없음
- 추후, 에셋 번들 시스템에 대한 정리와 구현이 필수
<br/>

### 7. 구글 플레이의 손상된 기능 정책
<img src="https://github.com/user-attachments/assets/b80e6f2a-d94d-4a58-9d03-0a1b827f805d" width="50%"/>
<br/>
<br/>

#### 문제 상황
- 구글 플레이에 앱을 업로드하는 과정에서 위 메시지와 함께 총 4회 거부를 받음
- 에디터, 모바일 기기, 블루스택 등 모든 테스트 환경에서 정상 작동함에도 거부를 받음

#### 원인 분석
##### 튜토리얼 진행 불가
- 심사는 AI 자동화 또는 해외 심사자로 이루어질 가능성이 높음
- 직관적이지 않은 튜토리얼은 심사자가 진행하지 못할 수 있음
##### 연속 클릭 시, 오류 가능성
- 같은 이유로 내가 원하는대로 단 한번만 클릭을 한다는 보장이 없음
- 연속 클릭 시, 오류 가능성이 있음
##### 동작하지 않는 버튼
- 추후, 업데이트 예정인 버튼에 버튼을 연결하지 않음
- 심사 시, 버튼이 동작하지 않는 것으로 판단 가능성 있음

#### 문제 해결을 위한 개선 작업
##### 닉네임 입력 강제 → 비강제 방식으로 변경
<img src="https://github.com/user-attachments/assets/9f534a66-9ee1-4c96-9fcb-536161194d87" width="50%"/>
<br/>

- 기존 닉네임을 입력하지 않으면 다음 튜토리얼로 넘어갈 수 없었음
- 자동 닉네임이 설정되도록 변경

##### 튜토리얼 진행 방식 변경 (특정 UI 클릭 → 화면 전체 클릭 가능)
<img src="https://github.com/user-attachments/assets/5e893e4b-1634-45bc-bb3a-4c13d2cf6169" width="50%"/>
<br/>

- 내가 원하는 UI를 클릭하지 않을 수도 있음
- 특정 UI를 클릭해야만 다음 튜토리얼로 넘어가는 방식에서 화면 전체 아무곳을 클릭해도 넘어가도록 변경

##### 중복 클릭 방지를 위한 Trigger 추가
- 여러 번 클릭하면 튜토리얼 로직이 중복 실행
- 다음 단계가 2~3번씩 실행되는 문제가 있는 것을 발견
- 연속 클릭에도 튜토리얼이 오작동하지 않도록 안정성 향상
```C#
if (m_IsClickButton)
    return;

m_IsClickButton = true;
await StartTutorial(TutorialStep.LobbyChat_1);
```
<br/>

##### 비활성 기능 버튼에 안내 팝업 추가
<img src="https://github.com/user-attachments/assets/56f6d741-8642-4249-885c-8cecc1449c55" width="50%"/>
<br/>

- 동작하지 않는 버튼이 존재하면 앱 품질 저하로 거부될 수 있음
- 실제 앱에는 업데이트 예정 기능이 다수 포함
- “업데이트 예정입니다.” 팝업이 노출 되도록 추가
- 모든 버튼이 사용자에게 반응을 주도록 조치
<br/>
<br/>

## 📋 프로젝트 회고
이번 프로젝트의 가장 큰 목표는 실무 경험을 바탕으로 실무 코드 스타일을 적용한 구조적이고 일관된 코드 작성이었습니다. 그래서 서버를 사용하지 않는 멀티 플레이 게임이라는 점은 이전 프로젝트와 비슷하지만, 지난번에는 구현에 초점을 맞췄다면, 이번에는 유지보수성이 좋은 코드 작성에 초점을 맞췄습니다. 이 부분에서는 원하는대로 잘 수행한 것 같았습니다. 그래서 지난 1년간의 실무 경험으로 많이 성장했다고 느꼈습니다. 목표한 부분은 프로젝트에 잘 나타냈다고 생각하지만, 새로운 기술을 사용한 부분이 많이 없어서 아쉬웠습니다. 그래서 다음 프로젝트에서는 이때까지는 사용하지 않았던 기술에 대해서 많이 도전을 할 예정입니다. 특히, 그 중에서 HTTP 서버 통신과 에셋번들은 꼭 프로젝트에 적용시켜 잘 정리해 볼 예정입니다.
  
