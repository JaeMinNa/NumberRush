using GameServer.Module.ServerManager.DataBase;
using MongoDB.Bson;

namespace GameServer.Module.ServerManager.Contents
{
    public class UserMethod
    {
        private static async Task<UserData_Common> CreateUserCommonData(string accountCode)
        {
            UserData_Common uInfo = new UserData_Common();
            uInfo.AccountCode = accountCode;
            uInfo.UID = ObjectId.GenerateNewId().ToString();
            uInfo.NickName = ObjectId.GenerateNewId().ToString()[..8];  // 8글자 제한
            uInfo.ImageNum = "01";

            await ServerDataBase.SetUserCommonData(accountCode, uInfo);
            return uInfo;
        }

        private static async Task<UserData_Common> CheckInvalidData(string accountCode)
        {
            var data = await ServerDataBase.GetUserCommonData(accountCode);

            bool isModify = false;
            for (eUserData_Common count = eUserData_Common.AccountCode; count < eUserData_Common.Max; ++count)
            {
                switch (count)
                {
                    case eUserData_Common.NickName:
                        {
                            if (string.IsNullOrEmpty(data.NickName))
                            {
                                data.NickName = await CreateNickName();
                                isModify = true;
                            }
                        }
                        break;

                    case eUserData_Common.AccountCode:
                        break;

                    case eUserData_Common.UID:
                        break;

                    case eUserData_Common.ImageNum:
                        break;

                    default:
                        break;
                }
            }

            if (isModify)
                await ServerDataBase.SetUserCommonData(accountCode, data);

            return data;
        }

        // 게임 접속 시 계정 코드 가져오기
        public static async Task<UserData_Common> GetUserCommonDataToConnect(string accountCode)
        {
            if (await ServerDataBase.IsExistCommonData(accountCode))
            {
                UserData_Common uInfo = await CheckInvalidData(accountCode);

                await ServerDataBase.SetUserCommonData(accountCode, uInfo);
                return uInfo;
            }
            else
            {
                return await CreateUserCommonData(accountCode);
            }
        }

        public static async Task<UserData_Common> GetUserCommonData(string accountCode)
        {
            return await ServerDataBase.GetUserCommonData(accountCode);
        }

        public static async Task<UserData_Common> GetUserCommonDataByNickName(string nickName)
        {
            return await ServerDataBase.GetUserCommonDataByNickName(nickName);
        }

        public static async Task<T> GetUserCommonData<T>(string uid, eUserData_Common type)
        {
            return await ServerDataBase.GetUserCommonData<T>(uid, type);
        }

        public static async Task<bool> IsExistUserCommonData(string accountCode)
        {
            return await ServerDataBase.IsExistCommonData(accountCode);
        }

        public static async Task<List<UserData_Common>> GetUserCommonDatas(List<string> accountCodes)
        {
            return await ServerDataBase.GetUserCommonDatas(accountCodes);
        }

        public static async Task<Tuple<PacketBody, string>> ProcessUserCommonAsync(string accountCode, UserData_Common data)
        {
            await ServerDataBase.SetUserCommonData(accountCode, data);
            var resultPacketBody = ServerUtil.MakeBodyData(ReceiveType.UpdateUserCommonData, eUserData_Common.Max, ConvertData(eUserData_Common.Max, data));
            return new Tuple<PacketBody, string>(resultPacketBody, "UpdateCommon");
        }

        public static async Task<Tuple<PacketBody, LogData>> ProcessUserCommonData(string accountCode, UserData_Common data)
        {
            var BeforeData = ServerDataBase.GetUserCommonData(accountCode);
            LogData ResultLog = ServerUtil.MakeLogData(eUserData_Common.Max, string.Empty, BeforeData.ToJson(), data.ToJson());
            await ServerDataBase.SetUserCommonData(accountCode, data);

            var ResultPacketBody = ServerUtil.MakeBodyData(ReceiveType.UpdateUserCommonData, eUserData_Common.Max, ConvertData(eUserData_Common.Max, data));
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

        private static string ConvertData(eUserData_Common type, object data)
        {
            switch (type)
            {
                case eUserData_Common.AccountCode:
                    return $"{data}";

                case eUserData_Common.Max:
                    return ServerUtil.ToJson(data);

                default:
                    return string.Empty;
            }
        }

        public static async Task<string> CreateNickName()
        {
            Random random = new Random();
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            List<string> stringList = new List<string>();
            List<string> returnList = new List<string>();
            for (int count = 0; count < 20; count++)
            {
                string Code = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
                stringList.Add(Code);
            }

            for (int count = 0; count < 20; count++)
            {
                if (await ServerDataBase.CheckNickName(stringList[count]))
                {
                    returnList.Add(stringList[count]);
                }
            }
            if (returnList.Count == 0)
                return await CreateNickName();
            else
            {
                return returnList[0];
            }
        }

        public static async Task<bool> CheckNickName(string nickName)
        {
            return await ServerDataBase.CheckNickName(nickName);
        }

        public static async Task<List<UserRankInfo>> GetUsersRankInfo()
        {
            return await ServerDataBase.GetUsersRankInfo();
        }

        public static async Task<UserRankInfo> GetUserRankInfo(string AccountCode)
        {
            return await ServerDataBase.GetUserRankInfo(AccountCode);
        }

        public static async Task<int> GetUserTotalCount()
        {
            return await ServerDataBase.GetUserTotalCount();
        }
    }
}