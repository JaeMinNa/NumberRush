using GameServer.Module.ServerManager;
using System.Net.Mime;

// Error Message는 GString으로 Client Side가 이해하기 쉬운 형태로 보낸다
// Error Code 는 Server Side에서 문제를 식별하기 위해 사용하는 약속된 값이다
public class ErrorResponseBuilder
{
    private readonly GamePacket m_gamePacket;
    private PacketHeader m_outHeaderData;
    private readonly List<PacketBody> m_outBodyData;
    private ReceiveType m_receiveType = ReceiveType.ShowMessage;
    private Messagetype m_messageType = Messagetype.Message;

    // 기본 에러 메시지는 ui_common_0158로 "서버 오류가 발생했습니다."이다
    private string m_errorMessage = "서버 오류가 발생했습니다.";
    private Enum m_contentsType = ContentsType.None;

    // 0은 sentinel value이므로 1부터 시작하도록 할 것
    private int _errorCode = 0;

    private ErrorResponseBuilder(GamePacket gamePacket, PacketHeader outHeaderData, List<PacketBody> outBodyData)
    {
        m_gamePacket = gamePacket;
        m_outHeaderData = outHeaderData;
        m_outBodyData = outBodyData;
    }

    public static ErrorResponseBuilder Make(GamePacket gamePacket, PacketHeader outHeaderData, List<PacketBody> outBodyData)
    {
        return new ErrorResponseBuilder(gamePacket, outHeaderData, outBodyData);
    }

    public ErrorResponseBuilder SetContentsType(Enum contentsType)
    {
        m_contentsType = contentsType;
        return this;
    }

    public ErrorResponseBuilder SetCode(int errorCode)
    {
        _errorCode = errorCode;
        return this;
    }

    public ErrorResponseBuilder SetMessage(string errorMessage)
    {
        m_errorMessage = errorMessage;
        return this;
    }

    public ErrorResponseBuilder SetReceiveType(ReceiveType receiveType)
    {
        m_receiveType = receiveType;
        return this;
    }

    public ErrorResponseBuilder SetMessageType(Messagetype messageType)
    {
        m_messageType = messageType;
        return this;
    }

    public async Task<Tuple<PacketState, string>> BuildAsync()
    {
        var errorCodeString = ServerUtil.MakeServerErrorData(m_gamePacket, _errorCode);
        var responseString = $"[{errorCodeString}]{m_errorMessage}";
        m_outBodyData.Add(ServerUtil.MakeBodyData(m_receiveType, m_messageType, responseString));
        m_outHeaderData = ServerUtil.MakeHeaderData(m_contentsType, false);
        var resultString = await ServerUtil.MakePacket(m_gamePacket.contentsType, m_outHeaderData, m_outBodyData);
        Console.WriteLine(resultString);
        return new Tuple<PacketState, string>(PacketState.None, resultString);
    }
}