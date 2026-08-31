using GameServer.Module.ServerManager.Contents;
using System.Net.Mime;
using System.Net.Sockets;

namespace GameServer.Module.ServerManager.Processors
{
    public partial class Processor
    {
        public static async Task<Tuple<PacketState, string>> ProcessPacket_UserGame(string accountCode, GamePacket packetData)
        {
            PacketState packetState = PacketState.None;
            string result = string.Empty;

            if (packetData.contentsType != ContentsType.UserGame)
            {
                packetState = PacketState.UnknownPacket;
                result = ServerUtil.MakeUnkownErrorData(PacketType.ContentsPacket, ContentsType.UserGame, packetData.contentsType, packetData.HeaderData.ContentsIndex);
                return new Tuple<PacketState, string>(packetState, result);
            }

            PacketHeader outHeaderData = null;
            List<PacketBody> outBodyData = [];
            List<LogData> outLogData = [];
            UserGameContents type = (UserGameContents)packetData.HeaderData.ContentsIndex;
            ErrorResponseBuilder errorResponse = ErrorResponseBuilder.Make(packetData, outHeaderData!, outBodyData).SetContentsType(type);

            switch (type)
            {
                case UserGameContents.EndChapter:
                    {
                        string data = packetData.HeaderData.Data;
                        var datas = packetData.HeaderData.Data.Split("#");
                        int score = int.Parse(datas[0]);
                        int gold = int.Parse(datas[1]);
                        float time = float.Parse(datas[2]);

                        var userGameData = await GameMethod.GetUserGameData(accountCode);

                        // 최고 Score 갱신
                        if (userGameData.Score < score)
                            userGameData.Score = score;

                        // Gold 갱신
                        userGameData.Gold += gold;

                        // 최고 Time 갱신
                        if (userGameData.Time < time)
                            userGameData.Time = time;

                        // UserGameData 저장
                        var updateUserGameData = await GameMethod.ProcessUserGameData(accountCode, userGameData);
                        outBodyData.Add(updateUserGameData.Item1);
                        outLogData.Add(updateUserGameData.Item2);

                        outHeaderData = ServerUtil.MakeHeaderData(UserGameContents.EndChapter, true);
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }
            }

            packetState = PacketState.UnknownPacket;
            result = ServerUtil.MakeUnkownErrorData(PacketType.ContentsPacket, ContentsType.UserGame, packetData.contentsType, packetData.HeaderData.ContentsIndex);
            return new Tuple<PacketState, string>(packetState, result);
        }
    }
}