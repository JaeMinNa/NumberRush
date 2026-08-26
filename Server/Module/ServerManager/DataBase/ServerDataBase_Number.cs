using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json;

namespace GameServer.Module.ServerManager.DataBase
{
    public partial class ServerDataBase
    {
        public static async Task DataBaseIndex_UserNumber()
        {
            var collection = UserDB.GetCollection<UserData_Number>(nameof(UserCollection.UserNumberData));
            var indexOptions = new CreateIndexOptions() { Background = true };
            var indexKey = Builders<UserData_Number>.IndexKeys.Ascending(Data => Data.AccountCode);
            var indexModel = new CreateIndexModel<UserData_Number>(indexKey, indexOptions);
            await collection.Indexes.CreateOneAsync(indexModel);
        }

        public static async Task<UserData_Number> GetUserNumberData(string accountCode)
        {
            return await UserDB.GetCollection<UserData_Number>(nameof(UserCollection.UserNumberData)).Find(uInfo => uInfo.AccountCode == accountCode).SingleOrDefaultAsync();
        }

        public static async Task<T> GetUserNumberData<T>(string accountCode, eUserData_Number infoType)
        {
            var filter = Builders<UserData_Number>.Filter.Eq(Data => Data.AccountCode, accountCode);
            var projection = Builders<UserData_Number>.Projection.Include(infoType.ToString());
            var document = await UserDB.GetCollection<UserData_Number>(nameof(UserCollection.UserNumberData)).Find(filter).Project(projection).SingleOrDefaultAsync();

            if (document.Contains(infoType.ToString()))
                return BsonSerializer.Deserialize<T>(document[infoType.ToString()].ToJson());
            else
            {
                switch (infoType)
                {
                    case eUserData_Number.AccountCode:
                        return default;

                    case eUserData_Number.NumberInventory:
                        return default;

                    case eUserData_Number.EquipNumber:
                        return default;

                    case eUserData_Number.Max:
                        return default;

                    default:
                        return default;
                }
            }
        }

        public static async Task<List<UserData_Number>> GetUserNumberDatas(List<string> accountCodes)
        {
            var builder = Builders<UserData_Number>.Filter.In(Data => Data.AccountCode, accountCodes);
            return await UserDB.GetCollection<UserData_Number>(nameof(UserCollection.UserCommonData)).Find(builder).ToListAsync();
        }

        public static async Task SetUserNumberData(string AccountCode, UserData_Number data)
        {
            await UserDB.GetCollection<UserData_Number>(nameof(UserCollection.UserNumberData)).ReplaceOneAsync(
                Builders<UserData_Number>.Filter.Eq(uInfo => uInfo.AccountCode, AccountCode),
                data,
                new ReplaceOptions() { IsUpsert = true });
        }

        public static async Task<bool> IsExistNumberData(string accountCode)
        {
            long docCount = await UserDB.GetCollection<UserData_Number>(nameof(UserCollection.UserNumberData)).Find(uData => uData.AccountCode == accountCode).CountDocumentsAsync();
            if (docCount > 0)
                return true;
            else
                return false;
        }
    }
}