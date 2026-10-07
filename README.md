# 🖥️ Number Rush
### ❗ 유료 에셋 사용으로 에셋 부분은 삭제하였습니다. ❗

+ 당신의 뇌의 피지컬을 보여주세요!
+ 끊임없이 다가오는 블록을 사친연산으로 파괴하세요!
+ 숫자들을 수집하세요!
+ 랭킹을 올려보세요!
<br/>
<br/>

## 📽️ 개인 프로젝트 소개
 - 게임 이름 : Number Rush
 - 플랫폼 : Android
 - 장르 : 2D 퍼즐 디펜스 캐주얼 수집
 - 개발 기간 : 26.07.28 ~ 26.09.05
<br/>

## 🎯 개발 목표
 - HTTP 서버 통신 구현
 - 구글 로그인 구현
 - AWS 라이브 서버 셋팅
 - 파이어 베이스 적용
 - 번들 패치 구현
 - AI 적극 활용
<br/>

## ⚙️ Environment
- `Unity 6000.3.13f1`
- **IDE** : Visual Studio 2026
- **VCS** : Git (GitHub Desktop)
- **Platform** : Android
- **Resolution** : 1080 x 1920 `FHD`
<br/>

## ▶️ 게임 스크린샷   ****

<p align="center">
  <img src="https://github.com/user-attachments/assets/b6b0eef1-77d3-4b0a-82e5-209b516450b3" width="30%"/>
  <img src="https://github.com/user-attachments/assets/50db07c2-b61b-4be5-925b-7a4f22e628f4" width="30%"/>
</p>
<p align="center">
  <img src="https://github.com/user-attachments/assets/c977b0e8-aef5-4ada-a269-22c5fe00ac9b" width="30%"/>
  <img src="https://github.com/user-attachments/assets/e43d0db6-50b6-40ae-9112-6872adc6f362" width="30%"/>
</p>
<p align="center">
  <img src="https://github.com/user-attachments/assets/f8d3c142-dc84-4c4a-b6b9-f1a83ec4f3bb" width="30%"/>
  <img src="https://github.com/user-attachments/assets/cb0af2da-6a81-47bd-842a-407c4ba263b8" width="30%"/>
</p>
<br/>

