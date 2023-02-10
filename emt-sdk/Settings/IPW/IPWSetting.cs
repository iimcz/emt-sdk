using System.Collections.Generic;

namespace emt_sdk.Settings.IPW
{
    /// <summary>
    /// Settings for an Interactive Projection Wall
    /// </summary>
    public class IPWSetting
    {
        /// <summary>
        /// Available layouts of connected projectors
        /// </summary>
        public enum IPWOrientation
        {
            /// <summary>
            /// Two projectors placed above eachother
            /// </summary>
            Vertical,
            /// <summary>
            /// Two projectors placed next to eachother
            /// </summary>
            Horizontal,
            /// <summary>
            /// Single projector spanning the entire wall
            /// </summary>
            Single
        }

        /// <summary>
        /// Relative rendering offset (0.0 - 1.0) between projectors. Ignored in <see cref="IPWOrientation.Single"/> layout
        /// </summary>
        public float LensShift { get; set; } = 0.5f;

        /// <summary>
        /// Currently active layout
        /// </summary>
        public IPWOrientation Orientation { get; set; } = IPWOrientation.Horizontal;

        /// <summary>
        /// Display transformations for individual displays
        /// </summary>
        public List<DisplaySetting> Displays { get; set; } = new List<DisplaySetting>();
    }
}
