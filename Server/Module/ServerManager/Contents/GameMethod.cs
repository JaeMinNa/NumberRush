using GameServer.Module.ServerManager.DataBase;
using MongoDB.Bson;

namespace GameServer.Module.ServerManager.Contents
{
    public class GameMethod
    {
        private static async Task<UserData_Game> CreateUserGameData(string accountCode)
        {
            UserData_Game uInfo = new UserData_Game();
            uInfo.AccountCode = accountCode;
            uInfo.Score = 0;
            uInfo.Gold = 0;
            uInfo.Time = 0f;

            await ServerDataBase.SetUserGameData(accountCode, uInfo);
            return uInfo;
        }

        private static async Task<UserData_Game> CheckInvalidData(string accountCode)
        {
            var data = await ServerDataBase.GetUserGameData(accountCode);

            bool isModify = false;
            for (eUserData_Game count = eUserData_Game.AccountCode; count < eUserData_Game.Max; ++count)
            {
                switch (count)
                {
                    case eUserData_Game.Score:
                        break;

                    case eUserData_Game.Gold:
                        break;

                    case eUserData_Game.Time:
                        break;

                    default:
                        break;
                }
            }

            if (isModify)
                await ServerDataBase.SetUserGameData(accountCode, data);

            return data;
        }

        // 게임 접속 시 계정 코드 가져오기
        public static async Task<UserData_Game> GetUserGameDataToConnect(string accountCode)
        {
            if (await ServerDataBase.IsExistGameData(accountCode))
            {
                UserData_Game uInfo = await CheckInvalidData(accountCode);

                await ServerDataBase.SetUserGameData(accountCode, uInfo);
                return uInfo;
            }
            else
            {
                return await CreateUserGameData(accountCode);
            }
        }

        public static async Task<UserData_Game> GetUserGameData(string accountCode)
        {
            return await ServerDataBase.GetUserGameData(accountCode);
        }

        public static async Task<T> GetUserGameData<T>(string uid, eUserData_Game type)
        {
            return await ServerDataBase.GetUserGameData<T>(uid, type);
        }

        public static async Task<bool> IsExistUserGameData(string accountCode)
        {
            return await ServerDataBase.IsExistGameData(accountCode);
        }

        public static async Task<List<UserData_Game>> GetUserGameDatas(List<string> accountCodes)
        {
            return await ServerDataBase.GetUserGameDatas(accountCodes);
        }

        public static async Task<Tuple<PacketBody, string>> ProcessUserGameAsync(string accountCode, UserData_Game data)
        {
            await ServerDataBase.SetUserGameData(accountCode, data);
            var resultPacketBody = ServerUtil.MakeBodyData(ReceiveType.UpdateUserGameData, eUserData_Game.Max, ConvertData(eUserData_Game.Max, data));
            return new Tuple<PacketBody, string>(resultPacketBody, "UpdateGame");
        }

        public static async Task<Tuple<PacketBody, LogData>> ProcessUserGameData(string accountCode, UserData_Game data)
        {
            var BeforeData = ServerDataBase.GetUserGameData(accountCode);
            LogData ResultLog = ServerUtil.MakeLogData(eUserData_Game.Max, string.Empty, BeforeData.ToJson(), data.ToJson());
            await ServerDataBase.SetUserGameData(accountCode, data);

            var ResultPacketBody = ServerUtil.MakeBodyData(ReceiveType.UpdateUserGameData, eUserData_Game.Max, ConvertData(eUserData_Game.Max, data));
            return new Tuple<PacketBody, LogData>(ResultPacketBody, ResultLog);
        }

        //public static async Task<Tuple<PacketBody, LogData>> ProcessUserCommonData<T>(string AccountCode, eUserData_Common Type, T Data)
        //{
        //    var BeforeData = ServerDataBase.GetUserCommonData<T>(AccountCode, Type);
        //    LogData ResultLog = ServerUtil.MakeLogData(Type, string.Empty, BeforeData.ToJson(), Data.ToJson());
        //    await ServerDataBase.SetUserCommonData(AccountCode, Type, Data);

        //    var ResultPacketBody = ServerUtil.MakeBodyData(ReceiveType.UpdateUserCommonData, Type, ConvertData(Type, Data));
        //    return new Tuple<PacketBody, LogData>(ResultPacketBody, ResultLog);
        //}

        private static string ConvertData(eUserData_Game type, object data)
        {
            switch (type)
            {
                case eUserData_Game.AccountCode:
                    return $"{data}";

                case eUserData_Game.Score:
                    return ServerUtil.ToJson(data);

                case eUserData_Game.Gold:
                    return ServerUtil.ToJson(data);

                case eUserData_Game.Time:
                    return ServerUtil.ToJson(data);

                case eUserData_Game.Max:
                    return ServerUtil.ToJson(data);

                default:
                    return string.Empty;
            }
        }
    }
}