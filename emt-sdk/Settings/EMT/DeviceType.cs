namespace emt_sdk.Settings.EMT
{
    public enum DeviceTypeEnum
    {
        DEVICE_TYPE_UNKNOWN = 0,
        DEVICE_TYPE_IPW = 1,
        DEVICE_TYPE_PGE = 2
    }

    static class DeviceTypeEnumExtensions
    {
        public static string GetName(this DeviceTypeEnum type)
        {
            switch (type)
            {
                default:
                case DeviceTypeEnum.DEVICE_TYPE_UNKNOWN:
                    return "unknown";
                case DeviceTypeEnum.DEVICE_TYPE_IPW:
                    return "ipw";
                case DeviceTypeEnum.DEVICE_TYPE_PGE:
                    return "pge";
            }
        }
    }
}
