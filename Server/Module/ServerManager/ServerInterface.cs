using GameServer.Module.ServerManager.Contents;
using GameServer.Module.ServerManager.Processors;
using System.Net.Mime;
using System.Net.Sockets;

// 패킷을 받아서 처리하는 코드이다.
namespace GameServer.Module.ServerManager
{
    public class ServerInterface
    {
        // 최상단 호출 함수, 패킷의 타입을 분류하여 각자의 기능을 호출한다
        public static async Task<Tuple<PacketState, string>> ProcessPacket(string accountCode, ServerPacket packetData)
        {
            switch (packetData.PacketType)
            {
                case PacketType.GetUserData: { return await GetUserDataToConnect(accountCode); }

                case PacketType.ContentsPacket: { return await ProcessContentsPacket(accountCode, packetData.Data); }
            }

            return new Tuple<PacketState, string>(PacketState.UnknownPacket, ServerUtil.MakeUnkownErrorData(packetData.PacketType, default, default, 0));
        }

        //#region Title
        //private static async Task<Tuple<PacketState, string>> ProcessCheckUID(string UID)
        //{
        //    PacketState packetState = PacketState.None;
        //    string Result = string.Empty;

        //    var Account = await UserMethod.GetUserAccountData(UID);
        //    if (Account == null)
        //    {
        //        packetState = PacketState.InvalidUser;
        //        Result = string.Empty;
        //    }
        //    else
        //    {
        //        Result = Account.UID;
        //    }

        //    return new Tuple<PacketState, string>(packetState, Result);
        //}
        //#endregion

        #region Game
        // 유저가 게임에 접속할 때 호출한다, 유저 데이터를 검사해서 보내준다
        public static async Task<Tuple<PacketState, string>> GetUserDataToConnect(string accountCode)
        {
            PacketState packetState = PacketState.None;
            string result = string.Empty;
            if (string.IsNullOrEmpty(accountCode))
            {
                packetState = PacketState.InvalidUser;
                return new Tuple<PacketState, string>(packetState, result);
            }

            // 최초 접속
            List<string> userDatas = new List<string>();

            // 유저 데이터
            for (UserCollection count = UserCollection.None; count < UserCollection.Max; ++count)
            {
                switch (count)
                {
                    case UserCollection.UserCommonData:
                        userDatas.Add($"{nameof(UserData_Common)}${ServerUtil.ToJson(await UserMethod.GetUserCommonDataToConnect(accountCode))}");
                        break;

                    case UserCollection.UserNumberData:
                        userDatas.Add($"{nameof(UserData_Number)}${ServerUtil.ToJson(await NumberMethod.GetUserNumberDataToConnect(accountCode))}");
                        break;

                    case UserCollection.UserGameData:
                        userDatas.Add($"{nameof(UserData_Game)}${ServerUtil.ToJson(await GameMethod.GetUserGameDataToConnect(accountCode))}");
                        break;

                    default:
                        break;
                }
            }

            packetState = PacketState.None;
            result = ServerUtil.MakeData("&", userDatas);
            return new Tuple<PacketState, string>(packetState, result);
        }

        // 서버의 시간을 유저에게 보내준다, 유저는 이 시간으로 게임 내 시간 컨텐츠를 제어한다
        public static Tuple<PacketState, string> GetServerTime()
        {
            string retVal = ServerUtil.DateTimeNow.ToString("yyyy/MM/dd HH:mm:ss");
            return new Tuple<PacketState, string>(PacketState.None, retVal);
        }

        // 컨텐츠 패킷 전용 함수, 각 컨텐츠에 맞는 기능을 호출한다
        private static async Task<Tuple<PacketState, string>> ProcessContentsPacket(string accountCode, string data)
        {
            // 계정 코드 체크
            if (await CheckAccountCode(accountCode) == false)
            {
                return new Tuple<PacketState, string>(PacketState.InvalidUser, string.Empty);
            }

            //// 인증키 체크
            //if (await CheckAuthKey(AccountCode, AuthKey) == false)
            //{
            //    return new Tuple<PacketState, string>(PacketState.AuthKeyError, string.Empty);
            //}

            GamePacket PacketData = ServerUtil.ToObjectJson<GamePacket>(data);

            switch (PacketData.contentsType)
            {
                case ContentsType.User:
                    {
                        return await Processor.ProcessPacket_User(accountCode, PacketData);
                    }
                case ContentsType.UserNumber:
                    {
                        return await Processor.ProcessPacket_UserNumber(accountCode, PacketData);
                    }
                case ContentsType.UserGame:
                    {
                        return await Processor.ProcessPacket_UserGame(accountCode, PacketData);
                    }
            }

            return new Tuple<PacketState, string>(PacketState.UnknownPacket, string.Empty);
        }
        #endregion

        // 계정코드를 검사한다
        public static async Task<bool> CheckAccountCode(string accountCode)
        {
            // 플랫폼 ID가 없음
            if (string.IsNullOrEmpty(accountCode))
                return false;

            // 계정 코드가 없음
            return await UserMethod.IsExistUserCommonData(accountCode);
        }

        //// 접속할 때 발급한 인증키를 검사한다
        //public static async Task<bool> CheckAuthKey(string AccountCode, string AuthKey)
        //{
        //    // 인증키가 다름
        //    if (string.IsNullOrEmpty(AuthKey) || await UserMethod.GetUserCommonData<string>(AccountCode, eUserData_Common.AuthKey) != AuthKey)
        //        return false;

        //    return true;
        //}
    }
}