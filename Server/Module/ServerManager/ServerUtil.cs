using Newtonsoft.Json;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// 서버에서 공통적으로 사용되는 유틸 함수 모음이다
namespace GameServer.Module.ServerManager
{
    public static class ServerUtil
    {
        // 패킷을 컨테이너를 만든다, ServerPacket 컨테이너 안에 string으로 치환되어 들어간다
        public static async Task<string> MakePacket(ContentsType type, PacketHeader head, List<PacketBody> body)
        {
            GamePacket gamePacket = new GamePacket()
            {
                contentsType = type,                                            // 컨텐츠 타입 (예 - 메인퀘스트 컨텐츠)
                HeaderData = head,                                              // 헤더 데이터 (예 - 아래의 헤더를 참고)
                BodyData = body,                                                // 바디 데이터 (예 - 아래의 바디를 참고)
            };

            return ToJson(gamePacket);
        }

        // GamePacket의 헤더 데이터에 포함될 데이터를 만든다
        public static PacketHeader MakeHeaderData(Enum subContentsType, bool success, string data = "")
        {
            PacketHeader packetHeader = new PacketHeader()
            {
                ContentsIndex = subContentsType.GetHashCode(),      // 컨텐츠 타입 하위의 인덱스 (예 - 메인퀘스트 승리)
                Success = success,                                  // 패킷처리 성공 여부
                Data = data,                                        // 데이터
            };

            return packetHeader;
        }

        // GamePacket의 바디 데이터에 포함될 데이터를 만든다
        public static PacketBody MakeBodyData(ReceiveType receiveType, Enum subReceiveType, string data = "")
        {
            PacketBody packetBody = new PacketBody()
            {
                ReceiveType = receiveType.GetHashCode(),            // 수신 후 처리할 타입 (예 - 유저재화 갱신)
                ReceiveIndex = subReceiveType.GetHashCode(),        // 수신 후 처리할 인덱스 (예 - 유저재화 중 골드)
                Data = data,                                        // 데이터 (예 - 10000으로 골드를 갱신)
            };

            return packetBody;
        }

        // 헤더, 바디의 string 데이터에 포함될 데이터를 만든다, (작업자의 성향에 따라 사용해도 되고 안해도 된다, 대신 클라이언트에서 제대로 파싱해야한다)
        public static string MakeData(string separator = "#", List<string> datas = null)
        {
            if (datas == null)
                return string.Empty;

            return string.Join(separator, datas.ToArray());
        }

        // 헤더, 바디의 string 데이터에 포함될 데이터를 만든다, (작업자의 성향에 따라 사용해도 되고 안해도 된다, 대신 클라이언트에서 제대로 파싱해야한다)
        public static string MakeData(string data1 = "", string data2 = "", string data3 = "", string data4 = "")
        {
            if (string.IsNullOrEmpty(data1))
                return string.Empty;

            if (string.IsNullOrEmpty(data2))
                return $"{data1}";

            if (string.IsNullOrEmpty(data3))
                return $"{data1}#{data2}";

            if (string.IsNullOrEmpty(data4))
                return $"{data1}#{data2}#{data3}";

            return $"{data1}#{data2}#{data3}#{data4}";
        }

        // 에러 메세지를 만든다(알 수 없는 에러)
        public static string MakeUnkownErrorData(PacketType PacketType, ContentsType ProcessType, ContentsType RecvType, int RecvSubType)
        {
            return $"-1{PacketType.GetHashCode().ToString():00}{ProcessType.GetHashCode().ToString():00}{RecvType.GetHashCode().ToString():00}{RecvSubType.ToString():00}";
        }

        public static string MakeServerErrorData(GamePacket GamePacket, int ErrorCode)
        {
            return $"-2{GamePacket.contentsType.GetHashCode().ToString():00}{GamePacket.HeaderData.ContentsIndex.ToString():00}{ErrorCode.ToString():00}";
        }

        // 에러 메세지를 만든다(서버 익셉션)
        public static string MakeServerExceptionData(PacketType PacketType, ContentsType contentsType, int RecvSubType)
        {
            return $"-3{PacketType.GetHashCode().ToString():00}{contentsType.GetHashCode().ToString():00}{RecvSubType.ToString():00}";
        }

