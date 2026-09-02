using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_UserInfo : UIElement
{
    #region Cashed Object
    [SerializeField] private Image Img_CurCharacter = null;
    [SerializeField] private TMP_Text Text_Score = null;
    [SerializeField] private TMP_Text Text_Level = null;
    [SerializeField] private TMP_Text Text_NickName = null;
    [SerializeField] private Button Btn_ChangeNickname = null;

    [SerializeField] private Button[] Btn_Characters = null;
    [SerializeField] private GameObject[] Obj_Selects = null;

    [SerializeField] private Button Btn_Close = null;
    
    #endregion

    #region Member Property
    #endregion

    #region Override Method
    public override void Init()
    {
        for (int i = 0; i < Btn_Characters.Length; ++i)
        {
            int index = i + 1;
            Btn_Characters[i].onClick.AddListener(() => OnClick_SelectCharactor(index));
        }

        Btn_ChangeNickname.onClick.AddListener(OnClick_ChangeNickName);
        Btn_Close.onClick.AddListener(OnClick_Close);
    }

    public override void OnOpen(List<object> Args)
    {
        Img_CurCharacter.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Character/Character_Sample{User.UserCommonData.ImageNum}");
        Text_Score.text = $"SCORE : {User.UserGameData.Score}";
        Text_Level.text = GameUtil.GetLevel(User.UserGameData.Score).ToString();
        Text_NickName.text = User.UserCommonData.NickName;

        SetSelectCharacters();
    }

    public override void OnClose()
    {

    }

    public override void OnRefresh()
    {
        Img_CurCharacter.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Character/Character_Sample{User.UserCommonData.ImageNum}");
        Text_NickName.text = User.UserCommonData.NickName;

        SetSelectCharacters();
    }
    #endregion

    #region Button Event
    private void SetSelectCharacters()
    {
        foreach (var obj in Obj_Selects)
            obj.SetActive(false);

        int characterNum = int.Parse(User.UserCommonData.ImageNum) - 1;
        Obj_Selects[characterNum].SetActive(true);
    }

    private void OnClick_SelectCharactor(int num)
    {
        if (num < 1 || num > 6)
            return;

        string newImageNum = $"0{num}";

        var Head = Util.MakeHeaderData(UserContents.ChangeImageNumber, newImageNum);
        NetworkManager.Instance.SendContentsPacket(ContentsType.User, Head);
    }

    private void OnClick_ChangeNickName()
    {
        UIManager.Instance.Open<Popup_ChangeNickName>(UI.Popup, "Prefabs/UI/Popup/Popup_ChangeNickName");
    }

    private void OnClick_Close()
    {
        UIManager.Instance.Close<Popup_UserInfo>();
    }
    #endregion
}