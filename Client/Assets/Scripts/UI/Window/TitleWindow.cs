using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TitleWindow : UIElement
{
    #region Cashed Object
    [SerializeField] private GameObject Obj_InputFiled = null;
    [SerializeField] private TMP_Text Text_Account = null;
    [SerializeField] private TMP_InputField Text_InputFieldAccount = null;
    [SerializeField] private Button Btn_EditorLogin = null;
    #endregion

    #region Member Property
    #endregion

    #region Override Method
    public override void Init()
    {
        // 초기화
        Obj_InputFiled.SetActive(false);
        Btn_EditorLogin.gameObject.SetActive(false);

        // 버튼 연결
        Btn_EditorLogin.onClick.AddListener(OnClick_EditorLogin);
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
        if (Util.IsEditor())
        {
            Obj_InputFiled.SetActive(true);
            Btn_EditorLogin.gameObject.SetActive(true);
        }
        else
        {
            // Todo : 구글 로그인 구현
        }
    }

    private void EnterLobbyScene()
    {
        GameManager.Instance.LoadLobbyScenen();
    }

    private async void OnClick_EditorLogin()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        GameManager.Instance.AccountCode = Text_InputFieldAccount.text;
        NetworkManager.Instance.SendPacket(PacketType.GetUserData, receiveAction : EnterLobbyScene);
    }
    #endregion
}
