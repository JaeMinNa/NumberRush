using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;

// 서버 데이터 베이스 클래스 기본
namespace GameServer.Module.ServerManager.DataBase
{
    public partial class ServerDataBase
    {
        // DB Client
        private static IMongoClient m_ServiceClient;

        // DB를 접속한다
        public static async Task Init(string connectString)
        {
            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

            m_ServiceClient = new MongoClient(connectString);

            // DB 접속 후 데이터를 빠르게 가져오기 위해 인덱싱한다
            for (UserCollection count = UserCollection.None; count < UserCollection.Max; ++count)
            {
                switch (count)
                {
                    // User 
                    case UserCollection.UserCommonData: await DataBaseIndex_UserCommon(); break;
                    case UserCollection.UserNumberData: await DataBaseIndex_UserNumber(); break;
                    case UserCollection.UserGameData: await DataBaseIndex_UserGame(); break;
                    default:
                        break;
                }
            }
        }

        // 유저 DB
        private static IMongoDatabase UserDB
        {
            get
            {
                return m_ServiceClient.GetDatabase(nameof(DataBaseKey.User));
            }
        }
    }
}