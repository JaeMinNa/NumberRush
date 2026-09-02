using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_System : UIElement
{
    #region Cashed Object
    [SerializeField] private TMP_Text Text_Title;
    [SerializeField] private TMP_Text Text_Message;
    [SerializeField] private Button Btn_OK;
    [SerializeField] private Button Btn_Cancel;
    #endregion

    #region Member Property
    private MessageData MyData = null;
    #endregion

    #region Override Method
    public override void Init()
    {
        Btn_OK.onClick.AddListener(OnClick_OK);
        Btn_Cancel.onClick.AddListener(OnClick_Cancel);
    }

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
            Text_Title.text = "NOTICE";
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

    public override void OnClose()
    {
    }

    public override void OnRefresh()
    {
    }
    #endregion

    #region Button Event
    private void OnClick_OK()
    {
        if (MyData != null)
            MyData.OkAction?.Invoke();

        UIManager.Instance.Close<Popup_System>();
    }

    private void OnClick_Cancel()
    {
        UIManager.Instance.Close<Popup_System>();
    }
    #endregion
}