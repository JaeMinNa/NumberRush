using UnityEngine;

public class TitleScene : MonoBehaviour
{
    [SerializeField] private GameObject Root_UI = null;

    // TitleScene 최초 호출 시점
    private void Start()
    {
        GameManager.Instance.InitDefaultManager();
        UIManager.Instance.SetUIRoot(Root_UI);
        UIManager.Instance.SetActiveRoot(UI.BackGround, false);

        // 타이틀
        //UIManager.Instance.Open<TitleWindow>(UI.Main, "Prefabs/TitleWindow");
    }
}