        #region Json Parser
        // LitJsonParser (ObjectID가 들어갔을 경우 사용 불가)
        public static T ToObjectJson<T>(string jsonData)
        {
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public static bool TryToObjectJson<T>(string jsonData, out T jsonObject)
        {
            try
            {
                jsonObject = JsonConvert.DeserializeObject<T>(jsonData);
                return true;
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                Console.WriteLine(e.Message);
                jsonObject = default;
                return false;
            }
        }

        // 오브젝트 -> json으로 변환
        public static string ToJson(object jsonData)
        {
            return JsonConvert.SerializeObject(jsonData);
        }

        public static bool TryToJson(object jsonObject, out string jsonData)
        {
            try
            {
                jsonData = JsonConvert.SerializeObject(jsonObject);
                return true;
            }
            catch (Newtonsoft.Json.JsonException e)
            {
                Console.WriteLine(e.Message);
                jsonData = string.Empty;
                return false;
            }
        }
        #endregion

        #region Util
        // 몽고DB는 UTC 시간을 사용하기 때문에 9시간을 더해야 한국시간이 된다
        public static DateTime DateTimeNow
        {
            get
            {
                return DateTime.UtcNow.AddHours(9);
            }
        }

        public static long TimeNowUnixTime
        {
            get
            {
                return DateTimeOffset.UtcNow.AddHours(9).ToUnixTimeSeconds();
            }
        }

        // 등속도 운동처리된 위치를 시간에 따라 알아내어 리턴(Float 버전)
        public static float UniformVelocity(float fStartPos, float fEndPos, float nCurrentTime, float nMaxTime)
        {
            if (nCurrentTime >= nMaxTime)
                return fEndPos;

            if (nCurrentTime <= 0)
                return fStartPos;

            return fStartPos + nCurrentTime * (fEndPos - fStartPos) / nMaxTime;
        }

        public static int UniformVelocity(int fStartPos, int fEndPos, float nCurrentTime, float nMaxTime)
        {
            if (nCurrentTime >= nMaxTime)
                return fEndPos;

            if (nCurrentTime <= 0)
                return fStartPos;

            return (int)(fStartPos + nCurrentTime * (fEndPos - fStartPos) / nMaxTime);
        }

        public static long UniformVelocity(long fStartPos, long fEndPos, float nCurrentTime, float nMaxTime)
        {
            if (nCurrentTime >= nMaxTime)
                return fEndPos;

            if (nCurrentTime <= 0)
                return fStartPos;

            return (long)(fStartPos + nCurrentTime * (fEndPos - fStartPos) / nMaxTime);
        }

        // 확률 전용으로 특정 확률을 픽하기 위한 함수이다
        //public static int GetRandomRate(int MinValue, int MaxValue)
        //{
        //    return ServerRandom.Next(MinValue, MaxValue + 1);
        //}

        // Enum값을 string으로 변환한다
        public static string GetEnumFullName(Enum TargetEnum)
        {
            return $"{TargetEnum.GetType().Name}.{TargetEnum}";
        }

        // 서버에 기록될 로그 데이터를 만든다
        public static LogData MakeLogData(Enum TargetEnum, string AddType, string Before, string After)
        {
            LogData LogData = new LogData()
            {
                Type = GetEnumFullName(TargetEnum),
                AddType = AddType,
                Before = Before,
                After = After
            };

            return LogData;
        }

        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
        #endregion

        static byte[] Skey = ASCIIEncoding.ASCII.GetBytes("NoonChee");

        public static string Encrypt(string Data)
        {
            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();
            rc2.Mode = CipherMode.ECB;
            rc2.Padding = PaddingMode.PKCS7;
            rc2.Key = Skey;
            rc2.IV = Skey;

            MemoryStream ms = new MemoryStream();

            CryptoStream cryptoStream = new CryptoStream(ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);

            byte[] data = Encoding.UTF8.GetBytes(Data);

            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            byte[] encryptBytes = ms.ToArray();
            string encryptString = Convert.ToBase64String(encryptBytes);

            return encryptString;
        }

        public static string Decrypt(string Data)
        {
            using DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();
            rc2.Mode = CipherMode.ECB;
            rc2.Padding = PaddingMode.PKCS7;
            rc2.Key = Skey;
            rc2.IV = Skey;

            byte[] encryptedData = Convert.FromBase64String(Data);

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(ms, rc2.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(encryptedData, 0, encryptedData.Length);
            cryptoStream.FlushFinalBlock();

            // 여기서 메모리 스트림의 전체 데이터를 추출한 후, 필요한 부분만 가져옵니다.
            byte[] decryptedBytes = ms.ToArray();
            string decryptedString = Encoding.UTF8.GetString(decryptedBytes);
            return decryptedString.Replace("\0", string.Empty);
        }

        public static string StringCompress(string Data)
        {
            byte[] sourceArray = Encoding.UTF8.GetBytes(Data);

            using MemoryStream inputStream = new MemoryStream(sourceArray);
            using MemoryStream outputStream = new MemoryStream();
            using BrotliStream bs = new BrotliStream(outputStream, CompressionLevel.Optimal);

            inputStream.CopyTo(bs);
            bs.Flush();

            return Convert.ToBase64String(outputStream.ToArray());
        }

        public static string StringDecompress(string Data)
        {
            byte[] sourceArray = Convert.FromBase64String(Data);

            using MemoryStream inputStream = new MemoryStream(sourceArray);
            using MemoryStream outputStream = new MemoryStream();
            using BrotliStream bs = new BrotliStream(inputStream, CompressionMode.Decompress);

            bs.CopyTo(outputStream);
            outputStream.Flush();

            return Encoding.UTF8.GetString(outputStream.ToArray());
        }

        public static TimeSpan SecondsToTimeSpan(long Seconds)
        {
            int Days = (int)(Seconds / (24 * 3600));
            Seconds %= (24 * 3600);
            int Hours = (int)(Seconds / 3600);
            Seconds %= 3600;
            int Minutes = (int)(Seconds / 60);
            Seconds %= 60;
            return new TimeSpan(Days, Hours, Minutes, (int)Seconds);
        }

        //public static byte[] ToByteArray<T>(T data)
        //{
        //    return MessagePackSerializer.Serialize(data);
        //}

        //public static T ToObject<T>(byte[] data)
        //{
        //    return MessagePackSerializer.Deserialize<T>(data);
        //}

        private static readonly object TickLock = new object();
        public static long GetTick()
        {
            // 동시에 접근하지 못하도록 Lock을 걸어준다, 같은 시간이 나오지 않도록
            lock (TickLock)
            {
                return DateTimeNow.Ticks;
            }
        }

        //public static T GetRandomItemByProbability<T>(IEnumerable<T> items, IEnumerable<int> probabilities)
        //{
        //    if (items == null || !items.Any())
        //        throw new ArgumentException("아이템 목록이 null이거나 비어있습니다");
        //    if (probabilities == null || !probabilities.Any())
        //        throw new ArgumentException("확률 목록이 null이거나 비어있습니다");

        //    var itemsList = items.ToList();
        //    var probabilitiesList = probabilities.ToList();

        //    // 아이템과 확률의 개수가 일치하는지 확인
        //    if (itemsList.Count != probabilitiesList.Count)
        //        throw new ArgumentException("아이템과 확률의 개수가 일치하지 않습니다");

        //    // 전체 확률 계산
        //    int totalProbability = probabilitiesList.Sum();
        //    if (totalProbability <= 0)
        //        throw new ArgumentException("전체 확률은 0보다 커야 합니다");

        //    // 1부터 전체 확률 사이의 랜덤 값 생성
        //    int randomNumber = GetRandomRate(1, totalProbability);

        //    // 확률에 따라 아이템 선택
        //    int currentSum = 0;
        //    for (int i = 0; i < itemsList.Count; i++)
        //    {
        //        currentSum += probabilitiesList[i];
        //        if (randomNumber <= currentSum)
        //            return itemsList[i];
        //    }

        //    // 구현이 올바르다면 이 코드는 실행되지 않아야 함
        //    throw new InvalidOperationException("아이템 선택에 실패했습니다");
        //}

        //public static T WeightedRandomSelection<T>(this IEnumerable<T> Items, Func<T, int> WeightSelector)
        //{
        //    List<T> ItemList = Items.ToList();

        //    if (ItemList.Count == 0)
        //        throw new ArgumentException("아이템 목록이 비어있습니다");

        //    int TotalWeight = ItemList.Sum(WeightSelector);

        //    if (TotalWeight <= 0)
        //        throw new ArgumentException("가중치 합계는 0보다 커야 합니다");

        //    int RandomWeight = GetRandomRate(0, TotalWeight - 1);
        //    int CurrentWeight = 0;

        //    foreach (var Item in ItemList)
        //    {
        //        CurrentWeight += WeightSelector(Item);
        //        if (RandomWeight < CurrentWeight)
        //        {
        //            return Item;
        //        }
        //    }

        //    // 안전장치: 이 코드가 실행되지 않아야 함
        //    return ItemList.Last();
        //}

        //    public static T WeightedRandomSelectionFloat<T>(this IEnumerable<T> Items, Func<T, float> WeightSelector)
        //    {
        //        List<T> ItemList = Items.ToList();

        //        if (ItemList.Count == 0)
        //            throw new ArgumentException("아이템 목록이 비어있습니다");

        //        float TotalWeight = ItemList.Sum(WeightSelector);

        //        if (TotalWeight <= 0)
        //            throw new ArgumentException("가중치 합계는 0보다 커야 합니다");

        //        // 0에서 TotalWeight 사이의 부동소수점 난수 생성
        //        float RandomWeight = (float)ServerRandom.NextDouble() * TotalWeight;
        //        float CurrentWeight = 0f;

        //        foreach (var Item in ItemList)
        //        {
        //            CurrentWeight += WeightSelector(Item);
        //            if (RandomWeight < CurrentWeight)
        //            {
        //                return Item;
        //            }
        //        }

        //        // 안전장치: 부동소수점 연산 오차로 인해 드물게 도달할 수 있음
        //        return ItemList.Last();
        //    }

        //    public static bool GetRandomBool(float probability)
        //    {
        //        if (probability < 0.0f || probability > 1.0f)
        //            throw new ArgumentOutOfRangeException(nameof(probability), "확률은 0.0f에서 1.0f 사이의 값이어야 합니다.");

        //        // 0.0f 이상 1.0f 미만의 랜덤 값 생성
        //        float randomValue = (float)ServerRandom.NextDouble();

        //        // 확률이 1.0f이면 무조건 true 반환
        //        return randomValue < probability;
        //    }
    }
}