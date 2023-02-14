using System;
namespace emt_sdk.Device
{
	public interface IDeviceManagementService
	{
		void Reboot();
		void Shutdown();
	}
}

