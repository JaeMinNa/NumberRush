using GameServer.Module.ServerManager.Contents;
using System.Net.Mime;
using System.Net.Sockets;

namespace GameServer.Module.ServerManager.Processors
{
    public partial class Processor
    {
        public static async Task<Tuple<PacketState, string>> ProcessPacket_UserNumber(string accountCode, GamePacket packetData)
        {
            PacketState packetState = PacketState.None;
            string result = string.Empty;

            if (packetData.contentsType != ContentsType.UserNumber)
            {
                packetState = PacketState.UnknownPacket;
                result = ServerUtil.MakeUnkownErrorData(PacketType.ContentsPacket, ContentsType.UserNumber, packetData.contentsType, packetData.HeaderData.ContentsIndex);
                return new Tuple<PacketState, string>(packetState, result);
            }

            PacketHeader outHeaderData = null;
            List<PacketBody> outBodyData = [];
            List<LogData> outLogData = [];
            UserNumberContents type = (UserNumberContents)packetData.HeaderData.ContentsIndex;
            ErrorResponseBuilder errorResponse = ErrorResponseBuilder.Make(packetData, outHeaderData!, outBodyData).SetContentsType(type);

            switch (type)
            {
                case UserNumberContents.SetEquip:
                    {
                        string data = packetData.HeaderData.Data;
                        List<int> euqipNumber = ServerUtil.ToObjectJson<List<int>>(data);

                        var userNumberData = await NumberMethod.GetUserNumberData(accountCode);
                        userNumberData.EquipNumber = euqipNumber;

                        var updateUserNumberData = await NumberMethod.ProcessUserNumberData(accountCode, userNumberData);
                        outBodyData.Add(updateUserNumberData.Item1);
                        outLogData.Add(updateUserNumberData.Item2);
                    
                        outHeaderData = ServerUtil.MakeHeaderData(UserNumberContents.SetEquip, true);
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }

                case UserNumberContents.SetInventory:
                    {
                        string data = packetData.HeaderData.Data;
                        List<int> invenNumber = ServerUtil.ToObjectJson<List<int>>(data);

                        var userNumberData = await NumberMethod.GetUserNumberData(accountCode);
                        userNumberData.NumberInventory = invenNumber;

                        var updateUserNumberData = await NumberMethod.ProcessUserNumberData(accountCode, userNumberData);
                        outBodyData.Add(updateUserNumberData.Item1);
                        outLogData.Add(updateUserNumberData.Item2);

                        outHeaderData = ServerUtil.MakeHeaderData(UserNumberContents.SetInventory, true);
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