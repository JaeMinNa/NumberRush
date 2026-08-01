using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json;

namespace GameServer.Module.ServerManager.DataBase
{
    public partial class ServerDataBase
    {
        #region Common Data
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

                    case eUserData_Common.Max:
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

        //public static async Task<List<CommonRankInfo>> GetUserCommonRankInfo()
        //{
        //    var AllCommonList = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(Builders<UserData_Common>.Filter.Empty).ToListAsync();
        //    var CommonList = AllCommonList.Where(Data => Data.PartyCombatPower > 0).OrderByDescending(Data => Data.PartyCombatPower).ThenByDescending(Data => Data.LastReinforceTime).ToList();
        //    int Range = CommonList.Count < 100 ? CommonList.Count : 100;
        //    var CommonListTop = CommonList.GetRange(0, Range);

        //    List<CommonRankInfo> Result = new List<CommonRankInfo>();

        //    for (int count = 0; count < CommonListTop.Count; count++)
        //    {
        //        CommonRankInfo Data = new CommonRankInfo();
        //        Data.Rank = count + 1;                
        //        Data.NickName = CommonListTop[count].NickName;
        //        Data.ProfileCharcater = CommonListTop[count].ProfileCharacter;
        //        Data.ProfileSkin = CommonListTop[count].ProfileSkin;
        //        Data.PartyCombatPower = CommonListTop[count].PartyCombatPower;
        //        Data.Lv = CommonListTop[count].AccountLevel;
        //        Data.Exp = CommonListTop[count].AccountExp;
        //        Data.CountryCode = CommonListTop[count].CountryCode;

        //        Result.Add(Data);
        //    }
        //    return Result;
        //}

        //public static async Task<CommonRankInfo> GetUserCommonRank(string AccountCode)
        //{
        //    var List = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(Builders<UserData_Common>.Filter.Empty).Limit(0).ToListAsync();
        //    var RankList = List.Where(Data => Data.PartyCombatPower != 0).OrderByDescending(Data => Data.PartyCombatPower).ThenByDescending(Data => Data.LastReinforceTime).ToList();

        //    int Rank = RankList.FindIndex(Data => Data.AccountCode == AccountCode);

        //    CommonRankInfo RankInfo = new CommonRankInfo();
        //    if(Rank == -1)
        //    {
        //        RankInfo.Rank = -1;
        //        RankInfo.PartyCombatPower = 0;
        //    }
        //    else
        //    {
        //        RankInfo.Rank = Rank + 1;
        //        RankInfo.PartyCombatPower = RankList[Rank].PartyCombatPower;
        //    }

        //    var UserCommonData = await UserMethod.GetUserCommonData(AccountCode);
        //    RankInfo.NickName = UserCommonData.NickName;
        //    RankInfo.ProfileCharcater = UserCommonData.ProfileCharacter;
        //    RankInfo.ProfileSkin = UserCommonData.ProfileSkin;
        //    RankInfo.Lv = UserCommonData.AccountLevel;
        //    RankInfo.Exp = UserCommonData.AccountExp;
        //    RankInfo.CountryCode = UserCommonData.CountryCode;

        //    return RankInfo;
        //}

        public static async Task<int> GetUserTotalCount()
        {
            var Result = await UserDB.GetCollection<UserData_Common>(nameof(UserCollection.UserCommonData)).Find(Builders<UserData_Common>.Filter.Empty).Limit(0).ToListAsync();
            return Result.Count;
        }
        #endregion
    }
}