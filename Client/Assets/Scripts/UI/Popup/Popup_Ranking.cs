using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;


public class Popup_Ranking : UIElement
{
    #region Cashed Object
    [Header("My Rank Info")]
    [SerializeField] private Image Img_Icon = null;
    [SerializeField] private TMP_Text Text_Rank = null;
    [SerializeField] private Image Img_Character = null;
    [SerializeField] private TMP_Text Text_LvNickName = null;
    [SerializeField] private TMP_Text Text_Score = null;
    [SerializeField] private TMP_Text Text_Time = null;
    [SerializeField] private Transform Trans_Content_NumberSlot = null;

    [Header("Users Rank Info")]
    [SerializeField] private Transform Trans_Content_RankingSlot = null;

    [SerializeField] private Button Btn_Close = null;
    #endregion

    #region Member Property
    private GameObject m_SlotNumberObj = null;
    private GameObject m_SlotRankingObj = null;
    private UserRankInfo m_MyRankInfo = null;
    private List<UserRankInfo> m_UsersRankInfo = new List<UserRankInfo>();
    #endregion

    #region Override Method
    public override void Init()
    {
        if (m_SlotNumberObj == null)
            m_SlotNumberObj = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/UI/Slot/Slot_Number");

        if (m_SlotRankingObj == null)
            m_SlotRankingObj = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/UI/Slot/Slot_Ranking");

        Btn_Close.onClick.AddListener(OnClick_Close);
    }

    public override void OnOpen(List<object> Args)
    {
        if (Args.Count == 0)
            return;

        m_MyRankInfo = Args[0] as UserRankInfo;
        m_UsersRankInfo = Args[1] as List<UserRankInfo>;

        // 1. 나의 랭킹 데이터
        switch (m_MyRankInfo.Rank)
        {
            case 1:
                Img_Icon.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Ranking/icon_medal_gold");
                Text_Rank.color = new Color(243f / 255f, 97f / 255f, 30f / 255f);
                break;

            case 2:
                Img_Icon.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Ranking/icon_medal_silver");
                Text_Rank.color = new Color(136f / 255f, 136f / 255f, 136f / 255f);
                break;

            case 3:
                Img_Icon.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Ranking/icon_medal_bronze");
                Text_Rank.color = new Color(215f / 255f, 102f / 255f, 57f / 255f);
                break;

            default:
                Img_Icon.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Ranking/Frame_Circle_78");
                Img_Icon.rectTransform.sizeDelta = new Vector2(90f, 90f);
                Img_Icon.color = new Color(131f / 255f, 171f / 255f, 219f / 255f);
                Text_Rank.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
                break;
        }

        Text_Rank.text = m_MyRankInfo.Rank.ToString();
        Img_Character.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Character/Character_Sample{m_MyRankInfo.ImageNum}");
        Text_LvNickName.text = $"LV {GameUtil.GetLevel(m_MyRankInfo.Score)}  {m_MyRankInfo.NickName}";
        Text_Score.text = $"SCORE : {m_MyRankInfo.Score}";
        Text_Time.text = $"TIME : {m_MyRankInfo.Time.ToString("F2")}";

        if (m_MyRankInfo.EquipNumber != null)
        {
            for (int i = 0; i < m_MyRankInfo.EquipNumber.Count; ++i)
            {
                GameObject slotObj = Instantiate(m_SlotNumberObj, Trans_Content_NumberSlot);
                slotObj.GetComponent<Slot_Number>().SetSlot(SlotType.Select, m_MyRankInfo.EquipNumber[i]);
            }
        }

        // 2. 유저 랭킹 데이터
        for (int i = 0; i < m_UsersRankInfo.Count; ++i)
        {
            GameObject slotObj = Instantiate(m_SlotRankingObj, Trans_Content_RankingSlot);
            slotObj.GetComponent<Slot_Ranking>().SetSlot(m_UsersRankInfo[i]);
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
    private void OnClick_Close()
    {
        UIManager.Instance.Close<Popup_Ranking>();
    }
    #endregion
}