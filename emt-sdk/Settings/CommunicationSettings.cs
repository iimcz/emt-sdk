namespace emt_sdk.Settings
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
        /// IP used by <see cref="Events.Local.EventManager"/> for listening for other emt_sdk devices
        /// </summary>
        public string InterdeviceListenIp { get; set; }

        /// <summary>
        /// Port used by <see cref="Events.Local.EventManager"/> for listening for incoming sensor events and emt_sdk devices.
        /// Port is the same for both sensor and interdevice communication, connections get filtered based on their IP.
        /// </summary>
        public int EventListenPort { get; set; } = 5000;
    }
}
