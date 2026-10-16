using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Patch : UIElement
{
    #region Cashed Object
    [SerializeField] private TMP_Text Text_Update = null;
    [SerializeField] private Slider Slider_Update = null;

    #endregion


    #region Override Method

    public override void Init()
    {

    }

    public override void OnOpen(List<object> Args)
    {
        Slider_Update.value = 0f;
        Text_Update.text = "Updating... 0%";

        // 패치 진행 이벤트 등록
        PatchManager.PatchProgressDelegate += OnPatchProgress;

        // 패치 완료 이벤트 등록
        PatchManager.PatchCompleteDelegate += OnPatchComplete;
    }

    public override void OnClose()
    {
        // 이벤트 해제
        PatchManager.PatchProgressDelegate -= OnPatchProgress;
        PatchManager.PatchCompleteDelegate -= OnPatchComplete;
    }

    public override void OnRefresh()
    {

    }

    #endregion


    #region Patch Event

    private void OnPatchProgress(float progress, int completeFile, int totalFile, long downloadByte, long totalByte)
    {
        // 슬라이더
        Slider_Update.value = progress;

        // 퍼센트
        float percent = progress * 100f;

        // Byte -> MB
        float downloadMB = downloadByte / 1024f / 1024f;
        float totalMB = totalByte / 1024f / 1024f;

        // 텍스트
        Text_Update.text = $"Updating... {percent:F0}% | Files {completeFile}/{totalFile} | {downloadMB:F1}/{totalMB:F1} MB";
    }

    private async void OnPatchComplete()
    {
        Slider_Update.value = 1f;
        Text_Update.text = "Update Complete!";

        // 2초 대기
        await UniTask.Delay(2000);

        // 패치 팝업 닫기
        UIManager.Instance.Close<Popup_Patch>();
    }

    #endregion
}