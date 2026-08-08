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

       //// Test
       //// 로그인(나중에 구글 로그인으로 대체)
       //// GameManager.Instance.AccountCode = "1234";
       //// NetworkManager.Instance.SendPacket(PacketType.GetUserData);

       //// 5초 대기
       //// await UniTask.Delay(5000);

       //// 닉네임 변경
       //// var Head = Util.MakeHeaderData(UserContents.ChangeNickName, "Jaemin");
       //// NetworkManager.Instance.SendContentsPacket(ContentsType.User, Head);

       //// 5초 대기
       //// await UniTask.Delay(5000);

       //// 서버 유저 정보 가져 오기
       ////var Header = Util.MakeHeaderData(UserContents.GetData);
       //// NetworkManager.Instance.SendContentsPacket(ContentsType.User, Header);

        // 타이틀
        UIManager.Instance.Open<TitleWindow>(UI.Main, "Prefabs/UI/Window/TitleWindow");
    }
}