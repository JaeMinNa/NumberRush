using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_EndGame : UIElement
{
    #region Cashed Object
    [SerializeField] private Button Btn_Home = null;
    [SerializeField] private Button Btn_Restart = null;

    [SerializeField] private TMP_Text Text_Score = null;
    [SerializeField] private TMP_Text Text_Gold = null;
    [SerializeField] private TMP_Text Text_Time = null;
    #endregion

    #region Member Property
    private ChapterModule m_ChapterModule = null;
    private int m_Score = 0;
    private int m_Gold = 0;
    private float m_Time = 0f;
    #endregion

    #region Override Method
    public override void Init()
    {
        if (m_ChapterModule == null)
            m_ChapterModule = BattleModule.Instance as ChapterModule;

        Btn_Restart.onClick.AddListener(OnClick_Restart);
        Btn_Home.onClick.AddListener(OnClick_Home);

        m_Score = 0;
        m_Gold = 0;
        m_Time = 0f;
    }

    public override void OnOpen(List<object> Args)
    {
        m_Score = m_ChapterModule.Score;
        m_Gold = m_ChapterModule.Gold;
        m_Time = m_ChapterModule.CurTime;

        Text_Score.text = $"SCORE : {m_Score}";
        Text_Gold.text = m_Gold.ToString();
        Text_Time.text = $"TIME : {m_Time.ToString("F2")}";

        // 서버 데이터 저장
        var Head = Util.MakeHeaderData(UserGameContents.EndChapter, Util.MakeData(m_Score.ToString(), m_Gold.ToString(), m_Time.ToString()));
        NetworkManager.Instance.SendContentsPacket(ContentsType.UserGame, Head);
    }

    public override void OnClose()
    {
        m_ChapterModule.SetPause(false);
    }

    public override void OnRefresh()
    {

    }
    #endregion

    #region Member Method
    private async void OnClick_Restart()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        await m_ChapterModule.RestartGame();

        UIManager.Instance.Close<Popup_EndGame>();
    }

    private void OnClick_Home()
    {
        SoundManager.Instance.StartSFX("ClickButton");

        m_ChapterModule.EndGame();

        UIManager.Instance.Close<Popup_EndGame>();
        GameManager.Instance.LoadLobbyScenen();
    }
    #endregion
}