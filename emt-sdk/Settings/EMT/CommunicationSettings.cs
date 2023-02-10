namespace emt_sdk.Settings.EMT
{
    public class CommunicationSettings
    {
        /// <summary>
        /// Hostname of the Content Manager server
        /// </summary>
        public string ContentHostname { get; set; }

        /// <summary>
        /// Port of the Content Manager server
        /// </summary>
        public int ContentPort { get; set; } = 3917;

        /// <summary>
        /// IP used by <see cref="Events.Local.EventManager"/> for listening for incoming sensor events
        /// </summary>
        public string SensorListenIp { get; set; }

        /// <summary>
        /// IP used by <see cref="Events.Remote.InterdeviceEventRelay"/> for listening for other emt_sdk devices
        /// </summary>
        public string InterdeviceListenIp { get; set; }

        /// <summary>
        /// Port used by <see cref="Events.Local.EventManager"/> for listening for incoming sensor events and emt_sdk devices.
        /// Port is the same for both sensor and interdevice communication, connections get filtered based on their IP.
        /// </summary>
        public int SensorListenPort { get; set; } = 5000;

        /// <summary>
        /// Port used by <see cref="Events.Remote.InterdeviceEventRelay"/> for listening for other emt_sdk devices
        /// </summary>
        public int InterdeviceListenPort { get; set; } = 8920;

        /// <summary>
        /// Hostname of NTP server, can be null for default european NTP server defined in <see cref="Events.NtpSync.NtpScheduler"/> (requires outside internet connection)
        /// </summary>
        public string NtpHostname { get; set; }

        /// <summary>
        /// Whether emt_sdk should log all incoming events. This is not recommended for production, only for debugging purposes.
        /// </summary>
        public bool LogEvents { get; set; }
    }
}
