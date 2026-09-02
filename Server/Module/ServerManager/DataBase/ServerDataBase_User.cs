using GameServer.Module.ServerManager.Contents;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameServer.Module.ServerManager.DataBase
{
    public partial class ServerDataBase
    {
        public static async Task DataBaseIndex_UserCommon()
        {
            var collection = UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData));
            var indexOptions = new CreateIndexOptions() { Background = true };
            var indexKey = Builders<UserData_Common>.IndexKeys.Ascending(Data => Data.AccountCode);
            var indexModel = new CreateIndexModel<UserData_Common>(indexKey, indexOptions);
            await collection.Indexes.CreateOneAsync(indexModel);
        }

        public static async Task<UserData_Common> GetUserCommonData(string accountCode)
        {
            return await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(uInfo => uInfo.AccountCode == accountCode).SingleOrDefaultAsync();
        }

        public static async Task<T> GetUserCommonData<T>(string accountCode, eUserData_Common infoType)
        {
            var filter = Builders<UserData_Common>.Filter.Eq(Data => Data.AccountCode, accountCode);
            var projection = Builders<UserData_Common>.Projection.Include(infoType.ToString());
            var document = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(filter).Project(projection).SingleOrDefaultAsync();

            if (document.Contains(infoType.ToString()))
                return BsonSerializer.Deserialize<T>(document[infoType.ToString()].ToJson());
            else
            {
                switch (infoType)
                {
                    case eUserData_Common.NickName:
                        return (T)Convert.ChangeType(string.Empty, typeof(T));

                    case eUserData_Common.AccountCode:
                        return default;

                    case eUserData_Common.UID:
                        return default;

                    case eUserData_Common.ImageNum:
                        return default;

                    default:
                        return default;
                }
            }
        }

        public static async Task<UserData_Common> GetUserCommonDataByNickName(string nickName)
        {
            var filter = Builders<UserData_Common>.Filter.Eq(uData => uData.NickName, nickName);
            return await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData))
                .Find(filter)
                .SingleOrDefaultAsync();
        }

        public static async Task<List<UserData_Common>> GetUserCommonDatas(List<string> accountCodes)
        {
            var builder = Builders<UserData_Common>.Filter.In(Data => Data.AccountCode, accountCodes);
            return await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(builder).ToListAsync();
        }

        public static async Task SetUserCommonData(string AccountCode, UserData_Common data)
        {
            await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).ReplaceOneAsync(
                Builders<UserData_Common>.Filter.Eq(uInfo => uInfo.AccountCode, AccountCode),
                data,
                new ReplaceOptions() { IsUpsert = true });
        }

        //public static async Task SetUserCommonData<T>(string accountCode, eUserData_Common infoType, T data)
        //{
        //    var Filter = Builders<UserData_Common>.Filter.Eq(uData => uData.NickName, accountCode);
        //    await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).UpdateOneAsync(
        //        Filter,
        //        Builders<UserData_Common>.Update.Set(infoType.ToString(), data),
        //        new UpdateOptions() { IsUpsert = true });
        //}

        public static async Task<bool> IsExistCommonData(string accountCode)
        {
            long docCount = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(uData => uData.AccountCode == accountCode).CountDocumentsAsync();
            if (docCount > 0)
                return true;
            else
                return false;
        }

        public static async Task<bool> CheckNickName(string nickName)
        {
            var filter = Builders<UserData_Common>.Filter.Eq(uData => uData.NickName, nickName);
            var nickNameData = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(filter).SingleOrDefaultAsync();
            return nickNameData == null;
        }

        public static async Task<List<UserRankInfo>> GetUsersRankInfo()
        {
            // 전체 유저 기본 정보 가져오기
            var commonList = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(Builders<UserData_Common>.Filter.Empty).ToListAsync();

            // 랭킹 계산용 리스트
            var rankList = new List<(UserData_Common CommonData, int Score, float Time)>();

            foreach (var commonData in commonList)
            {
                // 해당 유저의 게임 데이터 가져오기
                var gameData = await GameMethod.GetUserGameData(commonData.AccountCode);

                if (gameData == null)
                    continue;

                rankList.Add((commonData, gameData.Score, gameData.Time));
            }

            // Score 높은 순서, Time 높은 순서로 정렬
            var sortedList = rankList.OrderByDescending(data => data.Score).ThenByDescending(data => data.Time).Take(50).ToList();

            var result = new List<UserRankInfo>();
            for (int i = 0; i < sortedList.Count; i++)
            {
                var data = sortedList[i];

                UserRankInfo rankInfo = new UserRankInfo();

                rankInfo.Rank = i + 1;
                rankInfo.NickName = data.CommonData.NickName;
                rankInfo.Score = data.Score;
                rankInfo.Time = data.Time;
                rankInfo.ImageNum = data.CommonData.ImageNum;

                // 유저의 Number 데이터
                var userNumberData = await NumberMethod.GetUserNumberData(data.CommonData.AccountCode);
                if (userNumberData != null)
                    rankInfo.EquipNumber = userNumberData.EquipNumber;

                result.Add(rankInfo);
            }

            return result;
        }

        public static async Task<UserRankInfo> GetUserRankInfo(string accountCode)
        {
            // 전체 유저 기본 정보 가져오기
            var commonList = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(Builders<UserData_Common>.Filter.Empty).ToListAsync();

            // 랭킹 계산용 리스트
            var rankList = new List<(UserData_Common CommonData, int Score, float Time)>();

            foreach (var commonData in commonList)
            {
                // 게임 데이터 가져오기
                var gameData = await GameMethod.GetUserGameData(commonData.AccountCode);

                if (gameData == null)
                    continue;

                rankList.Add((commonData, gameData.Score, gameData.Time));
            }

            // Score 높은 순, Time 높은 순으로 정렬
            var sortedList = rankList.OrderByDescending(data => data.Score).ThenByDescending(data => data.Time).ToList();

            // 해당 유저 위치 검색
            int rank = sortedList.FindIndex(data => data.CommonData.AccountCode == accountCode);

            UserRankInfo rankInfo = new UserRankInfo();

            // 유저 기본 데이터
            var userCommonData = await GetUserCommonData(accountCode);
            if (userCommonData != null)
            {
                rankInfo.NickName = userCommonData.NickName;
                rankInfo.ImageNum = userCommonData.ImageNum;
            }

            // 유저의 Number 데이터
            var userNumberData = await NumberMethod.GetUserNumberData(accountCode);
            if (userNumberData != null)
            {
                rankInfo.EquipNumber = userNumberData.EquipNumber;
            }

            // 랭킹에 없는 경우
            if (rank == -1)
            {
                rankInfo.Rank = -1;

                var userGameData = await GameMethod.GetUserGameData(accountCode);
                rankInfo.Score = userGameData != null ? userGameData.Score : 0;
                rankInfo.Time = userGameData != null ? userGameData.Time : 0f;
            }
            // 랭킹에 있는 경우
            else
            {
                rankInfo.Rank = rank + 1;
                rankInfo.Score = sortedList[rank].Score;
                rankInfo.Time = sortedList[rank].Time;
            }

            return rankInfo;
        }

        public static async Task<int> GetUserTotalCount()
        {
            var Result = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(Builders<UserData_Common>.Filter.Empty).Limit(0).ToListAsync();
            return Result.Count;
        }
    }
}