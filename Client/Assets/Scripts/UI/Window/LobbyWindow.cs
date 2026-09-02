using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyWindow : UIElement
{
    #region Cashed Object
    [SerializeField] private Button Btn_UserInfo = null;
    [SerializeField] private Image Img_Character = null;
    [SerializeField] private TMP_Text Text_Score = null;
    [SerializeField] private TMP_Text Text_Level = null;
    [SerializeField] private TMP_Text Text_NickName = null;
    [SerializeField] private TMP_Text Text_Gold = null;
    [SerializeField] private Button Btn_GoldAdd = null;

    [SerializeField] private Button Btn_Numbers = null;
    [SerializeField] private Button Btn_Shop = null;
    [SerializeField] private Button Btn_Ranking = null;
    [SerializeField] private Button Btn_Settings = null;

    [SerializeField] private Button Btn_GameStart = null;
    #endregion

    #region Member Property
    #endregion

    #region Override Method
    public override void Init()
    {
        // 버튼 연결
        Btn_GameStart.onClick.AddListener(OnClick_GameStart);
        Btn_UserInfo.onClick.AddListener(OnClick_UserInfo);
        Btn_GoldAdd.onClick.AddListener(OnClick_GoldAdd);
        Btn_Numbers.onClick.AddListener(OnClick_Numbers);
        Btn_Shop.onClick.AddListener(OnClick_Shop);
        Btn_Ranking.onClick.AddListener(OnClick_Ranking);
        Btn_Settings.onClick.AddListener(OnClick_Settings);
    }

    public override void OnOpen(List<object> args)
    {
        Img_Character.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Character/Character_Sample{User.UserCommonData.ImageNum}");
        Text_Score.text = $"SCORE : {User.UserGameData.Score}";
        Text_Level.text = GameUtil.GetLevel(User.UserGameData.Score).ToString();
        Text_NickName.text = User.UserCommonData.NickName;
        Text_Gold.text = User.UserGameData.Gold.ToString();
    }

    public override void OnClose()
    {
    }

    public override void OnRefresh()
    {
        Img_Character.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Character/Character_Sample{User.UserCommonData.ImageNum}");
        Text_NickName.text = User.UserCommonData.NickName;
        Text_Gold.text = User.UserGameData.Gold.ToString();
    }
    #endregion

    #region Public Method

    #endregion

    #region Member Method
    private void OnClick_GameStart()
    {
        GameManager.Instance.LoadGameScenen();
    }

    private void OnClick_UserInfo()
    {
        UIManager.Instance.Open<Popup_UserInfo>(UI.Popup, "Prefabs/UI/Popup/Popup_UserInfo");
    }

    private void OnClick_GoldAdd()
    {
        UIManager.Instance.Open<Popup_Shop>(UI.Popup, "Prefabs/UI/Popup/Popup_Shop");

        // 골드 획득 치트
        if (Util.IsEditor())
        {
            var Head = Util.MakeHeaderData(UserContents.GoldCheat);
            NetworkManager.Instance.SendContentsPacket(ContentsType.User, Head);
        }
    }

    private void OnClick_Numbers()
    {
        UIManager.Instance.Open<Popup_MyNumbers>(UI.Popup, "Prefabs/UI/Popup/Popup_MyNumbers");
    }

    private void OnClick_Shop()
    {
        UIManager.Instance.Open<Popup_Shop>(UI.Popup, "Prefabs/UI/Popup/Popup_Shop");
    }

    private void OnClick_Ranking()
    {
        // 랭킹 데이터 가져오기
        var Head = Util.MakeHeaderData(UserContents.GetRankData);
        NetworkManager.Instance.SendContentsPacket(ContentsType.User, Head);
    }

    private void OnClick_Settings()
    {
        UIManager.Instance.Open<Popup_Settings>(UI.Popup, "Prefabs/UI/Popup/Popup_Settings");
    }
    #endregion
}
