using GameServer.Module.ServerManager.Contents;
using System.Net.Mime;
using System.Net.Sockets;

namespace GameServer.Module.ServerManager.Processors
{
    public partial class Processor
    {
        public static async Task<Tuple<PacketState, string>> ProcessPacket_User(string accountCode, GamePacket packetData)
        {
            PacketState packetState = PacketState.None;
            string result = string.Empty;

            if (packetData.contentsType != ContentsType.User)
            {
                packetState = PacketState.UnknownPacket;
                result = ServerUtil.MakeUnkownErrorData(PacketType.ContentsPacket, ContentsType.User, packetData.contentsType, packetData.HeaderData.ContentsIndex);
                return new Tuple<PacketState, string>(packetState, result);
            }

            PacketHeader outHeaderData = null;
            List<PacketBody> outBodyData = [];
            List<LogData> outLogData = [];
            UserContents type = (UserContents)packetData.HeaderData.ContentsIndex;
            ErrorResponseBuilder errorResponse = ErrorResponseBuilder.Make(packetData, outHeaderData!, outBodyData).SetContentsType(type);

            switch (type)
            {
                case UserContents.ChangeNickName:
                    {
                        // 닉네임 형식 검사
                        string newNickName = packetData.HeaderData.Data;

                        bool isEmpty = string.IsNullOrEmpty(newNickName);
                        if (isEmpty)
                            return await errorResponse.SetCode(0).BuildAsync();

                        // 이미 닉네임을 사용하고 있는지 확인
                        bool isExist = await UserMethod.CheckNickName(newNickName);
                        if (!isExist)
                            return await errorResponse.SetCode(1).SetMessage("Nickname_Duplicate").BuildAsync();

                        var userCommonData = await UserMethod.GetUserCommonData(accountCode);
                        userCommonData.NickName = newNickName;

                        var updateUserCommonData = await UserMethod.ProcessUserCommonData(accountCode, userCommonData);
                        outBodyData.Add(updateUserCommonData.Item1);
                        outLogData.Add(updateUserCommonData.Item2);

                        outHeaderData = ServerUtil.MakeHeaderData(UserContents.ChangeNickName, true);
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }

                case UserContents.ChangeImageNumber:
                    {
                        string newImageNum = packetData.HeaderData.Data;

                        var userCommonData = await UserMethod.GetUserCommonData(accountCode);
                        userCommonData.ImageNum = newImageNum;

                        var updateUserCommonData = await UserMethod.ProcessUserCommonData(accountCode, userCommonData);
                        outBodyData.Add(updateUserCommonData.Item1);
                        outLogData.Add(updateUserCommonData.Item2);

                        outHeaderData = ServerUtil.MakeHeaderData(UserContents.ChangeImageNumber, true);
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }

                case UserContents.GetData:
                    {
                        var userCommonData = await UserMethod.GetUserCommonData(accountCode);

                        outHeaderData = ServerUtil.MakeHeaderData(UserContents.GetData, true, $"AccountCode : {userCommonData.AccountCode}, UID : {userCommonData.UID} ,NikcName : {userCommonData.NickName}");
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }

                case UserContents.GoldCheat:
                    {
                        var userGameData = await GameMethod.GetUserGameData(accountCode);

                        userGameData.Gold += 1000000;

                        var updateUserGameData = await GameMethod.ProcessUserGameData(accountCode, userGameData);
                        outBodyData.Add(updateUserGameData.Item1);
                        outLogData.Add(updateUserGameData.Item2);

                        outHeaderData = ServerUtil.MakeHeaderData(UserContents.GoldCheat, true);
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }

                case UserContents.GetRankData:
                    {
                        // 나의 랭킹 데이터
                        var myRankInfo = await UserMethod.GetUserRankInfo(accountCode);
                        if (myRankInfo == null)
                            return await errorResponse.SetCode(0).BuildAsync();

                        // 유저 랭킹 데이터
                        var usersRankInfo = await UserMethod.GetUsersRankInfo();
                        if (usersRankInfo == null)
                            return await errorResponse.SetCode(1).BuildAsync();

                        outHeaderData = ServerUtil.MakeHeaderData(UserContents.GetRankData, true, ServerUtil.MakeData(ServerUtil.ToJson(myRankInfo), ServerUtil.ToJson(usersRankInfo)));
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }
            }

            packetState = PacketState.UnknownPacket;
            result = ServerUtil.MakeUnkownErrorData(PacketType.ContentsPacket, ContentsType.User, packetData.contentsType, packetData.HeaderData.ContentsIndex);
            return new Tuple<PacketState, string>(packetState, result);
        }
    }
}