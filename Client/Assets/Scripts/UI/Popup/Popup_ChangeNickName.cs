using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_ChangeNickName : UIElement
{
    #region Cashed Object
    [SerializeField] private TMP_Text Text_NewNickName = null;
    [SerializeField] private TMP_InputField Text_InputField = null;
    [SerializeField] private Button Btn_ChangeNickName = null;
    [SerializeField] private Button Btn_Close = null;
    #endregion

    #region Member Property
    #endregion

    #region Override Method
    public override void Init()
    {
        // 글자수 7자로 제한
        Text_InputField.characterLimit = 7;

        Btn_ChangeNickName.onClick.AddListener(Onclick_ChangeNickName);
        Btn_Close.onClick.AddListener(OnClick_Close);
    }

    public override void OnOpen(List<object> Args)
    {

    }

    public override void OnClose()
    {

    }

    public override void OnRefresh()
    {

    }
    #endregion

    #region Public Method
    public void UserInputText()
    {
        Text_NewNickName.text = Text_InputField.text;
    }
    #endregion

    #region Button Event
    private void Onclick_ChangeNickName()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        var Head = Util.MakeHeaderData(UserContents.ChangeNickName, Text_NewNickName.text);
        NetworkManager.Instance.SendContentsPacket(ContentsType.User, Head);
    }

    private void OnClick_Close()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        UIManager.Instance.Close<Popup_ChangeNickName>();
    }
    #endregion
}