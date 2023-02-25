using System.Net;
using System.Threading.Tasks;
using emt_sdk.Events.Local;
using emt_sdk.Settings;
using emt_sdk.Settings.EMT;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Naki3D.Common.Protocol;

namespace emt_sdk.Communication.Exhibit
{
	public class DeviceService : Naki3D.Common.Protocol.DeviceService.DeviceServiceBase
	{
        private readonly ISensorManager _sensorManager;
        private readonly IConfigurationProvider<EMTSetting> _config;

        public DeviceService(ISensorManager sensorManager, IConfigurationProvider<EMTSetting> config)
		{
            _sensorManager = sensorManager;
            _config = config;
        }

        public override Task<DeviceDescriptorResponse> GetDeviceDescriptor(Empty request, ServerCallContext context)
        {
            var response = new DeviceDescriptorResponse
            {
                DeviceType = _config.Configuration.Type.ToString(),
                Hostname = Dns.GetHostName(),
                ProtocolVersion = 2,
                FirmwareVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };

            var sensorEndpoints = _sensorManager.GetSensorEndpoints();
            response.AvailableSensors.AddRange(sensorEndpoints);

            return Task.FromResult(response);
        }

        public override Task<Empty> SetVolume(FloatValue request, ServerCallContext context)
        {
            OnVolumeChanged(request.Value);
            return Task.FromResult(new Empty());
        }
    }
}

