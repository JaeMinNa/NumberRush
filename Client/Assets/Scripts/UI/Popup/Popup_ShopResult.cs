using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_ShopResult : UIElement
{
    #region Cashed Object
    [SerializeField] private Transform Trans_Content = null;
    [SerializeField] private Button Btn_Close = null;
    #endregion

    #region Member Property
    List<int> m_ResultNumbers = new List<int>();
    private GameObject m_SlotNumberObj = null;
    #endregion

    #region Override Method
    public override void Init()
    {
        if (m_SlotNumberObj == null)
            m_SlotNumberObj = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/UI/Slot/Slot_Number");

        Btn_Close.onClick.AddListener(OnClick_Close);
    }

    public override void OnOpen(List<object> Args)
    {
        if (Args.Count == 0)
        {
            Debug.LogWarning("BuyData is Null");
            return;
        }

        m_ResultNumbers = Args[0] as List<int>;

        for (int i = 0; i < m_ResultNumbers.Count; ++i)
        {
            GameObject slotObj = Instantiate(m_SlotNumberObj, Trans_Content);
            slotObj.GetComponent<Slot_Number>().SetSlot(SlotType.Select, m_ResultNumbers[i]);
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
        UIManager.Instance.Close<Popup_ShopResult>();
    }
    #endregion
}