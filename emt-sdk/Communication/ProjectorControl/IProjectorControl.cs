namespace emt_sdk.Communication.ProjectorControl
{
    /// <summary>
    /// Provides control for all connected projectors.
    /// </summary>
    public interface IProjectorControl
    {
        /// <summary>
        /// Turns all connected projectors on.
        /// </summary>
        void PowerOn();

        /// <summary>
        /// Turns all connected projectors off.
        /// </summary>
        void PowerOff();
    }
}
