using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Slot_Ranking : MonoBehaviour
{
    #region Cashed Object
    [SerializeField] private Image Img_Icon = null;
    [SerializeField] private TMP_Text Text_Rank = null;
    [SerializeField] private Image Img_Character = null;
    [SerializeField] private TMP_Text Text_LvNickName = null;
    [SerializeField] private TMP_Text Text_Score = null;
    [SerializeField] private TMP_Text Text_Time = null;
    [SerializeField] private Transform Trans_Content = null;
    #endregion

    #region Member Property
    private GameObject m_SlotNumberObj = null;
    #endregion

    #region Unity Method
    private void Awake()
    {
        if (m_SlotNumberObj == null)
            m_SlotNumberObj = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/UI/Slot/Slot_Number");
    }
    #endregion

    public void SetSlot(UserRankInfo info)
    {
        switch (info.Rank)
        {
            case 1 :
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

        Text_Rank.text = info.Rank.ToString();
        Img_Character.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Character/Character_Sample{info.ImageNum}");
        Text_LvNickName.text = $"LV {GameUtil.GetLevel(info.Score)}  {info.NickName}";
        Text_Score.text = $"SCORE : {info.Score}";
        Text_Time.text = $"TIME : {info.Time.ToString("F2")}";


        if (info.EquipNumber != null)
        {
            for (int i = 0; i < info.EquipNumber.Count; ++i)
            {
                GameObject slotObj = Instantiate(m_SlotNumberObj, Trans_Content);
                slotObj.GetComponent<Slot_Number>().SetSlot(SlotType.Select, info.EquipNumber[i]);
            }
        }
    }
}
