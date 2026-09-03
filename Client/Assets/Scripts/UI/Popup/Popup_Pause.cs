using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_Pause : UIElement
{
    #region Cashed Object
    [SerializeField] private Button Btn_Continue = null;
    [SerializeField] private Button Btn_Quit = null;
    #endregion

    #region Member Property
    private BattleModule m_BattleModule = null;
    #endregion

    #region Override Method
    public override void Init()
    {
        if (m_BattleModule == null)
            m_BattleModule = BattleModule.Instance;

        Btn_Continue.onClick.AddListener(OnClick_Continue);
        Btn_Quit.onClick.AddListener(OnClick_Quit);
    }

    public override void OnOpen(List<object> Args)
    {

    }

    public override void OnClose()
    {
        m_BattleModule.SetPause(false);
    }

    public override void OnRefresh()
    {

    }
    #endregion

    #region Button Event
    private void OnClick_Continue()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        UIManager.Instance.Close<Popup_Pause>();
    }

    private void OnClick_Quit()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        m_BattleModule.EndGame();
        UIManager.Instance.Close<Popup_System>();
        GameManager.Instance.LoadLobbyScenen();
    }
    #endregion
}