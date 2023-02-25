namespace Naki3D.Common.Protocol
{
    public static partial class DeviceService
    {
        public abstract partial class DeviceServiceBase
        {
            public delegate void VolumeChangeHandler(float volume);
            public event VolumeChangeHandler VolumeChanged;

            protected void OnVolumeChanged(float volume)
            {
                VolumeChanged?.Invoke(volume);
            }
        }
    }
}