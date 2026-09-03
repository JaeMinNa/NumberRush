using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_MyNumbers : UIElement
{
    #region Cashed Object
    [Header("User Info")]
    [SerializeField] private Image Img_Character = null;
    [SerializeField] private TMP_Text Text_Nickname = null;
    [SerializeField] private TMP_Text Text_Level = null;

    [Header("Equip Numbers")]
    [SerializeField] private TMP_Text Text_EquipNumbersCount = null;
    [SerializeField] private Transform Trans_Content_Equip = null;

    [Header("Inventory")]
    [SerializeField] private TMP_Text Text_InventoryCount = null;
    [SerializeField] private Transform Trans_Content_Inven = null;

    [SerializeField] private Button Btn_Close = null;
    #endregion

    #region Member Property
    private GameObject m_SlotNumberObj = null;
    private List<Slot_Number> m_EquipSlots = new List<Slot_Number>();
    private List<Slot_Number> m_InvenSlots = new List<Slot_Number>();
    #endregion

    #region Override Method
    public override void Init()
    {
        if (m_SlotNumberObj == null)
            m_SlotNumberObj = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/UI/Slot/Slot_Number");

        Btn_Close.onClick.AddListener(OnClick_Quit);
    }

    public override void OnOpen(List<object> Args)
    {
        CreateEquipSlots();
        CreateInventorySlots();

        SetEquipNumbers();
        SetInventory();
        SetUserInfo();
    }

    public override void OnClose()
    {
        
    }

    public override void OnRefresh()
    {
        SetEquipNumbers();
        SetInventory();
        SetUserInfo();
    }
    #endregion

    #region Button Event
    private void CreateEquipSlots()
    {
        if (m_EquipSlots.Count > 0)
            return;

        for (int i = 0; i < 5; ++i)
        {
            GameObject slotObj = Instantiate(m_SlotNumberObj, Trans_Content_Equip);
            Slot_Number slot = slotObj.GetComponent<Slot_Number>();

            m_EquipSlots.Add(slot);
        }
    }

    private void CreateInventorySlots()
    {
        if (m_InvenSlots.Count > 0)
            return;

        for (int i = 0; i < 100; ++i)
        {
            GameObject slotObj = Instantiate(m_SlotNumberObj, Trans_Content_Inven);
            Slot_Number slot = slotObj.GetComponent<Slot_Number>();

            m_InvenSlots.Add(slot);
        }
    }

    private void SetEquipNumbers()
    {
        int equipCount = User.UserNumberData.EquipNumber.Count;

        Text_EquipNumbersCount.text = $"{equipCount} <color=#c6e4fd>/ 5</color>";

        for (int i = 0; i < m_EquipSlots.Count; ++i)
        {
            Slot_Number slot = m_EquipSlots[i];

            if (i < equipCount)
            {
                int num = User.UserNumberData.EquipNumber[i];

                slot.SetSlot(SlotType.Select, num, () => OnClick_EquipSlot(SlotType.Normal, num)
                );
            }
            else
            {
                slot.SetSlot(SlotType.Add);
            }
        }
    }

    private void SetInventory()
    {
        var equipNumber = User.UserNumberData.EquipNumber;
        var numberInven = User.UserNumberData.NumberInventory;

        Text_InventoryCount.text = $"{9 + numberInven.Count} <color=#c6e4fd>/ 101</color>";

        for (int i = 0; i < m_InvenSlots.Count; ++i)
        {
            int num = i;
            Slot_Number slot = m_InvenSlots[i];

            // 기본 Number (1~9)
            if (num >= 1 && num <= 9)
            {
                slot.SetSlot(SlotType.Equip, num);
                continue;
            }

            // Equip Number
            if (equipNumber.Contains(num))
            {
                slot.SetSlot(SlotType.Equip, num);
                continue;
            }

            // Inven Number
            if (numberInven.Contains(num))
            {
                slot.SetSlot(SlotType.Normal, num, () => OnClick_InvenSlot(SlotType.Normal, num)
                );

                continue;
            }

            // Lock Number
            slot.SetSlot(SlotType.Lock, num);
        }
    }

    private void SetUserInfo()
    {
        Img_Character.sprite = ResourceLoader.LoadAssetResources<Sprite>($"Image/Character/Character_Sample{User.UserCommonData.ImageNum}");
        Text_Nickname.text = User.UserCommonData.NickName;
        Text_Level.text = $"Lv. {GameUtil.GetLevel(User.UserGameData.Score)}";
    }

    private void OnClick_EquipSlot(SlotType type, int num)
    {
        SoundManager.Instance.StartSFX("ClickButton");

        switch (type)
        {
            case SlotType.Normal:

                var equipNumber = new List<int>(User.UserNumberData.EquipNumber);
                equipNumber.Remove(num);

                // 장착 해제 데이터를 서버 저장
                var Head = Util.MakeHeaderData(UserNumberContents.SetEquip, Util.ToJson(equipNumber));
                NetworkManager.Instance.SendContentsPacket(ContentsType.UserNumber, Head);

                break;

            default:
                break;
        }
    }

    private void OnClick_InvenSlot(SlotType type, int num)
    {
        SoundManager.Instance.StartSFX("ClickButton");

        var equipNumber = new List<int>(User.UserNumberData.EquipNumber);

        // 이미 최대 장착 슬롯 일 때
        if (equipNumber.Count >= 5)
            return;

        switch (type)
        {
            case SlotType.Normal:

                equipNumber.Add(num);

                // 장착 데이터를 서버 저장
                var Head = Util.MakeHeaderData(UserNumberContents.SetEquip, Util.ToJson(equipNumber));
                NetworkManager.Instance.SendContentsPacket(ContentsType.UserNumber, Head);

                break;

            default:
                break;
        }
    }



    private void OnClick_Quit()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        UIManager.Instance.Close<Popup_MyNumbers>();
    }
    #endregion
}