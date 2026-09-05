using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Universal.UniversalSDK;

public class TitleWindow : UIElement
{
    #region Cashed Object
    //[SerializeField] private GameObject Obj_InputFiled = null;
    [SerializeField] private TMP_Text Text_Account = null;
    [SerializeField] private TMP_InputField Text_InputFieldAccount = null;
    [SerializeField] private Button Btn_GoogleLogin = null;
    [SerializeField] private Button Btn_GuestLogin = null;
    [SerializeField] private Button Btn_EditorLogin = null;
    #endregion

    #region Member Property
    #endregion

    #region Override Method
    public override void Init()
    {
        // 버튼 연결
        Btn_EditorLogin.onClick.AddListener(OnClick_EditorLogin);
        Btn_GoogleLogin.onClick.AddListener(OnClick_GoogleLogin);
        Btn_GuestLogin.onClick.AddListener(OnClick_GuestLogin);
    }

    public override void OnOpen(List<object> args)
    {
        SoundManager.Instance.StartBGM("BGM_Title");

        SetLogin();
    }

    public override void OnClose()
    {
    }

    public override void OnRefresh()
    {
    }
    #endregion

    #region Public Method
    public void InputAccount()
    {
        Text_Account.text = Text_InputFieldAccount.text;
    }
    #endregion

    #region Member Method
    private void SetLogin()
    {
        Btn_EditorLogin.gameObject.SetActive(false);
        Text_InputFieldAccount.gameObject.SetActive(false);
        Btn_GoogleLogin.gameObject.SetActive(false);
        Btn_GuestLogin.gameObject.SetActive(false);

        Text_InputFieldAccount.text = PlayerPrefs.GetString("UserAccountCode", string.Empty);

        if (Util.IsEditor())
        {
            Text_InputFieldAccount.gameObject.SetActive(true);
            Btn_EditorLogin.gameObject.SetActive(true);
        }
        else
        {
            Btn_GoogleLogin.gameObject.SetActive(true);
            Btn_GuestLogin.gameObject.SetActive(true);
        }
    }

    private void EnterLobbyScene()
    {
        GameManager.Instance.LoadLobbyScenen();
    }

    private async void OnClick_EditorLogin()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        PlayerPrefs.SetString("UserAccountCode", Text_InputFieldAccount.text);

        GameManager.Instance.AccountCode = Text_InputFieldAccount.text;
        NetworkManager.Instance.SendPacket(PacketType.GetUserData, receiveAction : EnterLobbyScene);
    }

    private async void OnClick_GoogleLogin()
    {
        SoundManager.Instance.StartSFX("ClickButton");

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

    private async void OnClick_GuestLogin()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        // 게스트 로그인을 처음할 때
        if (Text_InputFieldAccount.text == string.Empty)
        {
            // AccountCode를 랜덤으로 생성
            Text_InputFieldAccount.text = Guid.NewGuid().ToString();  
            PlayerPrefs.SetString("UserAccountCode", Text_InputFieldAccount.text);
        }

        Debug.LogWarning($"Guest Account : {Text_InputFieldAccount.text}");

        GameManager.Instance.AccountCode = Text_InputFieldAccount.text;
        NetworkManager.Instance.SendPacket(PacketType.GetUserData, receiveAction: EnterLobbyScene);
    }
    #endregion
}
