using GameServer.Module.ServerManager.DataBase;
using MongoDB.Bson;

namespace GameServer.Module.ServerManager.Contents
{
    public class NumberMethod
    {
        private static async Task<UserData_Number> CreateUserNumberData(string accountCode)
        {
            UserData_Number uInfo = new UserData_Number();
            uInfo.AccountCode = accountCode;
            uInfo.NumberInventory = new List<int>();
            uInfo.EquipNumber = new List<int>();

            await ServerDataBase.SetUserNumberData(accountCode, uInfo);
            return uInfo;
        }

        private static async Task<UserData_Number> CheckInvalidData(string accountCode)
        {
            var data = await ServerDataBase.GetUserNumberData(accountCode);

            bool isModify = false;
            for (eUserData_Number count = eUserData_Number.AccountCode; count < eUserData_Number.Max; ++count)
            {
                switch (count)
                {
                    case eUserData_Number.NumberInventory:
                        break;

                    case eUserData_Number.EquipNumber:
                        break;

                    default:
                        break;
                }
            }

            if (isModify)
                await ServerDataBase.SetUserNumberData(accountCode, data);

            return data;
        }

        // 게임 접속 시 계정 코드 가져오기
        public static async Task<UserData_Number> GetUserNumberDataToConnect(string accountCode)
        {
            if (await ServerDataBase.IsExistNumberData(accountCode))
            {
                UserData_Number uInfo = await CheckInvalidData(accountCode);

                await ServerDataBase.SetUserNumberData(accountCode, uInfo);
                return uInfo;
            }
            else
            {
                return await CreateUserNumberData(accountCode);
            }
        }

        public static async Task<UserData_Number> GetUserNumberData(string accountCode)
        {
            return await ServerDataBase.GetUserNumberData(accountCode);
        }

        public static async Task<T> GetUserNumberData<T>(string uid, eUserData_Number type)
        {
            return await ServerDataBase.GetUserNumberData<T>(uid, type);
        }

        public static async Task<bool> IsExistUserNumberData(string accountCode)
        {
            return await ServerDataBase.IsExistNumberData(accountCode);
        }

        public static async Task<List<UserData_Number>> GetUserNumberDatas(List<string> accountCodes)
        {
            return await ServerDataBase.GetUserNumberDatas(accountCodes);
        }

        public static async Task<Tuple<PacketBody, string>> ProcessUserNumberAsync(string accountCode, UserData_Number data)
        {
            await ServerDataBase.SetUserNumberData(accountCode, data);
            var resultPacketBody = ServerUtil.MakeBodyData(ReceiveType.UpdateUserNumberData, eUserData_Number.Max, ConvertData(eUserData_Number.Max, data));
            return new Tuple<PacketBody, string>(resultPacketBody, "UpdateNumber");
        }

        public static async Task<Tuple<PacketBody, LogData>> ProcessUserNumberData(string accountCode, UserData_Number data)
        {
            var BeforeData = ServerDataBase.GetUserNumberData(accountCode);
            LogData ResultLog = ServerUtil.MakeLogData(eUserData_Number.Max, string.Empty, BeforeData.ToJson(), data.ToJson());
            await ServerDataBase.SetUserNumberData(accountCode, data);

            var ResultPacketBody = ServerUtil.MakeBodyData(ReceiveType.UpdateUserNumberData, eUserData_Number.Max, ConvertData(eUserData_Number.Max, data));
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

        private static string ConvertData(eUserData_Number type, object data)
        {
            switch (type)
            {
                case eUserData_Number.AccountCode:
                    return $"{data}";

                case eUserData_Number.NumberInventory:
                    return ServerUtil.ToJson(data);

                case eUserData_Number.EquipNumber:
                    return ServerUtil.ToJson(data);

                case eUserData_Number.Max:
                    return ServerUtil.ToJson(data);

                default:
                    return string.Empty;
            }
        }
    }
}