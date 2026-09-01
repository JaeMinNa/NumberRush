using Cysharp.Threading.Tasks;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public partial class PacketSystem
{
    public static async UniTask ProcessPacket(string recvData, UnityAction recvAction)
    {
        GamePacket gamePacket = Util.ToObjectJson<GamePacket>(recvData);

        if (gamePacket == null)
        {
            Debug.LogWarning("Packet is Null");
            return;
        }

        ProcessBodyData(gamePacket.BodyData);
        await ProcessHeaderData(gamePacket.contentsType, gamePacket.HeaderData);

        if (gamePacket.HeaderData.Success)
            recvAction?.Invoke();
    }

    // 바디 데이터
    public static void ProcessBodyData(List<PacketBody> bodyData)
    {
        if (bodyData == null || bodyData.Count == 0)
            return;

        for (int count = 0; count < bodyData.Count; ++count)
        {
            if (bodyData[count] == null)
                continue;

            ReceiveType receiveType = (ReceiveType)bodyData[count].ReceiveType;
            int receiveIndex = bodyData[count].ReceiveIndex;
            string data = bodyData[count].Data;

            switch (receiveType)
            {
                case ReceiveType.UpdateUserCommonData:
                    {
                        eUserData_Common Type = (eUserData_Common)receiveIndex;
                        switch (Type)
                        {
                            case eUserData_Common.NickName:
                                {
                                    User.UserCommonData.NickName = data;
                                }
                                break;

                            case eUserData_Common.AccountCode:
                                {
                                    User.UserCommonData.AccountCode = data;
                                }
                                break;

                            case eUserData_Common.UID:
                                {
                                    User.UserCommonData.UID = data;
                                }
                                break;

                            case eUserData_Common.ImageNum:
                                {
                                    User.UserCommonData.ImageNum = data;
                                }
                                break;

                            case eUserData_Common.Max:
                                {
                                    User.UserCommonData = Util.ToObjectJson<UserData_Common>(data);
                                }
                                break;
                        }
                    }
                    break;

                case ReceiveType.UpdateUserNumberData:
                    {
                        eUserData_Number Type = (eUserData_Number)receiveIndex;
                        switch (Type)
                        {
                            case eUserData_Number.AccountCode:
                            {
                                    User.UserNumberData.AccountCode = data;
                                }
                                break;

                            case eUserData_Number.NumberInventory:
                                {
                                    User.UserNumberData.NumberInventory = Util.ToObjectJson<List<int>>(data);
                                }
                                break;

                            case eUserData_Number.EquipNumber:
                                {
                                    User.UserNumberData.EquipNumber = Util.ToObjectJson<List<int>>(data);
                                }
                                break;

                            case eUserData_Number.Max:
                                {
                                    User.UserNumberData = Util.ToObjectJson<UserData_Number>(data);
                                }
                                break;
                        }
                    }
                    break;

                case ReceiveType.UpdateUserGameData:
                    {
                        eUserData_Game Type = (eUserData_Game)receiveIndex;
                        switch (Type)
                        {
                            case eUserData_Game.AccountCode:
                                {
                                    User.UserGameData.AccountCode = data;
                                }
                                break;

                            case eUserData_Game.Score:
                                {
                                    User.UserGameData.Score = Util.ToObjectJson<int>(data);
                                }
                                break;

                            case eUserData_Game.Gold:
                                {
                                    User.UserGameData.Gold = Util.ToObjectJson<int>(data);
                                }
                                break;

                            case eUserData_Game.Time:
                                {
                                    User.UserGameData.Time = Util.ToObjectJson<int>(data);
                                }
                                break;

                            case eUserData_Game.Max:
                                {
                                    User.UserGameData = Util.ToObjectJson<UserData_Game>(data);
                                }
                                break;
                        }
                    }
                    break;

                case ReceiveType.Max:
                    break;
            }
        }
    }

    // 헤더 데이터
    private static async UniTask ProcessHeaderData(ContentsType contentsType, PacketHeader headerData)
    {
        if (headerData == null)
            return;

        switch (contentsType)
        {
            case ContentsType.User:
                {
                    Excute_User(headerData);
                }
                break;

            case ContentsType.UserNumber:
                {
                    Excute_UserNumber(headerData);
                }
                break;

            case ContentsType.UserGame:
                {
                    Excute_UserGame(headerData);
                }
                break;
        }

        if (headerData.Success)
        {
            UIManager.Instance.Refresh();
        }
    }
}