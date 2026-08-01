using GameServer.Module.ServerManager;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Text;

namespace GameServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Server()
        {
            string account = HttpContext.Request.Form["AccountCode"];
            string data = HttpContext.Request.Form["Data"];
            string decompStr = ServerUtil.StringDecompress(data);

            if (ServerUtil.TryToObjectJson(decompStr, out ServerPacket recvPacket))
            {
                try
                {
                    var PacketResult = await ServerInterface.ProcessPacket(account, recvPacket);
                    if (PacketResult.Item1 == PacketState.None)
                    {
                        return SendSuccess(recvPacket.PacketType, PacketResult.Item1, PacketResult.Item2);
                    }
                    else
                    {
                        return SendError(recvPacket.PacketType, PacketResult.Item1, PacketResult.Item2);
                    }
                }
                catch (Exception e)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine(string.Format("Data:{0}", decompStr));
                    builder.AppendLine(string.Format("에러 메세지:{0}", e.Message));
                    builder.AppendLine(string.Format("호출스택:{0}\n", e.StackTrace));
                    builder.AppendLine(string.Format("소스코드:{0}\n", e.Source));
                    builder.AppendLine(string.Format("함수이름:{0}\n", e.TargetSite));
                    builder.AppendLine(string.Format("익셉션 발생 타입:{0}\n", e.TargetSite.MemberType));


                    if (string.IsNullOrEmpty(decompStr))
                    {
                        string errorCode = ServerUtil.MakeServerExceptionData(recvPacket.PacketType, default, 0);
                        return SendError(recvPacket.PacketType, PacketState.ServerException, errorCode);
                    }
                    else
                    {
                        ServerPacket PacketData = ServerUtil.ToObjectJson<ServerPacket>(decompStr);
                        if (string.IsNullOrEmpty(PacketData.Data))
                        {
                            string errorCode = ServerUtil.MakeServerExceptionData(recvPacket.PacketType, default, 0);
                            return SendError(recvPacket.PacketType, PacketState.ServerException, errorCode);
                        }
                        else
                        {
                            if (ServerUtil.TryToObjectJson(PacketData.Data, out GamePacket GamePacketData))
                            {
                                string errorCode = ServerUtil.MakeServerExceptionData(recvPacket.PacketType, GamePacketData.contentsType, GamePacketData.HeaderData.ContentsIndex);
                                return SendError(recvPacket.PacketType, PacketState.ServerException, errorCode);
                            }
                            else
                            {
                                string errorCode = ServerUtil.MakeServerExceptionData(recvPacket.PacketType, default, 0);
                                return SendError(recvPacket.PacketType, PacketState.ServerException, errorCode);
                            }
                        }
                    }
                }
            }
            else
            {
                return SendError(PacketType.Max, PacketState.UnknownPacket, string.Empty);
            }
        }

        private OkObjectResult SendSuccess(PacketType packetType, PacketState stateType, string data)
        {
            ServerPacket packet = new ServerPacket();
            packet.PacketType = packetType;
            packet.StateType = stateType;
            packet.Data = data;

            string compressData = ServerUtil.StringCompress(ServerUtil.ToJson(packet));
            return Ok(compressData);
        }

        private BadRequestObjectResult SendError(PacketType packetType, PacketState stateType, string data)
        {
            Console.WriteLine(data);

            ServerPacket packet = new ServerPacket();
            packet.PacketType = packetType;
            packet.StateType = stateType;
            packet.Data = data;

            string compressData = ServerUtil.StringCompress(ServerUtil.ToJson(packet));
            return BadRequest(compressData);
        }
    }
}