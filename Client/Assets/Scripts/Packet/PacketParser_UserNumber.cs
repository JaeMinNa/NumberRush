using UnityEngine;

public partial class PacketSystem
{
    private static void Excute_UserNumber(PacketHeader headerData)
    {
        UserNumberContents Type = (UserNumberContents)headerData.ContentsIndex;
        bool Success = headerData.Success;
        string Data = headerData.Data;

        switch (Type)
        {
            case UserNumberContents.SetEquip:
                {
                    if (Success)
                    {
                        //var Datas = Data.Split("#");
                        //var MyInviteRank = Util.ToObjectJson<InviteRankInfo>(Datas[0]);
                        //var InviteRankList = Util.ToObjectJson<List<InviteRankInfo>>(Datas[1]);
                        //UIManager.Instance.Open<Popup_Friend_Event>(UI.Popup, "UI/Popup/Popup_Friend_Event", new List<object>() { MyInviteRank, InviteRankList });
                    }
                }
                break;

            case UserNumberContents.SetInventory:
                {
                    if (Success)
                    { 

                    }
                }
                break;

            default:
                break;
        }
    }
}