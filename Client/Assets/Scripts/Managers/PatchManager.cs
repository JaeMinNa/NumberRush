using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Linq;

public class PatchManager : MonoBehaviour
{
    private static PatchManager m_Instance = null;
    public static PatchManager Instance
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

                GameObject ManagerObj = GameObject.Find("[Managers]/PatchManager");
                if (ManagerObj == null)
                {
                    ManagerObj = new GameObject("PatchManager");
                    ManagerObj.transform.SetParent(obj.transform);
                }

                m_Instance = ManagerObj.GetComponent<PatchManager>();
                if (m_Instance == null)
                {
                    m_Instance = ManagerObj.AddComponent<PatchManager>();
                }

                m_Instance.Init();
            }

            return m_Instance;
        }
    }

    public PatchList PatchList
    {
        get
        {
            return m_PatchList;
        }
    }

    #region Member Property
    private PatchList m_PatchList = null;
    private Dictionary<string, PatchData> m_NewList = null;
    private Dictionary<string, PatchData> m_UpdateList = null;
    private Dictionary<string, PatchData> m_RemoveList = null;

    private List<string> DownloadingList = null;
    private List<string> CompleteList = null;

    private bool IsPatchCancel = false;
    private bool IsPatchCompleting = false;
    private const int ThreadCount = 3;
    private bool[] ThreadStaus = null;
    private float[] ThreadProgress = null;
    private ulong[] ThreadDownloadedBytes = null;
    private long TotalDownloadBytes = 0;
    private long CompleteDownloadBytes = 0;

    public static UnityAction DownLoadStartDelegate;
    public static UnityAction PatchCompleteDelegate;
    public static UnityAction<float> DownloadProgressDelegate;

    // 패치 진행 상황
    // progress     : 0 ~ 1
    // completeFile : 다운로드 완료 파일 수
    // totalFile    : 전체 파일 수
    // downloadByte : 현재까지 다운로드 받은 용량
    // totalByte    : 전체 다운로드 용량
    public static Action<float, int, int, long, long> PatchProgressDelegate;
    #endregion

    #region Unity Method
    public void OnApplicationQuit()
    {
        IsPatchCancel = true;
    }
    #endregion

    private void Init()
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "AssetBundle")))
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "AssetBundle"));

#if UNITY_IOS
        UnityEngine.iOS.Device.SetNoBackupFlag(Path.Combine(Application.persistentDataPath, "AssetBundle"));
#endif

#if !UNITY_WEBGL
        // WEBGL에서는 Caching API를 사용할 수 없음
        Caching.compressionEnabled = false;
