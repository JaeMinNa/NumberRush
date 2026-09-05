using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

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

    private bool IsProcess = false;
    private PacketType prevPacketType = PacketType.None;
    private ContentsType prevContentsType = ContentsType.None;
    private int prevSubType = -1;


    #region Override Method
    protected override void CreateInstance()
    {

    }

    public override void DestroyInstance()
    {

    }
    #endregion

    public void SendPacket(PacketType packetType, ContentsType contentsType = default, int subType = default, string Data = default, UnityAction receiveAction = null)
    {
        StartCoroutine(SendServer(packetType, contentsType, subType, Data, receiveAction));
    }

    public void SendContentsPacket(ContentsType contentsType, PacketHeader headerData, List<PacketBody> bodyData = default, UnityAction receiveAction = null)
    {
        string sendData = Util.MakeGamePacket(contentsType, headerData, bodyData);
        StartCoroutine(SendServer(PacketType.ContentsPacket, contentsType, headerData.ContentsIndex, sendData, receiveAction));
    }

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

        //UIManager.Instance.SetTouchBlock(true);

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
                        //case PacketType.GetGameData:
                        //    {
                        //        List<string> TableData = Util.ToObjectJson<List<string>>(RecvPacket.Data);
                        //        DataManager.Load(TableData);
                        //        ReceiveAction?.Invoke();
                        //    }
                        //    break;
                        //case PacketType.GetServerTime:
                        //    {
                        //        GameManager.Instance.SetServerTime(RecvPacket.Data);
                        //        ReceiveAction?.Invoke();
                        //    }
                        //    break;

                        //case PacketType.CheckBanState:
                        //    {
                        //        ReceiveAction?.Invoke();
                        //    }
                        //    break;

                        //// 최초 접속 시 UID 생성
                        //case PacketType.CheckUID:
                        //    {
                        //        GameManager.Instance.UID = RecvPacket.Data;
                        //        ReceiveAction?.Invoke();
                        //    }
                        //    break;

                        //// 플랫폼 로그인으로 UID와 계정 정보를 가져오기
                        //case PacketType.GetUIDbyPlatform:
                        //    {
                        //        GameManager.Instance.UID = RecvPacket.Data;
                        //    }
                        //    break;
                        case PacketType.ContentsPacket:
                            {
                                yield return PacketSystem.ProcessPacket(RecvPacket.Data, receiveAction);
                            }
                            break;
                    }
                }
                else
                {
                    //UIManager.Instance.SetTouchBlock(false);
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
        //UIManager.Instance.SetTouchBlock(false);
    }

    // 서버와 통신이 성공했을 경우 에러
    private void OnServerError(string PacketData)
    {
        //if (Util.TryToObjectJson(Util.StringDecompress(PacketData), out ServerPacket RecvPacket))
        //{
        //    var MsgData = new MessageData() { Message = $"PacketType: {RecvPacket.PacketType}, StateType: {RecvPacket.StateType}", Type = PopupType.OkOnly };
        //    UIManager.Instance.OpenSystemPopup(MsgData);

        //    //switch (RecvPacket.StateType)
        //    //{
        //    //}
        //}
        //else
        //{
        //    var MsgData = new MessageData() { Message = PacketData, Type = PopupType.OkOnly };
        //    UIManager.Instance.OpenSystemPopup(MsgData);
        //}
    }

    // 서버와 통신이 실패했을 경우 에러
    private void OnConnectionError(PacketType packetType, ContentsType contentsType, int subType, string Data, UnityAction RecvAction)
    {
        //var MsgData = new MessageData() { Message = $"PacketType: {packetType}, ContentsType: {contentsType}", Type = PopupType.OkOnly };
        //UIManager.Instance.OpenSystemPopup(MsgData);
    }

    private string MakePacketType(PacketType packetType, ContentsType contentsType, int subContentsType)
    {
        switch (contentsType)
        {
            default:
                return $"{packetType}/{contentsType}/{subContentsType}";
        }
    }
}