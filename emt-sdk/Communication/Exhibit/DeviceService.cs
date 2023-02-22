using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Naki3D.Common.Protocol;

namespace emt_sdk.Communication.Exhibit
{
	public class DeviceService : Naki3D.Common.Protocol.DeviceService.DeviceServiceBase
	{
        public delegate void VolumeChangeHandler(float volume);
        public event VolumeChangeHandler OnVolumeChanged;

        private readonly ISensorService _sensor;

        public DeviceService(ISensorService sensor)
		{
            _sensor = sensor;
		}

        public override Task<DeviceDescriptorResponse> GetDeviceDescriptor(Empty request, ServerCallContext context)
        {
            // TODO: Sensor integration
            throw new NotImplementedException();
        }

        public override Task<Empty> SetVolume(FloatValue request, ServerCallContext context)
        {
            OnVolumeChanged?.Invoke(request.Value);
            return Task.FromResult(new Empty());
        }
    }
}