## 🔳 초기 기획
![image](https://github.com/user-attachments/assets/dbc3b220-0872-460b-85b0-60fd00cb0f97)


## 🧩 클라이언트 <-> 서버 통신 방식
![image](https://github.com/user-attachments/assets/0d35107a-69f3-4f4d-aea8-13c93cb854e4)


## ✏️ 구현 기능

### 1. HTTP 서버 통신 구현

#### 구현 이유
- BaaS/네트워크 솔루션을 사용하지 않고, 직접 서버를 구축하고 클라이언트와 서버가 통신하는 전체 흐름을 경험하기 위해
- 클라이언트에서 데이터를 직접 저장하지 않고 서버에서 검증 및 처리하도록 구성하여 데이터 위변조 가능성을 줄이기 위해

#### 구현 방법
- Unity 클라이언트에서는 UnityWebRequest를 사용하여 서버에 HTTP 요청
- 요청 데이터는 Header/Body 구조로 구성하고 JSON으로 직렬화하여 서버로 전송
- 서버 응답 역시 공통 Packet 형태로 받아 Header를 확인한 뒤 ContentsType에 맞는 데이터를 역직렬화하여 적용
- 향후 서버 기능이 추가되더라도 ContentsType을 기준으로 기능을 확장할 수 있도록 공통 통신 구조를 설계
- 네트워크 요청은 UniTask 기반 비동기 방식으로 처리하여 메인 스레드의 흐름을 막지 않도록 구성

```C#
private IEnumerator SendServer(PacketType packetType, ContentsType contentsType, int subType, string Data, UnityAction receiveAction)
{
    if (prevPacketType.Equals(packetType) && prevContentsType.Equals(contentsType) && prevSubType.Equals(subType))
        yield break;

    if (IsProcess)
        yield return new WaitUntil(() => !IsProcess);

    IsProcess = true;
    prevPacketType = packetType;
    prevContentsType = contentsType;
    prevSubType = subType;

    string json = Util.MakeServerPacket(packetType, Data);
    string sendData = Util.StringCompress(json);

    WWWForm formData = new WWWForm();
    formData.AddField("AccountCode", GameManager.Instance.AccountCode);
    formData.AddField("Data", sendData);

    string Url = Util.GetServerUrl(GameManager.Instance.GetServerType());

    using (UnityWebRequest www = UnityWebRequest.Post(Url, formData))
    {
        yield return www.SendWebRequest();

        string PacketTitle = $"[{MakePacketType(packetType, contentsType, subType)}]";
        Debug.LogWarning($"<color=#57DE59>{PacketTitle}</color>");

        prevPacketType = PacketType.None;
        prevContentsType = ContentsType.None;
        prevSubType = -1;

        if (www.result == UnityWebRequest.Result.Success)
        {
            ServerPacket RecvPacket = Util.ToObjectJson<ServerPacket>(Util.StringDecompress(www.downloadHandler.text));
            if (RecvPacket.StateType == PacketState.None)
            {
                switch (RecvPacket.PacketType)
                {
                    // 직접 데이터를 받아서 처리하는 패킷
                    case PacketType.GetUserData:
                        {
                            GameManager.Instance.LoadUserData(RecvPacket.Data);
                            receiveAction?.Invoke();
                        }
                        break;
                    
                    case PacketType.ContentsPacket:
                        {
                            yield return PacketSystem.ProcessPacket(RecvPacket.Data, receiveAction);
                        }
                        break;
                }
            }
            else
            {
                OnServerError(www.downloadHandler.text);
            }
        }
        else if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError($"{www.result}, {www.error}");
            OnConnectionError(packetType, contentsType, subType, Data, receiveAction);
        }
        else if (www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"{www.result}, {www.error}");
            OnServerError(www.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"{www.result}, {www.error}");
            OnServerError(www.downloadHandler.text);
        }

        www.Dispose();
    }

    IsProcess = false;
}
```
<br/>

- 클라이언트의 네트워크 요청을 한 곳에서 관리하기 위해 NetworkManager 구성

```C#
public partial class NetworkManager : Singleton<NetworkManager>
{
    public static NetworkManager Instance
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
    
                GameObject managerObj = GameObject.Find("[Managers]/NetworkManager");
                if (managerObj == null)
                {
                    managerObj = new GameObject("NetworkManager");
                    managerObj.transform.SetParent(obj.transform);
                }
    
                m_Instance = managerObj.GetComponent<NetworkManager>();
                if (m_Instance == null)
                {
                    m_Instance = managerObj.AddComponent<NetworkManager>();
                }
    
                m_Instance.CreateInstance();
            }
    
            return m_Instance;
        }
    }
}
```
<br/>

- Local/Live 환경에 따라 서버 주소를 분리하여 개발 환경과 실제 서비스 환경을 구분

```C#
public static string GetServerUrl(ServerType type)
{
    string url = string.Empty;

    switch (type)
    {
        case ServerType.Local:
            url = "http://localhost:15000/Server";
            break;

        case ServerType.Live:
            url = "http://43.201.58.20:15000/Server"; // 퍼블릭 IPv4 주소
            break;

        default:
            break;
    }

    return url;
}
}
```
<br/>

- 서버는 ASP.NET Core 기반으로 구성하고 클라이언트가 전달한 ContentsType에 따라 필요한 로직을 처리
- MongoDB에서 유저 데이터를 조회하고, 서버에서 데이터를 검증/수정한 뒤 결과를 다시 클라이언트에 전달

```C#
switch (packetData.contentsType)
{
    case UserNumberContents.SetEquip:
    {
        string data = packetData.HeaderData.Data;
        List<int> euqipNumber = ServerUtil.ToObjectJson<List<int>>(data);

        var userNumberData = await NumberMethod.GetUserNumberData(accountCode);
        userNumberData.EquipNumber = euqipNumber;

        var updateUserNumberData = await NumberMethod.ProcessUserNumberData(accountCode, userNumberData);
        outBodyData.Add(updateUserNumberData.Item1);
        outLogData.Add(updateUserNumberData.Item2);
    
        outHeaderData = ServerUtil.MakeHeaderData(UserNumberContents.SetEquip, true);
        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
        return new Tuple<PacketState, string>(packetState, result);
    }
}
```
<br/>

- 재화 구매와 같은 중요한 데이터는 클라이언트가 결과값을 결정하지 않고 서버에서 현재 데이터를 다시 조회한 뒤 검증

```C#
case UserNumberContents.BuyOneNumber_Random:
    {
        var userGameData = await GameMethod.GetUserGameData(accountCode);
        var userNumberData = await NumberMethod.GetUserNumberData(accountCode);

        // Gold 충분한지 확인
        if (userGameData.Gold < 2000)
            return await errorResponse.SetCode(0).BuildAsync();

        // Gold 소모
        userGameData.Gold -= 2000;

        // 0~99 중 랜덤 숫자 선택 (1~9 는 제외)
        Random random = new Random();
        int randomNumber;

        do
        {
            randomNumber = random.Next(0, 100);
        }
        while (randomNumber >= 1 && randomNumber <= 9);

        List<int> numList = new List<int>();
        numList.Add(randomNumber);

        // 가지고 있지 않은 숫자라면, 인벤토리 추가
        if(!userNumberData.NumberInventory.Contains(randomNumber))
            userNumberData.NumberInventory.Add(randomNumber);

        // 저장
        var updateUserGameData = await GameMethod.ProcessUserGameData(accountCode, userGameData);
        outBodyData.Add(updateUserGameData.Item1);
        outLogData.Add(updateUserGameData.Item2);

        var updateUserNumberData = await NumberMethod.ProcessUserNumberData(accountCode, userNumberData);
        outBodyData.Add(updateUserNumberData.Item1);
        outLogData.Add(updateUserNumberData.Item2);

        outHeaderData = ServerUtil.MakeHeaderData(UserNumberContents.BuyOneNumber_Random, true, ServerUtil.ToJson(numList));
        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
        return new Tuple<PacketState, string>(packetState, result);
    }
```
<br/>
<br/>


### 2. 번들 패치 구현
<img src="https://github.com/user-attachments/assets/50db07c2-b61b-4be5-925b-7a4f22e628f4" width="30%"/>
<br/>

#### 구현 이유
- 게임 업데이트가 발생할 때마다 전체 APK/AAB를 다시 배포하지 않고 변경된 리소스만 업데이트할 수 있는 구조를 구현하기 위해
- 이미지, Prefab, Audio 등의 리소스를 실행 파일과 분리하여 필요한 리소스를 서버에서 다운로드할 수 있도록 구성하기 위해
- 실제 라이브 게임에서 사용하는 패치 구조를 직접 구현하며 AssetBundle 생성부터 업로드, 다운로드, 로드까지 전체 흐름을 경험하기 위해

#### 구현 방법
- 기존 Resources 중심의 리소스 로드 구조에서 AssetBundle을 사용할 수 있도록 ResourceType을 분리
- Local과 Bundle 모드를 GameConfig에서 선택할 수 있도록 구성

```C#
public enum ResourceType
{
    Local,
    Bundle,
}

[CreateAssetMenu]
public class GameConfig : ScriptableObject
{
    [SerializeField] private ServerType m_ServerType;
    [SerializeField] private ResourceType m_ResourceType;
    [SerializeField] private int m_PackageBundleCode;

    private string m_ServerAddress = string.Empty;
    private string m_PatchAddress = string.Empty;

    public void SetConfig(ResourceType resourceType, int bundleCode)
    {
        m_ResourceType = resourceType;
        m_PackageBundleCode = bundleCode;
    }

    public void SetServerType(ServerType serverType)
    {
        m_ServerType = serverType;
    }

    public void SetPatchAddress(string addr)
    {
        m_PatchAddress = addr;
    }
}
```
<br/>

- Local/Bundle 설정을 코드 전체에서 직접 분기하지 않고 GameConfig를 기준으로 판단하도록 하여 리소스 환경 변경을 한 곳에서 관리

```C#
GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Resources/Data/GameConfig.asset");

config.SetConfig(ResourceType.Bundle, bundleCode);
config.SetServerType(serverType);

EditorUtility.SetDirty(config);
AssetDatabase.SaveAssets();
```
<br/>

- AssetBundle 파일과 패치 목록을 Amazon S3에 업로드하고 Amazon CloudFront를 CDN으로 사용하여 클라이언트가 패치 파일을 다운로드하도록 구성
- PatchList.pat 파일을 기준으로 현재 클라이언트가 보유한 리소스와 서버의 리소스를 비교하여 필요한 파일을 판단

```C#
GameManager.Instance.GameConfig.SetPatchAddress("https://dpy99u25ih8lc.cloudfront.net/Number%20Rush");
```
<br/>

- 게임 시작 시 Bundle 환경에서는 바로 로비로 진입하지 않고 PatchManager를 통해 패치 여부를 먼저 확인하도록 구성

```C#
private void Start()
{
#if UNITY_EDITOR
    if (GameManager.Instance.GetResourceType() == ResourceType.Bundle)
    {
        PatchManager.Instance.CheckPatch();
    }
    else
    {
        StartGame();
    }
#else
    PatchManager.Instance.CheckPatch();
#endif
}
```
<br/>

- 단순 Slider 진행률뿐 아니라 완료된 파일 수와 전체 파일 수, 현재 다운로드 용량과 전체 다운로드 용량을 함께 표시

```C#
public void PatchProgressDelegate(float progress, int completeFile, int totalFile, long downloadByte, long totalByte)
{
    Slider_Progress.value = progress;

    Txt_FileCount.text =
        $"{completeFile} / {totalFile}";

    Txt_DownloadSize.text =
        $"{ConvertByte(downloadByte)} / {ConvertByte(totalByte)}";
}
```
<br/>
<br/>

### 3. Firebase 적용

#### 구현 이유
- 실제 서비스 환경에서 유저의 게임 이용 흐름과 주요 이벤트를 확인할 수 있도록 로그 수집 환경을 구성하기 위해
- Android 빌드에서 앱 실행 및 주요 행동 데이터를 확인하여 추후 게임 데이터 분석에 활용하기 위해
- Firebase Cloud Messaging을 적용하여 Push 알림 기능 구현을 위해

#### 구현 방법
- Firebase 프로젝트 생성 후 Android 앱을 등록하고 google-services.json을 Unity 프로젝트에 적용
- Firebase SDK 초기화가 완료된 이후에만 Firebase 기능을 사용할 수 있도록 초기화 상태를 관리

```C#
public class FirebaseManager : Singleton<FirebaseManager>
{
    public bool IsInitialized { get; private set; }

    public void Init()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                IsInitialized = true;

                Debug.LogWarning("Firebase Initialize Success");
            }
            else
            {
                Debug.LogError($"Firebase Initialize Failed : {dependencyStatus}");
            }
        });
    }
}
```
<br/>

- Firebase Analytics를 이용하여 게임에서 발생하는 주요 이벤트를 기록
- 주요 시점에서 이벤트를 전달하여 Firebase Analytics에서 유저 행동 데이터를 확인할 수 있도록 구성
<img src="https://github.com/user-attachments/assets/4c239287-2e8b-4b06-8d18-a92fffb25b24" width="30%"/>
<br/>

```C#
FirebaseManager.Instance.LogEvent("Game Start");
```
<br/>

- 필요에 따라 UserProperty를 등록할 수 있도록 공통 함수를 구성하여 유저 상태 및 게임 데이터를 Analytics 분석에 활용할 수 있도록 확장
```C#
public void SetUserProperty(string propertyName, string value)
{
    if (!IsInitialized)
        return;

    Firebase.Analytics.FirebaseAnalytics.SetUserProperty(propertyName, value);
}
```
<br/>

- Firebase Cloud Messaging을 함께 적용하여 Push 알림을 받을 수 있는 기반을 구성
<img src="https://github.com/user-attachments/assets/b772a05e-1cee-4dd7-a74d-bbeb524213ca" width="30%"/>
<br/>

- Firebase를 게임 데이터 저장 용도로 사용하지 않고 Analytics 및 Messaging 역할로 분리

<br/>


### 4. Google 로그인 구현 (Universal SDK)
<img src="https://github.com/user-attachments/assets/b6b0eef1-77d3-4b0a-82e5-209b516450b3" width="30%"/>
<br/>

#### 구현 이유
- Google 계정을 이용한 간편 로그인을 제공하기 위해

#### 구현 방법
- Google 로그인 성공 시 SDK에서 전달받은 고유 UserID를 획득

```C#
private async void OnClick_GoogleLogin()
{
    UniversalSDK.Ins.Login(LoginType.GOOGLE)
       .OnSuccess(res =>
       {
           Debug.LogWarning("Success Google Login!");
           Debug.LogWarning($"UserID : {res.UserID}");
           Debug.LogWarning($"IdToken : {res.IdToken}");
           Debug.LogWarning($"Name : {res.Name}");
           Debug.LogWarning($"Email : {res.Email}");
           Debug.LogWarning($"ImageURL : {res.ImageURL}");
           Debug.LogWarning($"AuthCode : {res.AuthCode}");

           GameManager.Instance.AccountCode = res.UserID;
           NetworkManager.Instance.SendPacket(PacketType.GetUserData, receiveAction: EnterLobbyScene);
       })
       .OnError(err =>
       {
           UIManager.Instance.OpenSystemPopup(new MessageData { Type = PopupType.OkOnly, Message = $"Fail to Google Login. ({err.Code})" });
           Debug.LogError(err.Code);
       });
}
```
<br/>
  
- 획득한 UserID를 AccountCode로 서버에 전달하여 기존 계정 조회

```C#
public static async Task<UserData_Common> GetUserCommonData(string accountCode)
{
    return await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(uInfo => uInfo.AccountCode == accountCode).SingleOrDefaultAsync();
}
```
<br/>

- 데이터가 존재하지 않으면 신규 유저 데이터를 생성하고, 존재하면 기존 데이터를 로드

```C#
public static async Task<UserData_Common> GetUserCommonDataToConnect(string accountCode)
{
    if (await ServerDataBase.IsExistCommonData(accountCode))
    {
        UserData_Common uInfo = await CheckInvalidData(accountCode);

        await ServerDataBase.SetUserCommonData(accountCode, uInfo);
        return uInfo;
    }
    else
    {
        return await CreateUserCommonData(accountCode);
    }
}
```
<br/>

- 클라이언트에서는 Google 인증만 담당하고 실제 게임 데이터는 서버와 MongoDB에서 관리
<br/>
<br/>


### 5. AWS 라이브 서버 셋팅
![image](https://github.com/user-attachments/assets/6de974f7-44db-42d6-8e3c-092470ee9adc)
<br/>

#### 구현 이유
- 로컬 PC에서만 동작하던 ASP.NET Core 서버를 실제 Android 빌드에서도 접속할 수 있는 라이브 환경으로 구성하기 위해
- 개인 프로젝트 규모에서 필요한 성능을 확보하면서 서버 유지 비용을 최소화하기 위해
- 서버와 MongoDB를 직접 운영하여 HTTP 통신부터 DB 저장까지 전체 서버 흐름을 경험하기 위해

#### 구현 방법
- AWS EC2 인스턴스 생성
- 인스턴스 타입은 `t4g.small`, 아키텍처는 `ARM64` 선택
- ASP.NET Core 서버를 Linux ARM64 환경에 맞게 Publish
- Live 환경용 appsettings를 분리하여 MongoDB 접속 정보를 관리
- 서버 실행 후 Android 클라이언트의 Live 서버 주소를 AWS 서버 주소로 연결
- AWS 보안 그룹에서 실제 서버 통신에 필요한 포트만 허용하여 외부 접근 범위를 제한
<br/>
<br/>


### 6. 블록 타입 구현 (AI 활용)
<img src="https://github.com/user-attachments/assets/c977b0e8-aef5-4ada-a269-22c5fe00ac9b" width="30%"/>
<br/>

#### 구현 이유
- 단순히 숫자가 내려오는 방식만 반복하면 플레이가 빠르게 단조로워질 수 있기 때문에 블록별 특성을 추가
- 하나의 Block 클래스를 기반으로 여러 특성을 조합할 수 있도록 설계하여 새로운 패턴을 쉽게 확장하기 위해
- 이번 프로젝트의 개발 목표 중 하나인 AI 적극 활용 경험을 위해 블록 타입 아이디어 및 구현 과정에 AI를 적극 활용

#### 구현 방법
- 블록 특성을 `[System.Flags]` enum으로 정의
- 하나의 블록이 Rotation + Move, Armor + Move처럼 여러 타입을 동시에 가질 수 있도록 비트 플래그 방식 사용

```C#
[System.Flags]
public enum BlockType
{
    None = 0,
    Rotation = 1 << 0,  // 1
    Move = 1 << 1,  // 2
    Armor = 1 << 2,  // 4
    Ghost = 1 << 3,  // 8
}
```
<br/>

- 랜덤으로 특성 개수를 결정

```C#
private BlockType GetRandomBlockType()
{
    BlockType[] blockTypes =
        {
        BlockType.Rotation,
        BlockType.Move,
        BlockType.Armor,
        BlockType.Ghost
    };

    // 0 ~ 99 중 하나
    int randomValue = RandomUtil.GetRandomIndex(0, 99);

    // 일반 블록 : 40%
    if (randomValue < 40)
    {
        return BlockType.None;
    }

    // 특성 개수 결정
    // 40 ~ 84 : 특성 1개 = 45%
    // 85 ~ 94 : 특성 2개 = 10%
    // 95 ~ 99 : 특성 3개 = 5%
    int typeCount;

    if (randomValue < 85)
    {
        typeCount = 1;
    }
    else if (randomValue < 95)
    {
        typeCount = 2;
    }
    else
    {
        typeCount = 3;
    }

    BlockType result = BlockType.None;
    List<int> selectedIndexes = new List<int>();

    while (selectedIndexes.Count < typeCount)
    {
        int randomIndex =
            RandomUtil.GetRandomIndex(0, blockTypes.Length - 1);

        // 이미 선택한 타입이면 다시 뽑기
        if (selectedIndexes.Contains(randomIndex))
            continue;

        selectedIndexes.Add(randomIndex);

        // BlockType 추가
        result |= blockTypes[randomIndex];
    }

    return result;
}
```
<br/>

- 기본 Block 로직은 공통으로 유지하고 타입별 동작만 분리

```C#
public void Update()
{
    // 기본 하강
    MoveDown();

    // Rotation
    if ((m_BlockType & BlockType.Rotation) != 0)
    {
        UpdateRotation();
    }

    // Move
    if ((m_BlockType & BlockType.Move) != 0)
    {
        UpdateMove();
    }

    // Armor
    if ((m_BlockType & BlockType.Armor) != 0)
    {
        UpdateArmor();
    }

    // Ghost
    if ((m_BlockType & BlockType.Ghost) != 0)
    {
        UpdateGhost();
    }
}
```
<br/>

- AI를 단순 코드 생성 용도로만 사용하지 않고 Flags 기반 구조, 타입 조합 방법, 예외 상황 등을 질문한 뒤 실제 프로젝트 구조에 맞게 수정하여 적용
- 생성된 코드를 그대로 사용하는 대신 동작 원리를 확인하고 프로젝트의 기존 코드 스타일에 맞게 재구성
<br/>
<br/>


### 7. HTTP 통신 방식의 랭킹 구현
<img src="https://github.com/user-attachments/assets/cb0af2da-6a81-47bd-842a-407c4ba263b8" width="30%"/>
<br/>

#### 구현 이유
- 직접 구축한 서버와 DB만으로 랭킹 시스템을 구현하기 위해
- 별도의 랭킹 SDK 의존성 없이 게임 데이터와 동일한 서버 구조에서 관리하기 위해
- 점수 저장 및 랭킹 조회는 프레임 단위 실시간성이 필요하지 않기 때문에 HTTP 통신으로 충분하다고 판단

#### 구현 방법
- 게임 종료 후 최고 점수 갱신이 필요한 경우 서버에 점수 저장 요청
- 서버는 클라이언트가 전달한 계정을 기준으로 유저 데이터를 조회하고 점수를 저장
- 랭킹 화면 진입 시 클라이언트에서 랭킹 조회 HTTP 요청
- 서버에서는 MongoDB의 Score/Time 기준으로 내림차순 정렬하여 상위 유저 목록 반환

```C#
public static async Task<UserRankInfo> GetUserRankInfo(string accountCode)
{
    // 전체 유저 기본 정보 가져오기
    var commonList = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(Builders<UserData_Common>.Filter.Empty).ToListAsync();

    // 랭킹 계산용 리스트
    var rankList = new List<(UserData_Common CommonData, int Score, float Time)>();

    foreach (var commonData in commonList)
    {
        // 게임 데이터 가져오기
        var gameData = await GameMethod.GetUserGameData(commonData.AccountCode);

        if (gameData == null)
            continue;

        rankList.Add((commonData, gameData.Score, gameData.Time));
    }

    // Score 높은 순, Time 높은 순으로 정렬
    var sortedList = rankList.OrderByDescending(data => data.Score).ThenByDescending(data => data.Time).ToList();

    // 해당 유저 위치 검색
    int rank = sortedList.FindIndex(data => data.CommonData.AccountCode == accountCode);

    UserRankInfo rankInfo = new UserRankInfo();

    // 유저 기본 데이터
    var userCommonData = await GetUserCommonData(accountCode);
    if (userCommonData != null)
    {
        rankInfo.NickName = userCommonData.NickName;
        rankInfo.ImageNum = userCommonData.ImageNum;
    }

    // 유저의 Number 데이터
    var userNumberData = await NumberMethod.GetUserNumberData(accountCode);
    if (userNumberData != null)
    {
        rankInfo.EquipNumber = userNumberData.EquipNumber;
    }

    // 랭킹에 없는 경우
    if (rank == -1)
    {
        rankInfo.Rank = -1;

        var userGameData = await GameMethod.GetUserGameData(accountCode);
        rankInfo.Score = userGameData != null ? userGameData.Score : 0;
        rankInfo.Time = userGameData != null ? userGameData.Time : 0f;
    }
    // 랭킹에 있는 경우
    else
    {
        rankInfo.Rank = rank + 1;
        rankInfo.Score = sortedList[rank].Score;
        rankInfo.Time = sortedList[rank].Time;
    }

    return rankInfo;
}
```
<br/>

- 서버에서 받은 유저 데이터를 랭킹 데이터 형태로 가공

```C#
public class UserRankInfo
{
    public int Rank = 0;
    public string NickName = string.Empty;
    public int Score = 0;
    public float Time = 0f;
    public List<int> EquipNumber = new List<int>();
    public string ImageNum = string.Empty;
}
```
<br/>

- 클라이언트는 서버에서 받은 순위 데이터를 기반으로 랭킹 슬롯 생성

```C#
for (int i = 0; i < m_UsersRankInfo.Count; ++i)
{
    GameObject slotObj = Instantiate(m_SlotRankingObj, Trans_Content_RankingSlot);
    slotObj.GetComponent<Slot_Ranking>().SetSlot(m_UsersRankInfo[i]);
}
```
<br/>
<br/>


## 💥 트러블 슈팅

### 1. 서버 통신 방법 선택

#### 문제 상황
- 로그인, 유저 데이터 저장, 숫자 구매/장착, 랭킹 등의 서버 기능이 필요
- 이전 프로젝트에서는 Photon, 뒤끝 서버 등 외부 서비스를 사용했기 때문에 직접 서버를 구축하고 통신하는 경험이 부족했음
- 서버 통신 방식에 따라 구현 난이도와 서버 구조가 크게 달라지기 때문에 프로젝트 성격에 맞는 방식 선택이 필요

#### 해결 방안

##### HTTP
- Request/Response 구조가 단순하고 구현 및 디버깅이 쉬움
- 모바일 환경에서 안정적
- 로그인, 저장, 구매, 랭킹처럼 특정 시점에만 데이터를 주고받는 기능에 적합
- ASP.NET Core와 Unity 모두 관련 기능을 기본적으로 지원
- 프레임 단위 실시간 통신에는 적합하지 않음

##### TCP Socket
- 연결을 유지한 상태로 지속적인 양방향 통신 가능
- 실시간 게임 서버, 채팅, 전투 동기화 등에 적합
- Packet 분할/조립, 연결 유지, 재접속, Heartbeat 등 직접 관리해야 할 요소가 많음
- 현재 프로젝트는 실시간 멀티플레이가 없기 때문에 구조가 과도하게 복잡해질 수 있음

##### WebSocket
- HTTP Handshake 이후 연결을 유지하며 양방향 통신 가능
- 실시간 알림, 채팅과 같은 기능에 유리
- 현재 Number Rush의 서버 기능은 서버가 클라이언트에 지속적으로 Push해야 하는 데이터가 없음
- 유지 연결의 장점을 활용할 기능이 부족함

##### UDP
- 지연이 거의 없고 가장 빠름
- 전송 순서가 보장 안됨
- 패킷이 사라질 수 있음
- 실시간 FPS, 플레이어 위치, 물리 연산 같은 기능에 유리

#### 의견 결정
##### HTTP 통신 방식 사용
- Number Rush는 싱글 플레이 중심이며 프레임 단위 실시간 동기화가 필요하지 않음
- 로그인, 데이터 저장, 구매, 랭킹 모두 요청 시점에 결과를 받는 구조로 충분
- 구현 구조가 단순하여 서버 로직 자체에 집중할 수 있음
- ASP.NET Core 서버 구축부터 Unity 통신, AWS 배포까지 전체 과정을 직접 경험할 수 있음
<br/>
<br/>


### 2. 번들 패치를 통한 APK 용량 감소 및 빌드 자동화

#### 문제 상황
- 기존에는 게임에서 사용하는 Prefab, Image, Sound 등의 리소스가 APK에 함께 포함되는 구조
- 프로젝트에 리소스가 추가될수록 APK 자체의 용량도 함께 증가하며, 실제 Number Rush의 APK 크기가 `79.6MB`까지 증가
- 라이브 서비스 중 리소스만 수정되더라도 앱에 리소스가 포함되어 있다면 새로운 빌드를 스토어에 다시 배포해야 하는 문제가 있음
- 따라서 게임 리소스를 APK에서 분리하여 필요한 리소스를 서버에서 다운로드하는 패치 구조가 필요
- AssetBundle 적용 이후에는 리소스마다 Bundle Name을 지정하고, 플랫폼과 버전에 맞게 AssetBundle을 반복해서 빌드해야 하기 때문에 수동 작업으로 관리할 경우 실수 가능성이 있다고 판단
- APK 빌드와 AssetBundle 빌드 환경이 서로 달라지는 문제를 방지하기 위해 빌드 과정까지 하나의 툴에서 관리할 필요가 있었음

#### 해결 방안

##### AssetBundle을 이용한 게임 리소스 분리
- 기존 APK 내부에 포함되던 게임 리소스를 `ResourcesBundle` 경로로 분리
- Prefab, Image, Sound, AudioMixer 등의 리소스에 AssetBundle Name을 지정하여 별도의 Bundle 파일로 생성
- 생성된 AssetBundle은 APK 내부에 다시 포함하지 않고 오브젝트 스토리지인 `Amazon S3`에 업로드
- 클라이언트에서는 `Amazon CloudFront`를 통해 필요한 AssetBundle을 다운로드하도록 구성
- 이를 통해 실행 파일에는 필요한 코드와 최소 리소스만 포함하고 실제 게임 리소스는 패치 서버에서 관리하도록 변경

```text
기존 구조

APK
├─ Script
├─ Prefab
├─ Image
├─ Sound
└─ 기타 Game Resource


AssetBundle 적용 후

APK
├─ Script
└─ 필수 Resource

        ↓ 게임 실행

Amazon CloudFront
        ↓
Amazon S3
├─ PatchList.pat
├─ Prefab AssetBundle
├─ Image AssetBundle
├─ Sound AssetBundle
└─ 기타 AssetBundle
```
<br/>

##### BuildWindow를 이용한 빌드 과정 자동화
<img src="https://github.com/user-attachments/assets/9f95c245-ef10-4fd1-b89b-1eb6c96b986c" width="30%"/>
<br/>

- APK와 AssetBundle을 각각 수동으로 설정하고 빌드하는 과정에서 발생할 수 있는 설정 실수를 줄이기 위해 Unity EditorWindow 기반 `BuildWindow` 제작
- 하나의 BuildWindow에서 다음 작업을 처리할 수 있도록 구성

```text
BuildWindow

Build Setting
├─ Server Type 설정
├─ Development Build 설정
├─ Build Version 설정
├─ Bundle Code 설정
├─ Build Path 설정
└─ APK Build

Patch Setting
├─ Bundle Name 지정
├─ Bundle Name 제거
└─ AssetBundle Build
```
<br/>

##### AssetBundle Name 자동 지정
- AssetBundle을 직접 관리하려면 리소스마다 올바른 Bundle Name이 지정되어 있어야 함
- 리소스가 증가할수록 Inspector에서 Bundle Name을 직접 지정하는 것은 반복 작업이 많고 누락 가능성이 있음
- BuildWindow의 `Patch Setting - Bundle Name - Save`를 실행하면 기존 Bundle Name을 정리하고 `ResourcesBundle` 아래의 리소스를 기준으로 Bundle Name을 자동 지정하도록 구성

```C#
private void SignBundleName()
{
    string[] allAssetName = AssetDatabase.GetAllAssetBundleNames();

    for (int count = 0; count < allAssetName.Length; ++count)
    {
        AssetDatabase.RemoveAssetBundleName(allAssetName[count], true);
    }

    BundleBuilder.AssignAssetBundleName("Assets/ResourcesBundle");
    ShowNotification(new GUIContent("Set BundleName"));
}
```
<br/>

- `Builder`에서는 프로젝트의 리소스 구조와 예외 설정을 기준으로 실제 AssetBundle 대상 리소스를 검색
- 검색된 리소스의 경로와 Bundle Name을 비교하여 필요한 경우 자동으로 Bundle Name을 설정하고 다시 Import

```C#
public void AssignAssetBundleName(string resourceRootPath)
{
    string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
    List<string> unnecessaryAssetbundleNameList = allAssetBundleNames.ToList();
    List<AssetExceptionInfo> exceptionInfoList = GetExceptionInfo();
    List<AssetInfo> assetInfoList = GetAssetInfoList(resourceRootPath, true, exceptionInfoList);

    foreach (AssetInfo assetInfo in assetInfoList)
    {
        string resourcePath = assetInfo.Path;
        string assetBundleName = assetInfo.Name;

        AssetImporter assetImporter = AssetImporter.GetAtPath(resourcePath);

        if (!assetImporter.assetBundleName.Equals(assetBundleName))
        {
            assetImporter.assetBundleName = assetBundleName;
            assetImporter.SaveAndReimport();
        }

        unnecessaryAssetbundleNameList.Remove(assetBundleName);
    }

    foreach (string unnecessaryAssetBundleName in unnecessaryAssetbundleNameList)
    {
        AssetDatabase.RemoveAssetBundleName(unnecessaryAssetBundleName, true);
    }
}
```
<br/>

##### BuildWindow를 이용한 AssetBundle 빌드
- Bundle Name 설정 이후 BuildWindow의 `Patch Setting - Build - Run`을 통해 AssetBundle을 생성하도록 구성
- 빌드 경로가 지정되어 있는지 확인하고 현재 Build Target이 Android인지 검증한 후 Bundle 빌드를 실행
- 잘못된 플랫폼으로 AssetBundle을 생성하는 실수를 방지할 수 있도록 Build Target 검증 로직 추가

```C#
private void BuildBundle()
{
    if (string.IsNullOrEmpty(OutputPath))
    {
        Debug.LogError("[AssetBundle] Build Path가 설정되지 않았습니다.");
        return;
    }

    if (EditorUserBuildSettings.activeBuildTarget
        != BuildTarget.Android)
    {
        Debug.LogError($"[AssetBundle] 현재 Build Target이 Android가 아닙니다. " +
            $"현재 Target : {EditorUserBuildSettings.activeBuildTarget}");

        return;
    }

    EditorPrefs.SetString(KEY_BUILDPATH, OutputPath);

    string bundleOutputPath = MakeBundleOutPut();
    BundleBuilder.MakeAllBundles("Assets/ResourcesBundle", bundleOutputPath, EditorUserBuildSettings.activeBuildTarget);

    if (Directory.Exists(bundleOutputPath))
    {
        EditorUtility.RevealInFinder(bundleOutputPath);
    }
}
```
<br/>

##### 버전 및 플랫폼별 AssetBundle 관리
- 서로 다른 게임 버전의 AssetBundle이 같은 위치에 생성되지 않도록 `ProductVersion`과 현재 Build Target을 기준으로 출력 경로를 자동 생성
- 이를 통해 APK 버전과 패치 리소스의 버전을 구분하여 관리할 수 있도록 구성

```C#
private string MakeBundleOutPut()
{
    return $"{OutputPath}/AssetBundle/" + $"{ProductVersion}/" + $"{EditorUserBuildSettings.activeBuildTarget}";
}
```
<br/>

```text
AssetBundle
├─ 1.0.1
│  └─ Android
│     ├─ PatchList.pat
│     └─ AssetBundles...
│
├─ 1.0.2
│  └─ Android
│     ├─ PatchList.pat
│     └─ AssetBundles...
│
└─ 1.0.3
   └─ Android
      ├─ PatchList.pat
      └─ AssetBundles...
```
<br/>

- 최종적으로 BuildWindow에서 아래 순서로 APK 및 AssetBundle 빌드를 관리

```text
1. Build Setting
   └─ Server / Version / Bundle Code / Build Path 설정

2. Patch Setting
   └─ Bundle Name - Save

3. ResourcesBundle 리소스의 Bundle Name 자동 지정

4. Patch Setting
   └─ Build - Run

5. 버전 / 플랫폼별 AssetBundle 생성

6. AssetBundle + PatchList.pat
   └─ Amazon S3 업로드

7. Build - Run
   └─ Bundle 모드가 적용된 APK 생성

8. 게임 실행
   └─ CloudFront를 통해 필요한 AssetBundle 다운로드
```
<br/>

#### 실제 APK 용량 감소
 <img src="https://github.com/user-attachments/assets/1d9f8fd8-9c99-4a04-8f9d-59f2f2b3a202" width="30%"/>
 <img src="https://github.com/user-attachments/assets/446382f7-c010-4125-86e1-69c9d0c59792" width="30%"/>
 <br/>

- AssetBundle 적용 전에는 게임 리소스가 APK에 함께 포함되어 `79.6MB`의 파일이 생성
- 리소스를 AssetBundle로 분리하고 외부 패치 서버에서 다운로드하도록 변경한 이후 APK 크기가 `55.1MB`로 감소
- 결과적으로 약 `24.5MB`의 APK 용량을 줄였으며 기존 크기 대비 약 `30.8%` 감소

```text
AssetBundle 적용 전 : 79.6 MB
AssetBundle 적용 후 : 55.1 MB

감소 용량 : 24.5 MB
감소 비율 : 약 30.8%
```
<br/>

#### 의견 결정

##### AssetBundle 패치 + BuildWindow 사용
- 게임 리소스를 APK에서 AssetBundle로 분리하여 APK 파일 크기를 `79.6MB → 55.1MB`로 감소
- 리소스 분리만으로 약 `24.5MB`, 약 `30.8%`의 APK 용량 감소 효과를 직접 확인
- APK에 모든 리소스를 포함하는 대신 AssetBundle을 `Amazon S3`에 저장하고 `Amazon CloudFront`를 통해 다운로드하는 패치 구조로 변경
- 리소스만 수정된 경우 앱 전체를 다시 배포하지 않고 변경된 AssetBundle을 교체할 수 있는 기반을 구성
- AssetBundle 적용으로 추가된 반복적인 빌드 작업을 줄이기 위해 직접 `BuildWindow`를 제작
- BuildWindow에서 **APK 빌드, Server Type 및 버전 설정, Bundle Name 자동 지정/제거, AssetBundle 빌드**를 한 곳에서 처리하도록 구성
- AssetBundle 출력 경로를 게임 버전과 플랫폼 기준으로 자동 분리하여 패치 파일의 버전 관리 과정도 단순화
- 결과적으로 단순히 AssetBundle을 적용하는 것에서 끝나지 않고 **리소스 분리 → Bundle Name 자동 설정 → AssetBundle 빌드 → S3 업로드 → CloudFront 배포 → 클라이언트 패치 → APK 용량 감소**까지 실제 라이브 패치에 필요한 전체 흐름을 구현
<br/>
<br/>


### 3. DB 선택
![image](https://github.com/user-attachments/assets/36831194-d2f7-4bdd-afa3-276e5e189fe8)
<br/>

#### 문제 상황
- 서버에서 유저 기본 정보, 게임 데이터, 보유 숫자, 장착 숫자, 점수 등을 영구 저장할 DB 필요
- 개인 프로젝트이지만 실제 라이브 서버처럼 클라이언트가 아닌 서버에서 데이터를 관리하고 싶었음
- 데이터 구조가 개발 과정에서 자주 변경될 가능성이 있어 초기 설계 부담이 적은 DB가 필요

#### 해결 방안

##### MySQL / PostgreSQL
- 관계형 데이터베이스이기 때문에 데이터 관계와 무결성을 명확하게 관리 가능
- JOIN, Transaction, 복잡한 통계 쿼리에 강함
- 테이블 Schema를 명확하게 정의해야 하며 데이터 구조 변경 시 Migration 관리가 필요
- 현재 프로젝트의 유저 데이터는 계정별 Document 단위로 저장하기 쉬운 구조이며 복잡한 관계형 쿼리가 많지 않음

##### SQLite
- 별도의 DB 서버 없이 가볍게 사용할 수 있음
- 로컬 게임이나 소규모 도구에서는 간단하게 사용 가능
- 서버가 여러 인스턴스로 확장되는 환경이나 다수의 동시 접근을 처리하는 라이브 DB 용도로는 현재 목적과 맞지 않음

##### Redis
- 메모리 기반으로 매우 빠른 읽기/쓰기가 가능
- Cache, Session, Ranking 등에 유리
- 이번 프로젝트에서는 유저 데이터를 영구 저장할 메인 DB가 필요
- 랭킹 하나만을 위해 Redis를 추가하면 개인 프로젝트 규모에서는 관리 대상만 증가한다고 판단

##### MongoDB
- JSON과 유사한 Document 구조로 C# 객체와 데이터 형태가 직관적으로 대응
- Schema가 비교적 유연하여 개발 중 필드 추가/변경이 쉬움
- AccountCode를 기준으로 유저 단위 데이터를 조회하는 현재 구조와 잘 맞음
- MongoDB.Driver를 이용해 C#에서 Lambda 기반으로 간단하게 Query 작성 가능

#### 의견 결정
##### MongoDB 사용
- Number Rush의 데이터는 강한 관계형 구조보다 `유저 1명 = 여러 게임 데이터 Document` 형태
- 개발 과정에서 Gold, Score, ImageNum, EquipNumber 등 필드가 계속 추가되었기 때문에 유연한 Document DB가 유리
- C# Model과 MongoDB Document를 유사한 형태로 관리할 수 있어 개발 속도가 빠름
- 현재 규모에서는 충분한 성능을 확보할 수 있다고 판단
<br/>
<br/>


### 4. AWS EC2 인스턴스 선택

#### 문제 상황
- 로컬에서 개발한 ASP.NET Core + MongoDB 서버를 실제 Android 클라이언트가 접속할 수 있는 환경으로 배포해야 함
- 개인 포트폴리오 프로젝트이기 때문에 높은 서버 비용은 부담
- 반대로 너무 낮은 사양을 선택하면 .NET 서버와 DB를 함께 실행할 때 메모리 부족이나 성능 저하 가능성이 있음

#### 해결 방안

##### t3.micro / t4g.micro
- 비용이 저렴하고 작은 테스트 서버에 적합
- 메모리가 작아 ASP.NET Core와 MongoDB를 함께 운영할 경우 여유 메모리가 부족할 가능성이 있음
- 단순 테스트에는 충분하지만 라이브 환경을 구성하는 목적에서는 여유가 적다고 판단

##### t3.small
- 2 vCPU / 2 GiB 메모리로 개인 프로젝트 서버에 충분한 수준
- x86_64 아키텍처이기 때문에 호환성이 높음
- 동일한 목적에서 ARM 기반 Graviton 인스턴스보다 비용 효율이 떨어질 수 있음

##### t4g.small
- 2 vCPU / 2 GiB 메모리
- AWS Graviton 기반 ARM64 인스턴스
- ASP.NET Core가 Linux ARM64를 지원하기 때문에 현재 서버 실행에 문제 없음
- 개인 프로젝트에서 필요한 성능을 확보하면서 비용 효율을 높일 수 있음

#### 의견 결정
##### `t4g.small / ARM64` 선택
- 서버와 MongoDB를 함께 실행하기 위해 micro보다 메모리 여유가 있는 small 선택
- 실시간 대규모 전투 서버가 아니기 때문에 2 vCPU / 2 GiB 수준이면 현재 트래픽에 충분하다고 판단
- .NET이 ARM64 Publish를 지원하므로 x86 인스턴스를 고집할 이유가 적음
- Graviton 기반 인스턴스를 사용하여 비용 대비 성능을 확보
<br/>
<br/>


### 5. Google 로그인 방식 선택

#### 문제 상황
- Google 계정을 이용해 유저를 식별하고 서버의 AccountCode로 사용할 고유 ID가 필요
- Google 로그인 구현 방법으로 GPGS, Firebase Authentication, Universal SDK를 비교

#### 해결 방안

##### Google Play Games Services (GPGS)
- Google Play Games와 직접 연동할 수 있음
- 로그인 외에도 업적, 리더보드, Saved Games 등 Google Play Games 기능을 사용할 수 있음
- Google Cloud, Play Console 설정과 OAuth Client 설정 등 초기 구성이 필요
- 현재 프로젝트는 GPGS의 게임 기능보다 Google 계정 식별만 필요

##### Firebase Authentication
- Google뿐 아니라 Apple, Email, 익명 로그인 등 다양한 인증 방식을 통합 관리하기 좋음
- Firebase의 다른 서비스와 연결하기 쉬움
- 게스트 계정 연동, 여러 Provider 통합 같은 확장에는 유리
- 현재 프로젝트는 Google 로그인 하나만 필요하기 때문에 Firebase 인증 구조 전체를 추가하는 것은 기능 대비 복잡도가 증가
- 게임 데이터는 이미 직접 구축한 ASP.NET Core + MongoDB 서버에서 관리하므로 Firebase DB 기능도 필요하지 않음

##### Universal SDK
- 현재 필요한 Google 로그인 기능을 비교적 간단하게 구현 가능
- SDK에서 로그인 후 전달받은 Google 고유 ID를 자체 서버의 AccountCode로 연결 가능
- 기존 서버/DB 구조를 변경하지 않고 인증 단계만 추가할 수 있음
- 추후 필요하다면 Firebase를 추가하여 게스트 계정 연동이나 다른 Provider 로그인 구조로 확장 가능

#### 의견 결정
##### Universal SDK 사용
- 현재 목표는 Google 로그인 성공 후 고유 ID를 획득하여 자체 서버 계정과 연결하는 것
- 게임 데이터와 계정 데이터의 실질적인 관리는 자체 서버에서 담당하므로 Firebase 의존성이 필수적이지 않음
- GPGS의 업적/리더보드 기능도 현재 프로젝트에서 사용하지 않음
- 필요한 기능만 빠르게 구현하면서 기존 HTTP 서버 구조를 유지할 수 있는 Universal SDK가 가장 적합하다고 판단
- 먼저 Universal SDK 기반으로 구현하고, 추후 게스트 계정 연동이나 다중 로그인 Provider가 필요해질 경우 Firebase를 추가할 수 있도록 구조를 분리
<br/>
<br/>


## 📋 프로젝트 회고
이번 프로젝트의 가장 큰 목표는 이전 프로젝트에서 사용하지 않았던 **HTTP 서버 통신과 직접적인 라이브 서버 구축 경험**을 얻는 것이었습니다. 이전에는 Photon이나 뒤끝 서버와 같은 외부 서비스를 활용하여 기능 구현에 집중했다면, Number Rush에서는 Unity 클라이언트부터 ASP.NET Core 서버, MongoDB, AWS 배포까지 하나의 흐름을 직접 구성했습니다.

특히 클라이언트가 요청한 값을 그대로 저장하는 것이 아니라 서버에서 데이터를 다시 조회하고 검증한 뒤 결과를 반환하는 구조를 구현하면서, 단순히 통신이 되는 코드와 실제 서비스에서 사용할 수 있는 서버 구조의 차이를 경험할 수 있었습니다. 또한 AWS EC2 인스턴스와 CPU 아키텍처, Test/Live 환경 분리처럼 게임 로직 외에도 서버 운영에 필요한 요소들을 직접 다뤄볼 수 있었습니다.

Google 로그인 역시 단순히 SDK를 적용하는 것보다 GPGS, Firebase, Universal SDK의 역할을 비교하고 현재 프로젝트에 필요한 범위를 기준으로 Universal SDK를 선택했습니다. 이를 통해 기술을 많이 사용하는 것보다 프로젝트의 요구사항에 맞는 기술을 선택하는 과정이 중요하다는 점을 다시 확인했습니다.

또한 블록 타입 구현 과정에서는 AI를 적극 활용했습니다. AI가 제안한 코드를 그대로 적용하는 방식이 아니라, 기존 프로젝트 구조에 맞게 수정하면서 개발 보조 도구로 활용했습니다.

이번 프로젝트를 통해 **Unity 클라이언트 → HTTP 통신 → ASP.NET Core 서버 → MongoDB → AWS 라이브 환경**으로 이어지는 전체 구조를 직접 구축했다는 점이 가장 큰 성과였습니다. 추후에는 에셋 번들 적용, 파이어 베이스 적용, 운영툴 제작 등 실제 서비스 운영에 가까운 인프라 구조까지 확장해보고 싶습니다.
