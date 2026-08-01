public enum ResponseType
{
    None,                   // 미정의
    Success,                // 성공

    ServerError,            // 서버 에러
    DuplicateRequest,       // 중복 요청
    UnknownError,           // 알 수 없는 에러
    ConnectionError,        // 연결 에러
    InvalidRequest,         // 잘못된 요청 (데이터 누락 및 형식 오류)
    GameDataError,          // 게임 데이터 에러
    UserDataError,          // 유저 데이터 에러
    InvalidUser,            // 비정상 유저
    RequireUpdate,          // 업데이트 필요
    VersionError,           // 버전 에러
    AuthKeyError,           // 인증키 에러
}

public enum MessageType
{
    None,

    Message,                // Gstr 사용 메세지
    ItemMessage,            // Gstr 사용 아이템 메세지
    Toast,                  // Gstr 사용 토스트
    MissionToast,           // 미션 토스트

    MessageWithKey,         // {0}{1}을 사용하는 Gstr 메세지
    ToastWithKey,           // {0}{1}을 사용하는 Gstr 토스트

    AccountLevelUp,

    Max
}

public enum MsgType
{
    None,
    Toast,
    Message,
}

public class TestResponse
{
    public ResponseType Type { get; set; } = ResponseType.UnknownError;
    public int Code { get; set; }
    public string Message { get; set; }

    public MsgType MsgType { get; set; }

    public string Data { get; set; }

    public List<PacketBody> Bodies { get; set; }

    private const string Separator = "#";

    public TestResponse SetData(params object[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentNullException(nameof(values));

        if (values.Any(x => x == null))
            throw new ArgumentException("Values cannot contain null", nameof(values));

        Data = string.Join(Separator, values);
        return this;
    }

    public TestResponse SetCode(int code)
    {
        Code = code;
        return this;
    }

    public TestResponse SetMessage(string message)
    {
        Message = message;
        return this;
    }

    public TestResponse SetType(ResponseType type)
    {
        Type = type;
        return this;
    }

    public TestResponse SetMsgType(MsgType msgType)
    {
        MsgType = msgType;
        return this;
    }

    public TestResponse AddBody(PacketBody body)
    {
        Bodies ??= new List<PacketBody>();
        Bodies.Add(body);
        return this;
    }

    public TestResponse AddBodies(List<PacketBody> bodies)
    {
        if (bodies == null || bodies.Count == 0)
            return this;

        if (bodies.Any(x => x == null))
            return this;

        Bodies ??= new List<PacketBody>();
        Bodies.AddRange(bodies);
        return this;
    }
}