using UnityEngine;

public partial class PacketSystem
{
    private static void Excute_User(PacketHeader headerData)
    {
        UserContents Type = (UserContents)headerData.ContentsIndex;
        bool Success = headerData.Success;
        string Data = headerData.Data;

        switch (Type)
        {
            case UserContents.ChangeNickName:
                {
                    if (Success)
                    {
                        //var Datas = Data.Split("#");
                        //var MyInviteRank = Util.ToObjectJson<InviteRankInfo>(Datas[0]);
                        //var InviteRankList = Util.ToObjectJson<List<InviteRankInfo>>(Datas[1]);
                        //UIManager.Instance.Open<Popup_Friend_Event>(UI.Popup, "UI/Popup/Popup_Friend_Event", new List<object>() { MyInviteRank, InviteRankList });

                        Debug.LogWarning($"NickName 변경 성공! : {User.UserCommonData.NickName}, UID : {User.UserCommonData.UID}, AccountCode : {User.UserCommonData.AccountCode}");
                    }
                }
                break;

            case UserContents.GetData:
                {
                    if (Success)
                    {
                        string Notice = Data;
                        Debug.LogWarning(Notice);
                    }
                }
                break;

            default:
                break;
        }
    }
}