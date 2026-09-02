using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_Shop : UIElement
{
    #region Cashed Object
    [SerializeField] private Button Btn_BuyOneNumber_Random = null;
    [SerializeField] private Button Btn_BuyTenNumber_Random = null;
    [SerializeField] private Button Btn_BuyOneNumber_0 = null;
    [SerializeField] private Button Btn_Close = null;
    #endregion

    #region Member Property
    #endregion

    #region Override Method
    public override void Init()
    {
        Btn_BuyOneNumber_Random.onClick.AddListener(OnClick_BuyOneNumber_Random);
        Btn_BuyTenNumber_Random.onClick.AddListener(OnClick_BuyTenNumber_Random);
        Btn_BuyOneNumber_0.onClick.AddListener(() => OnClick_BuyTenNumber_Select(0));
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

    #region Button Event
    private void OnClick_BuyOneNumber_Random()
    {
        if (User.UserGameData.Gold < 2000)
        {
            UIManager.Instance.OpenSystemPopup(new MessageData { Type = PopupType.OkOnly, Message = "You don't have enough Gold." });
            return;
        }

        var Head = Util.MakeHeaderData(UserNumberContents.BuyOneNumber_Random);
        NetworkManager.Instance.SendContentsPacket(ContentsType.UserNumber, Head);
    }

    private void OnClick_BuyTenNumber_Random()
    {
        if (User.UserGameData.Gold < 18000)
        {
            UIManager.Instance.OpenSystemPopup(new MessageData { Type = PopupType.OkOnly, Message = "You don't have enough Gold." });
            return;
        }

        var Head = Util.MakeHeaderData(UserNumberContents.BuyTenNumber_Random);
        NetworkManager.Instance.SendContentsPacket(ContentsType.UserNumber, Head);
    }

    private void OnClick_BuyTenNumber_Select(int num)
    {
        if (User.UserGameData.Gold < 100000)
        {
            UIManager.Instance.OpenSystemPopup(new MessageData { Type = PopupType.OkOnly, Message = "You don't have enough Gold." });
            return;
        }

        var Head = Util.MakeHeaderData(UserNumberContents.BuyOneNumber_Select, num.ToString());
        NetworkManager.Instance.SendContentsPacket(ContentsType.UserNumber, Head);
    }

    private void OnClick_Close()
    {
        UIManager.Instance.Close<Popup_Shop>();
    }
    #endregion
}