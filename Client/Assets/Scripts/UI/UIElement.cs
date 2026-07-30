using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIElement : MonoBehaviour
{
    public string UIName = string.Empty;
    public RectTransform RectTransform;
    public UI UIParent;

    // 초기화
    public abstract void Init();
    // UI가 열릴 때 호출
    public abstract void OnOpen(List<object> Args);
    // UI가 닫힐 때 호출
    public abstract void OnClose();
    // UI 갱신
    public abstract void OnRefresh();

    #region Async
    public virtual async UniTask InitAsync()
    {
        await UniTask.Yield();
    }

    public virtual async UniTask OnOpenAsync(List<object> Args)
    {
        await UniTask.Yield();
    }

    public virtual async UniTask OnCloseAsync()
    {
        await UniTask.Yield();
    }

    public virtual async UniTask OpenAction()
    {
        await UniTask.Yield();
    }

    public virtual async UniTask CloseAction()
    {
        await UniTask.Yield();
    }
    #endregion
}