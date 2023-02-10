namespace emt_sdk.Settings.EMT
{
    /// <summary>
    /// Settings for the this library and the entire device in general
    /// </summary>
    public class EMTSetting
    {
        /// <summary>
        /// Type of current device
        /// </summary>
        public DeviceTypeEnum Type { get; set; }

        /// <summary>
        /// Settings for all remote connections
        /// </summary>
        public CommunicationSettings Communication { get; set; } = new CommunicationSettings();

        /// <summary>
        /// Name of default package loaded at startup
        /// </summary>
        public string StartupPackage { get; set; }
    }
}