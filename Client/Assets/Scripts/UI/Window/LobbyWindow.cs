using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyWindow : UIElement
{
    #region Cashed Object
    [SerializeField] private Image Img_Character = null;
    [SerializeField] private TMP_Text Text_Score = null;
    [SerializeField] private TMP_Text Text_Level = null;
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
        Btn_GoldAdd.onClick.AddListener(OnClick_GoldAdd);
        Btn_Numbers.onClick.AddListener(OnClick_Numbers);
        Btn_Settings.onClick.AddListener(OnClick_Settings);
    }

    public override void OnOpen(List<object> args)
    {
        Img_Character.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Character/Character_Sample{User.UserCommonData.ImageNum}");
        Text_Score.text = $"SCORE : {User.UserGameData.Score}";
        Text_Level.text = GameUtil.GetLevel(User.UserGameData.Score).ToString();
        Text_Gold.text = User.UserGameData.Gold.ToString();
    }

    public override void OnClose()
    {
    }

    public override void OnRefresh()
    {
    }
    #endregion

    #region Public Method

    #endregion

    #region Member Method
    private void OnClick_GameStart()
    {
        GameManager.Instance.LoadGameScenen();
    }

    private void OnClick_GoldAdd()
    {
        // TODO
    }

    private void OnClick_Numbers()
    {
        UIManager.Instance.Open<Popup_MyNumbers>(UI.Popup, "Prefabs/UI/Popup/Popup_MyNumbers");
    }

    private void OnClick_Settings()
    {
        UIManager.Instance.Open<Popup_Settings>(UI.Popup, "Prefabs/UI/Popup/Popup_Settings");
    }
    #endregion
}
