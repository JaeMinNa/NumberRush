using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Messaging;
using UnityEngine;
using UnityEngine.Android;

public class FirebaseManager : Singleton<FirebaseManager>
{
    private FirebaseApp m_FirebaseApp;
    private bool m_IsInitialized = false;

    public bool IsInitialized => m_IsInitialized;

    public static FirebaseManager Instance
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

                GameObject managerObj = GameObject.Find("[Managers]/FirebaseManager");

                if (managerObj == null)
                {
                    managerObj = new GameObject("FirebaseManager");
                    managerObj.transform.SetParent(obj.transform);
                }

                m_Instance = managerObj.GetComponent<FirebaseManager>();

                if (m_Instance == null)
                {
                    m_Instance = managerObj.AddComponent<FirebaseManager>();
                }

                m_Instance.CreateInstance();
            }

            return m_Instance;
        }
    }

    #region Override Method
    public override void DestroyInstance()
    {
        // Firebase Messaging 이벤트 해제
        FirebaseMessaging.RegistrationReceived -= OnRegistrationReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;

        m_IsInitialized = false;
        m_FirebaseApp = null;
    }

    protected override void CreateInstance()
    {
        InitializeFirebase();
    }

    #endregion

    #region Firebase Initialize
    // Firebase 초기화
    private void InitializeFirebase()
    {
        // 중복 초기화 방지
        if (m_IsInitialized)
        {
            return;
        }

        Debug.LogWarning("[Firebase] 초기화 시작");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("[Firebase] 초기화 취소");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"[Firebase] 초기화 실패 : {task.Exception}");
                return;
            }

            DependencyStatus dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                m_FirebaseApp = FirebaseApp.DefaultInstance;
                m_IsInitialized = true;

                Debug.LogWarning("[Firebase] 초기화 성공");

                // Firebase Analytics 활성화
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                // Firebase Messaging 초기화
                InitializeFirebaseMessaging();

                // 앱 실행 이벤트
                LogEvent("GameStart");
            }
            else
            {
                Debug.LogError($"[Firebase] Firebase 의존성 오류 : {dependencyStatus}");
            }
        });
    }

    // Firebase Messaging 초기화
    private void InitializeFirebaseMessaging()
    {
        // FCM 등록 이벤트
        FirebaseMessaging.RegistrationReceived += OnRegistrationReceived;

        // FCM 메시지 수신 이벤트
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        // Android 알림 권한 요청
        RequestNotificationPermission();

        Debug.LogWarning("[Firebase Messaging] 초기화 완료");
    }
    #endregion

    #region Analytics
    // Event 이름으로만 호출
    public void LogEvent(string eventName)
    {
        if (!CheckInitialized())
        {
            return;
        }

        FirebaseAnalytics.LogEvent(eventName);

        Debug.LogWarning($"[Firebase Analytics] Event : {eventName}");
    }

    // string 파라미터 이벤트 전송
    public void LogEvent(string eventName, string parameterName, string parameterValue)
    {
        if (!CheckInitialized())
        {
            return;
        }

        FirebaseAnalytics.LogEvent(eventName, new Parameter(parameterName, parameterValue));

        Debug.LogWarning($"[Firebase Analytics] Event : {eventName} / " + $"{parameterName} : {parameterValue}");
    }


    // 정수 파라미터 이벤트 전송
    public void LogEvent(string eventName, string parameterName, long parameterValue)
    {
        if (!CheckInitialized())
        {
            return;
        }

        FirebaseAnalytics.LogEvent(eventName, new Parameter(parameterName, parameterValue));

        Debug.LogWarning($"[Firebase Analytics] Event : {eventName} / " + $"{parameterName} : {parameterValue}");
    }


    // 실수 파라미터 이벤트 전송
    public void LogEvent(string eventName, string parameterName, double parameterValue)
    {
        if (!CheckInitialized())
        {
            return;
        }

        FirebaseAnalytics.LogEvent(eventName, new Parameter(parameterName, parameterValue));

        Debug.LogWarning($"[Firebase Analytics] Event : {eventName} / " + $"{parameterName} : {parameterValue}");
    }

    // Firebase Analytics UserId 설정 (로그인 후, 최초 1회만 호출)
    public void SetUserId(string userId)
    {
        if (!CheckInitialized())
        {
            return;
        }

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[Firebase Analytics] User ID가 비어있습니다.");
            return;
        }

        FirebaseAnalytics.SetUserId(userId);

        Debug.LogWarning($"[Firebase Analytics] User ID 설정 : {userId}");
    }


    // User 속성 설정
    public void SetUserProperty(string propertyName, string propertyValue)
    {
        if (!CheckInitialized())
        {
            return;
        }

        FirebaseAnalytics.SetUserProperty(propertyName, propertyValue);

        Debug.LogWarning($"[Firebase Analytics] User Property : " + $"{propertyName} = {propertyValue}");
    }
    #endregion

    #region Messaging
    // FCM 등록 완료
    private void OnRegistrationReceived(object sender, RegistrationReceivedEventArgs e)
    {
        Debug.LogWarning($"[Firebase Messaging] Registration 성공 / Installation ID : {e.InstallationId}");

        // 추후 필요하면 이 Installation ID를 게임 서버에 전달
    }


    // FCM 메시지 수신
    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        FirebaseMessage message = e.Message;

        Debug.LogWarning("[Firebase Messaging] Push 메시지 수신");

        if (!string.IsNullOrEmpty(message.From))
        {
            Debug.LogWarning($"[Firebase Messaging] From : {message.From}");
        }

        if (message.Notification != null)
        {
            Debug.LogWarning($"[Firebase Messaging] Title : {message.Notification.Title}");
            Debug.LogWarning($"[Firebase Messaging] Body : {message.Notification.Body}");
        }

        if (message.Data != null)
        {
            foreach (var data in message.Data)
            {
                Debug.LogWarning($"[Firebase Messaging] Data : {data.Key} = {data.Value}");
            }
        }
    }

    // Android 알림 권한 요청
    private void RequestNotificationPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

    if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
    {
        Debug.LogWarning("[Firebase Messaging] 알림 권한이 이미 허용되어 있습니다.");
        return;
    }

    Debug.LogWarning("[Firebase Messaging] 알림 권한 요청");

    Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");

#endif
    }
    #endregion

    #region Private Method

    private bool CheckInitialized()
    {
        if (!m_IsInitialized)
        {
            Debug.LogError("[Firebase] Firebase가 아직 초기화되지 않았습니다.");

            return false;
        }

        return true;
    }

    #endregion
}