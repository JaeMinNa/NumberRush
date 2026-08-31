using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json;

namespace GameServer.Module.ServerManager.DataBase
{
    public partial class ServerDataBase
    {
        public static async Task DataBaseIndex_UserGame()
        {
            var collection = UserDB.GetCollection<UserData_Game>(nameof(UserCollection.UserGameData));
            var indexOptions = new CreateIndexOptions() { Background = true };
            var indexKey = Builders<UserData_Game>.IndexKeys.Ascending(Data => Data.AccountCode);
            var indexModel = new CreateIndexModel<UserData_Game>(indexKey, indexOptions);
            await collection.Indexes.CreateOneAsync(indexModel);
        }

        public static async Task<UserData_Game> GetUserGameData(string accountCode)
        {
            return await UserDB.GetCollection<UserData_Game>(nameof(UserCollection.UserGameData)).Find(uInfo => uInfo.AccountCode == accountCode).SingleOrDefaultAsync();
        }

        public static async Task<T> GetUserGameData<T>(string accountCode, eUserData_Game infoType)
        {
            var filter = Builders<UserData_Game>.Filter.Eq(Data => Data.AccountCode, accountCode);
            var projection = Builders<UserData_Game>.Projection.Include(infoType.ToString());
            var document = await UserDB.GetCollection<UserData_Game>(nameof(UserCollection.UserNumberData)).Find(filter).Project(projection).SingleOrDefaultAsync();

            if (document.Contains(infoType.ToString()))
                return BsonSerializer.Deserialize<T>(document[infoType.ToString()].ToJson());
            else
            {
                switch (infoType)
                {
                    case eUserData_Game.AccountCode:
                        return default;

                    case eUserData_Game.Score:
                        return default;

                    case eUserData_Game.Gold:
                        return default;

                    case eUserData_Game.Time:
                        return default;

                    case eUserData_Game.Max:
                        return default;

                    default:
                        return default;
                }
            }
        }

        public static async Task<List<UserData_Game>> GetUserGameDatas(List<string> accountCodes)
        {
            var builder = Builders<UserData_Game>.Filter.In(Data => Data.AccountCode, accountCodes);
            return await UserDB.GetCollection<UserData_Game>(nameof(UserCollection.UserGameData)).Find(builder).ToListAsync();
        }

        public static async Task SetUserGameData(string AccountCode, UserData_Game data)
        {
            await UserDB.GetCollection<UserData_Game>(nameof(UserCollection.UserGameData)).ReplaceOneAsync(
                Builders<UserData_Game>.Filter.Eq(uInfo => uInfo.AccountCode, AccountCode),
                data,
                new ReplaceOptions() { IsUpsert = true });
        }

        public static async Task<bool> IsExistGameData(string accountCode)
        {
            long docCount = await UserDB.GetCollection<UserData_Game>(nameof(UserCollection.UserGameData)).Find(uData => uData.AccountCode == accountCode).CountDocumentsAsync();
            if (docCount > 0)
                return true;
            else
                return false;
        }
    }
}