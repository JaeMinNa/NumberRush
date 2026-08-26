using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameWindow : UIElement
{
    #region Cashed Object
    [Header("TopUI")]
    [SerializeField] private TMP_Text Text_Hp = null;
    [SerializeField] private TMP_Text Text_Time = null;
    [SerializeField] private TMP_Text Text_Score = null;
    [SerializeField] private Button Btn_Pause = null;

    [Header("Formula")]
    [SerializeField] private TMP_Text[] Text_FormulaSlots = null;
    [SerializeField] private Button Btn_Skip = null;
    [SerializeField] private TMP_Text Text_SkipCount = null;

    [Header("Calculator")]
    [SerializeField] private Button[] Btn_NormalNumbers = null;
    [SerializeField] private Button[] Btn_EquipNumbers = null;
    [SerializeField] private Button Btn_Clear = null;
    [SerializeField] private Button Btn_Equal = null;
    #endregion

    #region Member Property
    private ChapterModule m_ChapterModule = null;
    #endregion

    #region Override Method
    public override void Init()
    {
        // 초기화
        if (m_ChapterModule == null)
            m_ChapterModule = BattleModule.Instance as ChapterModule;

        // 버튼 연결
        for (int i = 0; i < Btn_NormalNumbers.Length; ++i)
        {
            int number = i + 1;

            Btn_NormalNumbers[i].onClick.AddListener(() =>
            {
                OnClick_Number(number);
            });
        }

        Btn_Skip.onClick.AddListener(OnClick_Skip);
        Btn_Clear.onClick.AddListener(OnClick_Clear);
        Btn_Equal.onClick.AddListener(OnClick_Equal);
    }

    public override void OnOpen(List<object> args)
    {
        RefreshTopUI();
        RefreshFormula();
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
        m_ChapterModule.AddNumber(number);

        RefreshFormula();
    }

    private void OnClick_Skip()
    {
        // 스킵 개수 확인
        if (m_ChapterModule.SkipNowCount <= 0)
            return;

        m_ChapterModule.SkipNowCount--;

        m_ChapterModule.SetFormula();
        RefreshFormula();
    }

    private void OnClick_Clear()
    {
        m_ChapterModule.ClearInputNumbers();

        RefreshFormula();
    }

    private void OnClick_Equal()
    {
        m_ChapterModule.Calculate();

        RefreshFormula();
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
    }
    #endregion

    #region Unity Method
    private void Update()
    {
        Text_Time.text = m_ChapterModule.CurTime.ToString("F2");
    }
    #endregion
}