#endif

        m_PatchList = new PatchList();

        m_NewList = new Dictionary<string, PatchData>();
        m_UpdateList = new Dictionary<string, PatchData>();
        m_RemoveList = new Dictionary<string, PatchData>();

        DownloadingList = new List<string>();
        CompleteList = new List<string>();

        ThreadStaus = new bool[ThreadCount];
        ThreadProgress = new float[ThreadCount];
        ThreadDownloadedBytes = new ulong[ThreadCount];
    }

    private async UniTask GetPatchList()
    {
        string patchAddress = $"{GameManager.Instance.GameConfig.PatchAdress()}/PatchList.pat";

        Debug.LogWarning($"[Patch] PatchList 주소 : {patchAddress}");

        using (UnityWebRequest www = UnityWebRequest.Get(patchAddress))
        {
            await www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[Patch] 서버 PatchList 다운로드 성공");

                m_PatchList.LoadLocalPatchList();
                m_NewList = PatchList.MakePatchList(www.downloadHandler.text);
                m_UpdateList = m_PatchList.GetUpdateList(m_NewList);
                m_RemoveList = m_PatchList.GetRemoveList(m_NewList);

                Debug.LogWarning($"[Patch] 서버 파일 : {m_NewList.Count}");
                Debug.LogWarning($"[Patch] 업데이트 파일 : {m_UpdateList.Count}");
                Debug.LogWarning($"[Patch] 삭제 파일 : {m_RemoveList.Count}");
            }
            else
            {
                Debug.LogError(
                    $"[Patch] PatchList 다운로드 실패\n" +
                    $"URL : {patchAddress}\n" +
                    $"Error : {www.error}"
                );
            }
        }
    }

    public async UniTask<bool> CheckPatch()
    {
        await GetPatchList();

        if (m_UpdateList == null || m_UpdateList.Count <= 0)
        {
            Debug.LogWarning("[Patch] 업데이트할 파일이 없습니다.");

            PatchComplete();

            return true;
        }

        long fileSize = 0;

        foreach (KeyValuePair<string, PatchData> pair in m_UpdateList)
        {
            fileSize += pair.Value.size;

            Debug.LogWarning($"[Patch] 업데이트 필요 : {pair.Value.name} / {pair.Value.size} bytes");
        }

        CompleteDownloadBytes = 0;
        TotalDownloadBytes = fileSize;
        float fileSizeMB = fileSize / 1024f / 1024f;

        Debug.LogWarning($"[Patch] 업데이트 파일 개수 : {m_UpdateList.Count}");
        Debug.LogWarning($"[Patch] 총 다운로드 크기 : {fileSizeMB:F2} MB");

        UIManager.Instance.OpenSystemPopup(new MessageData 
        { 
            Type = PopupType.OkOnly, 
            Title = "PATCH",
            Message = $"A total patch size of {fileSizeMB:F2} MB is required.",
            OkAction = () => 
            {
                // 패치 팝업
                UIManager.Instance.Open<Popup_Patch>(UI.Popup, "Prefabs/UI/Popup/Popup_Patch", IsBundle:false);

                DownloadPatch();
            }
        });

        return false;
    }

    public void DownloadPatch()
    {
        IsPatchCancel = false;
        IsPatchCompleting = false;

        for (int count = 0; count < ThreadCount; ++count)
        {
            ThreadStaus[count] = false;
            ThreadProgress[count] = 0f;
            ThreadDownloadedBytes[count] = 0;
        }

        for (int count = 0; count < ThreadCount; ++count)
        {
            DownloadThreadAsync(count);
        }
    }

    private void MakeDownloadThread(int Index)
    {
        StartCoroutine(DownloadThread(Index));
    }

    private IEnumerator DownloadThread(int ThreadIndex)
    {
        ThreadStaus[ThreadIndex] = false;
        ThreadProgress[ThreadIndex] = 0f;

        foreach (KeyValuePair<string, PatchData> pair in m_UpdateList)
        {
            if (DownloadingList.Contains(pair.Key))
                continue;

            DownloadingList.Add(pair.Key);

            string PatchAdress = $"{GameManager.Instance.GameConfig.PatchAdress()}/{pair.Value.name}";
            string FilePath = Path.Combine(Application.persistentDataPath, "AssetBundle", pair.Value.name);

            Debug.LogWarning(PatchAdress);

            // 그외 에셋번들
            using (var wwwDownload = UnityWebRequest.Get(PatchAdress))
            using (var downloadHandler = new DownloadHandlerBuffer())
            {
                wwwDownload.downloadHandler = downloadHandler;
                wwwDownload.SendWebRequest();

                while (true)
                {
                    yield return null;

                    if (IsPatchCancel)
                        yield break;

                    if (wwwDownload.result == UnityWebRequest.Result.ConnectionError ||
                        wwwDownload.result == UnityWebRequest.Result.ProtocolError ||
                        wwwDownload.result == UnityWebRequest.Result.DataProcessingError)
                    {
                        Debug.LogError($"Download Error : {wwwDownload.error}");

                        // 에러 발생
                        DownloadingList.Remove(pair.Key);
                        ThreadProgress[ThreadIndex] = 0f;

                        MakeDownloadThread(ThreadIndex);

                        downloadHandler.Dispose();
                        wwwDownload.Dispose();

                        yield break;
                    }
                    else if (wwwDownload.isDone)
                    {
                        // 다운로드 끝
                        CompleteList.Add(pair.Key);

                        ThreadProgress[ThreadIndex] = 0f;
                        ThreadDownloadedBytes[ThreadIndex] = 0;

                        SetProgress();

                        if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                        byte[] dataCache = wwwDownload.downloadHandler.data;

                        downloadHandler.Dispose();
                        wwwDownload.Dispose();

                        File.WriteAllBytes(FilePath, dataCache);
                        dataCache = null;

                        yield return null;
                        break;
                    }
                    else
                    {
                        ThreadProgress[ThreadIndex] = wwwDownload.downloadProgress;
                        SetProgress();
                    }
                }
            }
        }

        // 다운로드 종료
        bool IsAllDownload = true;
        ThreadStaus[ThreadIndex] = true;

        for (int count = 0; count < ThreadCount; ++count)
        {
            if (ThreadStaus[count] == false)
                IsAllDownload = false;
        }

        if (m_UpdateList.Count != DownloadingList.Count)
            IsAllDownload = false;

        if (IsAllDownload)
        {
            DownloadProgressDelegate?.Invoke(100f);

            yield return Resources.UnloadUnusedAssets();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            yield return null;

            yield return new WaitForSeconds(1.5f);

            // 다운로드 완료
            PatchComplete();
        }
    }

    private async void DownloadThreadAsync(int ThreadIndex)
    {
        ThreadStaus[ThreadIndex] = false;
        ThreadProgress[ThreadIndex] = 0f;
        ThreadDownloadedBytes[ThreadIndex] = 0;

        foreach (KeyValuePair<string, PatchData> pair in m_UpdateList)
        {
            if (DownloadingList.Contains(pair.Key))
                continue;

            DownloadingList.Add(pair.Key);

            string PatchAdress = $"{GameManager.Instance.GameConfig.PatchAdress()}/{pair.Value.name}";
            string FilePath = Path.Combine(Application.persistentDataPath, "AssetBundle", pair.Value.name);

            Debug.LogWarning(PatchAdress);

            // 그외 에셋번들
            using (var wwwDownload = UnityWebRequest.Get(PatchAdress))
            using (var downloadHandler = new DownloadHandlerBuffer())
            {
                wwwDownload.downloadHandler = downloadHandler;
                _ = wwwDownload.SendWebRequest();

                while (true)
                {
                    await UniTask.Yield();

                    if (IsPatchCancel)
                        return;

                    if (wwwDownload.result == UnityWebRequest.Result.ConnectionError ||
                        wwwDownload.result == UnityWebRequest.Result.ProtocolError ||
                        wwwDownload.result == UnityWebRequest.Result.DataProcessingError)
                    {
                        Debug.LogWarning($"Download Error : {wwwDownload.error}");

                        // 에러 발생
                        DownloadingList.Remove(pair.Key);
                        ThreadProgress[ThreadIndex] = 0f;

                        downloadHandler.Dispose();
                        wwwDownload.Dispose();

                        DownloadThreadAsync(ThreadIndex);

                        return;
                    }
                    else if (wwwDownload.isDone)
                    {
                        // 다운로드 끝
                        CompleteList.Add(pair.Key);

                        // 완료된 파일 용량 누적
                        CompleteDownloadBytes += pair.Value.size;

                        // 현재 스레드 진행량 초기화
                        ThreadProgress[ThreadIndex] = 0f;
                        ThreadDownloadedBytes[ThreadIndex] = 0;

                        SetProgress();

                        if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                        byte[] dataCache = wwwDownload.downloadHandler.data;

                        downloadHandler.Dispose();
                        wwwDownload.Dispose();

                        File.WriteAllBytes(FilePath, dataCache);

                        var bundle = await AssetBundle.LoadFromMemoryAsync(dataCache);
                        if (bundle is not null)
                            await bundle.UnloadAsync(true);

                        dataCache = null;

                        break;
                    }
                    else
                    {
                        ThreadProgress[ThreadIndex] = wwwDownload.downloadProgress;

                        // 현재 이 스레드에서 다운로드 받은 실제 Byte
                        ThreadDownloadedBytes[ThreadIndex] = wwwDownload.downloadedBytes;

                        SetProgress();
                    }
                }
            }
        }

        // 다운로드 종료
        ThreadStaus[ThreadIndex] = true;

        // 다른 다운로드 스레드가 아직 작업 중인지 확인
        for (int count = 0; count < ThreadCount; ++count)
        {
            if (ThreadStaus[count] == false)
                return;
        }

        // 모든 파일이 다운로드 목록에 들어갔는지 확인
        if (m_UpdateList == null ||
            DownloadingList == null ||
            m_UpdateList.Count != DownloadingList.Count)
        {
            return;
        }

        // 이미 다른 스레드가 패치 완료 처리를 시작했다면 종료
        if (IsPatchCompleting)
            return;

        // 여기까지 도달한 단 하나의 스레드만 완료 처리
        IsPatchCompleting = true;

        DownloadProgressDelegate?.Invoke(100f);

        PatchProgressDelegate?.Invoke(
            1f,
            m_UpdateList.Count,
            m_UpdateList.Count,
            TotalDownloadBytes,
            TotalDownloadBytes
        );

        await UniTask.Delay(500);

        await Resources.UnloadUnusedAssets();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        await UniTask.Delay(500);

        // 패치 완료
        PatchComplete();
    }

    private async void PatchComplete()
    {
        m_PatchList.UpdateData(m_NewList);
        m_PatchList.WriteLocalPatchList();

        if (m_RemoveList != null && m_RemoveList.Count > 0)
        {
            foreach (var Patch in m_RemoveList)
            {
                string FilePath = Path.Combine(
                    Application.persistentDataPath,
                    "AssetBundle",
                    Patch.Value.name
                );

                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
        }

        await UniTask.Delay(500);

        PatchCompleteDelegate?.Invoke();

        DownLoadStartDelegate = null;
        PatchCompleteDelegate = null;
        DownloadProgressDelegate = null;
        PatchProgressDelegate = null;
    }

    private void SetProgress()
    {
        if (m_UpdateList == null || m_UpdateList.Count <= 0)
            return;

        // 현재 다운로드 중인 스레드들의 Byte 합계
        ulong downloadingBytes = 0;

        for (int i = 0; i < ThreadDownloadedBytes.Length; i++)
        {
            downloadingBytes += ThreadDownloadedBytes[i];
        }

        // 완료된 파일 + 현재 다운로드 중인 파일
        long currentDownloadBytes = CompleteDownloadBytes + (long)downloadingBytes;

        // 전체 용량보다 넘어가지 않도록 제한
        currentDownloadBytes = Math.Min(currentDownloadBytes, TotalDownloadBytes);

        // 실제 Byte 기준 진행률
        float progress = 0f;

        if (TotalDownloadBytes > 0)
        {
            progress = currentDownloadBytes / (float)TotalDownloadBytes;
        }

        progress = Mathf.Clamp01(progress);

        // 기존 진행률 Delegate
        DownloadProgressDelegate?.Invoke(progress * 100f);

        // 패치 UI 진행 정보
        PatchProgressDelegate?.Invoke(progress, CompleteList.Count, m_UpdateList.Count, currentDownloadBytes, TotalDownloadBytes);
    }
}