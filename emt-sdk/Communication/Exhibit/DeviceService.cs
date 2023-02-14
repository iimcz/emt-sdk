using System;
using System.Threading.Tasks;
using emt_sdk.Device;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Naki3D.Common.Protocol;

namespace emt_sdk.Communication.Exhibit
{
	public class DeviceService : Naki3D.Common.Protocol.DeviceService.DeviceServiceBase
	{
        private readonly IDeviceManagementService _deviceManagement;

        public DeviceService(IDeviceManagementService deviceManagement)//, ISensorService sensor)
		{
            _deviceManagement = deviceManagement;
		}

        public override Task<DeviceDescriptorResponse> GetDeviceDescriptor(Empty request, ServerCallContext context)
        {
            // TODO: Sensor integration
            throw new NotImplementedException();
        }

        public override Task<Empty> RebootDevice(Empty request, ServerCallContext context)
        {
            _deviceManagement.Reboot();
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> ShutdownDevice(Empty request, ServerCallContext context)
        {
            _deviceManagement.Shutdown();
            return Task.FromResult(new Empty());
        }
    }
}

