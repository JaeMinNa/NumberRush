using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_Settings : UIElement
{
    #region Cashed Object
    [SerializeField] private Button Btn_Close = null;
    [SerializeField] private Button Btn_Exit = null;
    #endregion

    #region Member Property
    #endregion

    #region Override Method
    public override void Init()
    { 

        Btn_Close.onClick.AddListener(OnClick_Close);
        Btn_Exit.onClick.AddListener(OnClick_Exit);
    }

    public override void OnOpen(List<object> Args)
    {

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
        UIManager.Instance.Close<Popup_Settings>();
    }

    private void OnClick_Exit()
    {
        GameManager.Instance.ExitGame();
    }
    #endregion
}