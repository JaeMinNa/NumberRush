using System.Collections.Generic;
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
                        UIManager.Instance.OpenSystemPopup(new MessageData { Type = PopupType.OkOnly, Message = "Success to change nickname." });
                    }
                    else
                    {
                        UIManager.Instance.OpenSystemPopup(new MessageData { Type = PopupType.OkOnly, Message = "Fail to change nicknaem." });
                    }
                }
                break;

            case UserContents.ChangeImageNumber:
                {
                    if (Success)
                    {
                        
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

            case UserContents.GoldCheat:
                {
                    if (Success)
                    {
                        
                    }
                }
                break;

            case UserContents.GetRankData:
                {
                    if (Success)
                    {
                        var datas = Data.Split("#");
                        var myRankInfo = Util.ToObjectJson<UserRankInfo>(datas[0]);
                        var usersRankInfo = Util.ToObjectJson<List<UserRankInfo>>(datas[1]);

                        UIManager.Instance.Open<Popup_Ranking>(UI.Popup, "Prefabs/UI/Popup/Popup_Ranking", new List<object>() { myRankInfo, usersRankInfo });
                    }
                }
                break;

            default:
                break;
        }
    }
}