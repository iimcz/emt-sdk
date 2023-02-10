using Grpc.Core;
using Naki3D.Common.Protocol;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace emt_sdk.Diagnostics
{
    public class DiagnosticService : Naki3D.Common.Protocol.DiagnosticService.DiagnosticServiceBase
    {
        private readonly IDiagnosticLogProvider _log;
        //private readonly ISystemServiceManager _systemService;

        public DiagnosticService(IDiagnosticLogProvider log)//, ISystemServiceManager systemService)
        {
            _log = log;
            //_systemService = systemService;
        }

        public override Task<LogsResponse> GetLogs(LogsRequest request, ServerCallContext context)
        {
            var logText = _log.ReadMainLog();
            var requestedLines = logText.Reverse().Take(request.Lines).Reverse();

            var response = new LogsResponse();
            response.LogLine.AddRange(requestedLines);

            return Task.FromResult(response);
        }

        public override Task<ServiceStatusResponse> GetServiceStatus(ServiceStatusRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }
}
