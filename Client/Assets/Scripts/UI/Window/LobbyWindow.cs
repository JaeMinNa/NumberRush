using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyWindow : UIElement
{
    #region Cashed Object
    [SerializeField] private Button Btn_GameStart = null;
    #endregion

    #region Member Property
    #endregion

    #region Override Method
    public override void Init()
    {
        // 초기화

        // 버튼 연결
        Btn_GameStart.onClick.AddListener(OnClick_GameStart);
    }

    public override void OnOpen(List<object> args)
    { 

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
    #endregion
}
