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

                case UserNumberContents.BuyOneNumber_Random:
                    {
                        var userGameData = await GameMethod.GetUserGameData(accountCode);
                        var userNumberData = await NumberMethod.GetUserNumberData(accountCode);

                        // Gold 충분한지 확인
                        if (userGameData.Gold < 2000)
                            return await errorResponse.SetCode(0).BuildAsync();

                        // Gold 소모
                        userGameData.Gold -= 2000;

                        // 0~99 중 랜덤 숫자 선택 (1~9 는 제외)
                        Random random = new Random();
                        int randomNumber;

                        do
                        {
                            randomNumber = random.Next(0, 100);
                        }
                        while (randomNumber >= 1 && randomNumber <= 9);

                        List<int> numList = new List<int>();
                        numList.Add(randomNumber);

                        // 가지고 있지 않은 숫자라면, 인벤토리 추가
                        if(!userNumberData.NumberInventory.Contains(randomNumber))
                            userNumberData.NumberInventory.Add(randomNumber);

                        // 저장
                        var updateUserGameData = await GameMethod.ProcessUserGameData(accountCode, userGameData);
                        outBodyData.Add(updateUserGameData.Item1);
                        outLogData.Add(updateUserGameData.Item2);

                        var updateUserNumberData = await NumberMethod.ProcessUserNumberData(accountCode, userNumberData);
                        outBodyData.Add(updateUserNumberData.Item1);
                        outLogData.Add(updateUserNumberData.Item2);

                        outHeaderData = ServerUtil.MakeHeaderData(UserNumberContents.BuyOneNumber_Random, true, ServerUtil.ToJson(numList));
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }

                case UserNumberContents.BuyTenNumber_Random:
                    {
                        var userGameData = await GameMethod.GetUserGameData(accountCode);
                        var userNumberData = await NumberMethod.GetUserNumberData(accountCode);

                        // Gold 충분한지 확인
                        if (userGameData.Gold < 18000)
                            return await errorResponse.SetCode(0).BuildAsync();

                        // Gold 소모
                        userGameData.Gold -= 18000;

                        // 0~99 중 랜덤 숫자 선택 (1~9 는 제외) x 10
                        List<int> numList = new List<int>();
                        for (int i = 0; i < 10; ++i)
                        {
                            Random random = new Random();
                            int randomNumber;

                            do
                            {
                                randomNumber = random.Next(0, 100);
                            }
                            while (randomNumber >= 1 && randomNumber <= 9);

                            numList.Add(randomNumber);

                            // 가지고 있지 않은 숫자라면, 인벤토리 추가
                            if (!userNumberData.NumberInventory.Contains(randomNumber))
                                userNumberData.NumberInventory.Add(randomNumber);
                        }

                        // 저장
                        var updateUserGameData = await GameMethod.ProcessUserGameData(accountCode, userGameData);
                        outBodyData.Add(updateUserGameData.Item1);
                        outLogData.Add(updateUserGameData.Item2);

                        var updateUserNumberData = await NumberMethod.ProcessUserNumberData(accountCode, userNumberData);
                        outBodyData.Add(updateUserNumberData.Item1);
                        outLogData.Add(updateUserNumberData.Item2);

                        outHeaderData = ServerUtil.MakeHeaderData(UserNumberContents.BuyOneNumber_Random, true, ServerUtil.ToJson(numList));
                        result = await ServerUtil.MakePacket(packetData.contentsType, outHeaderData, outBodyData);
                        return new Tuple<PacketState, string>(packetState, result);
                    }

                case UserNumberContents.BuyOneNumber_Select:
                    {
                        string data = packetData.HeaderData.Data;
                        int selectNum = ServerUtil.ToObjectJson<int>(data);

                        var userGameData = await GameMethod.GetUserGameData(accountCode);
                        var userNumberData = await NumberMethod.GetUserNumberData(accountCode);

                        // Gold 충분한지 확인
                        if (userGameData.Gold < 100000)
                            return await errorResponse.SetCode(0).BuildAsync();

                        // Gold 소모
                        userGameData.Gold -= 100000;

                        List<int> numList = new List<int>();
                        numList.Add(selectNum);

                        // 가지고 있지 않은 숫자라면, 인벤토리 추가
                        if (!userNumberData.NumberInventory.Contains(selectNum))
                            userNumberData.NumberInventory.Add(selectNum);

                        // 저장
                        var updateUserGameData = await GameMethod.ProcessUserGameData(accountCode, userGameData);
                        outBodyData.Add(updateUserGameData.Item1);
                        outLogData.Add(updateUserGameData.Item2);

                        var updateUserNumberData = await NumberMethod.ProcessUserNumberData(accountCode, userNumberData);
                        outBodyData.Add(updateUserNumberData.Item1);
                        outLogData.Add(updateUserNumberData.Item2);

                        outHeaderData = ServerUtil.MakeHeaderData(UserNumberContents.BuyOneNumber_Random, true, ServerUtil.ToJson(numList));
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