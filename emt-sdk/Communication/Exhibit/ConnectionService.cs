using Grpc.Core;
using Naki3D.Common.Protocol;
using System.Threading.Tasks;

namespace emt_sdk.Communication.Exhibit
{
    public class ConnectionService : Naki3D.Common.Protocol.ConnectionService.ConnectionServiceBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingResponse
            {
                Echo = request.Msg
            });
        }
    }
}
