using Cysharp.Threading.Tasks;
using UnityEngine;

public class TitleScene : MonoBehaviour
{
    [SerializeField] private GameObject Root_UI = null;

    // TitleScene 최초 호출 시점
    private async void Start()
    {
        GameManager.Instance.InitDefaultManager();
        UIManager.Instance.SetUIRoot(Root_UI);
        UIManager.Instance.SetActiveRoot(UI.BackGround, false);

        // 번들 패치 확인
        if (!Util.IsEditor() ||
            GameManager.Instance.GameConfig.ResourceLoadType() == ResourceType.Bundle)
        {
            Debug.LogWarning("Resource Type : Bundle");

            // 패치 다운로드가 실제로 완료됐을 때 호출
            PatchManager.PatchCompleteDelegate = OnPatchComplete;

            bool isComplete = await PatchManager.Instance.CheckPatch();
            if (!isComplete)
            {
                Debug.LogWarning("[TitleScene] 패치 다운로드 대기");
            }
        }
        else
        {
            Debug.LogWarning("Resource Type : Local");

            OpenTitleWindow();
        }
    }

    private void OnPatchComplete()
    {
        Debug.LogWarning("[TitleScene] 패치 완료");

        OpenTitleWindow();
    }

    private void OpenTitleWindow()
    {
        Debug.LogWarning("[TitleScene] TitleWindow Open");

        UIManager.Instance.Open<TitleWindow>(UI.Main, "Prefabs/UI/Window/TitleWindow");
    }
}