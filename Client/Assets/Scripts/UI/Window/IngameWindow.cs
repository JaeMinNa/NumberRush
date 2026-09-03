using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IngameWindow : UIElement
{
    #region Cashed Object
    [Header("TopUI")]
    [SerializeField] private TMP_Text Text_Hp = null;
    [SerializeField] private TMP_Text Text_Time = null;
    [SerializeField] private TMP_Text Text_Gold = null;
    [SerializeField] private TMP_Text Text_Score = null;
    [SerializeField] private Button Btn_Pause = null;

    [Header("Formula")]
    [SerializeField] private RectTransform Rect_Formula = null;
    [SerializeField] private TMP_Text[] Text_FormulaSlots = null;
    [SerializeField] private Button Btn_Skip = null;
    [SerializeField] private TMP_Text Text_SkipCount = null;

    [Header("Calculator")]
    [SerializeField] private Transform Trans_Content_Normal_Up = null;
    [SerializeField] private Transform Trans_Content_Normal_Down = null;
    [SerializeField] private Transform Trans_Content_Equip = null;
    [SerializeField] private Button Btn_Clear = null;
    [SerializeField] private Button Btn_Equal = null;
    #endregion

    #region Member Property
    private ChapterModule m_ChapterModule = null;
    private GameObject m_SlotNumberObj = null;
    #endregion

    #region Override Method
    public override void Init()
    {
        if (m_ChapterModule == null)
            m_ChapterModule = BattleModule.Instance as ChapterModule;

        if (m_SlotNumberObj == null)
            m_SlotNumberObj = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/UI/Slot/Slot_Number");

        InitSlots();

        Btn_Skip.onClick.AddListener(OnClick_Skip);
        Btn_Clear.onClick.AddListener(OnClick_Clear);
        Btn_Equal.onClick.AddListener(OnClick_Equal);
        Btn_Pause.onClick.AddListener(OnClick_Pause);
    }

    public override void OnOpen(List<object> args)
    {
        SoundManager.Instance.StartBGM("BGM_Game");

        RefreshTopUI();
        RefreshFormula();

        m_ChapterModule.SetPause(false);
    }

    public override void OnClose()
    {
    }

    public override void OnRefresh()
    {
        RefreshTopUI();
        RefreshFormula();
    }
    #endregion

    #region Public Method

    #endregion

    #region Member Method
    private void OnClick_Number(int number)
    {
        SoundManager.Instance.StartSFX("ClickButton");

        m_ChapterModule.AddNumber(number);

        RefreshFormula();
    }

    private void OnClick_Skip()
    {
        // 스킵 개수 확인
        if (m_ChapterModule.SkipNowCount <= 0)
        {
            SoundManager.Instance.StartSFX("MissButton");
            return;
        }

        SoundManager.Instance.StartSFX("ClickButton");

        m_ChapterModule.SkipNowCount--;

        m_ChapterModule.SetFormula();
        RefreshFormula();
    }

    private void OnClick_Clear()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        m_ChapterModule.ClearInputNumbers();

        RefreshFormula();
    }

    private void OnClick_Equal()
    {
        m_ChapterModule.Calculate();

        RefreshFormula();
    }

    private void OnClick_Pause()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        m_ChapterModule.SetPause(true, true);
    }

    private void InitSlots()
    {
        // Normal Number
        for (int i = 0; i < 4; ++i)
        {
            int number = i + 1;
            GameObject slotObj = Instantiate(m_SlotNumberObj, Trans_Content_Normal_Up);
            slotObj.GetComponent<Slot_Number>().SetSlot(SlotType.Normal, number, () => OnClick_Number(number));
        }

        for (int i = 0; i < 5; ++i)
        {
            int number = i + 5;
            GameObject slotObj = Instantiate(m_SlotNumberObj, Trans_Content_Normal_Down);
            slotObj.GetComponent<Slot_Number>().SetSlot(SlotType.Normal, number, () => OnClick_Number(number));
        }

        // Equip Number
        for (int i = 0; i < User.UserNumberData.EquipNumber.Count; ++i)
        {
            int number = User.UserNumberData.EquipNumber[i];

            GameObject slotObj = Instantiate(m_SlotNumberObj, Trans_Content_Equip);
            slotObj.GetComponent<Slot_Number>().SetSlot(SlotType.Select, number, () => OnClick_Number(number));
        }
    }

    private void RefreshFormula()
    {
        // 모든 슬롯 비활성화, 텍스트 초기화
        for (int i = 0; i < Text_FormulaSlots.Length; ++i)
        {
            Text_FormulaSlots[i].text = "";
            Text_FormulaSlots[i].transform.parent.gameObject.SetActive(false);
        }

        // 연산자 개수에 따른 슬롯 개수
        int slotCount = (m_ChapterModule.OperatorCount * 2) + 1;

        // 계산식 숫자 개수에 따른 Formula Width 설정
        float formulaWidth = 350f;

        switch (m_ChapterModule.OperatorCount + 1)
        {
            case 2:
                formulaWidth = 350f;
                break;

            case 3:
                formulaWidth = 550f;
                break;

            case 4:
                formulaWidth = 760f;
                break;
        }

        Rect_Formula.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, formulaWidth);

        // 슬롯 활성화
        for (int i = 0; i < slotCount; ++i)
        {
            Text_FormulaSlots[i].transform.parent.gameObject.SetActive(true);
        }

        // 연산자 표시
        for (int i = 0; i < m_ChapterModule.OperatorCount; ++i)
        {
            int slotIndex = (i * 2) + 1;

            Text_FormulaSlots[slotIndex].text = m_ChapterModule.GetOperatorText(i);
        }

        // 입력한 숫자 표시
        for (int i = 0; i < m_ChapterModule.InputNumberCount; ++i)
        {
            int slotIndex = i * 2;

            Text_FormulaSlots[slotIndex].text = m_ChapterModule.GetInputNumber(i).ToString();
        }

        // 스킵 개수 표시
        Text_SkipCount.text = $"SKIP {m_ChapterModule.SkipNowCount}/{m_ChapterModule.SkipTotalCount}";
    }
    
    private void RefreshTopUI()
    {
        Text_Hp.text = m_ChapterModule.CurHp.ToString();
        Text_Score.text = $"SCORE {m_ChapterModule.Score}";
        Text_Gold.text = $"{m_ChapterModule.Gold}";
    }
    #endregion

    #region Unity Method
    private void Update()
    {
        Text_Time.text = m_ChapterModule.CurTime.ToString("F2");
    }
    #endregion
}
