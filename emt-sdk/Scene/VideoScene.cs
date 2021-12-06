namespace emt_sdk.Scene
{
    /// <summary>
    /// Definition of video scene specific data
    /// </summary>
    public class VideoScene
    {
        /// <summary>
        /// Defines how the video should be scaled
        /// </summary>
        public enum VideoAspectRatioEnum 
        {
            /// <summary>
            /// Fits video into the viewport, adding black bars - Fit larger side
            /// </summary>
            FitInside,

            /// <summary>
            /// Fits the video into the viewport, cropping parts that don't fit - Fit smaller side
            /// </summary>
            FitOutside,

            /// <summary>
            /// Stretches the video across the entire viewport (distorts image)
            /// </summary>
            Stretch
        }

        public class VideoEvent
        {
            /// <summary>
            /// Point in time when the event should be raised in seconds
            /// </summary>
            public float Timestamp { get; set; }

            public string EventName { get; set; }
        }

        /// <summary>
        /// Gets or sets the filename of the video file to be played
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Whether the video should automatically loop
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Whether the video should start playing as soon as the scene loads
        /// </summary>
        public bool AutoStart { get; set; }

        /// <summary>
        /// Determines how the video content will be rescaled to fit the screen
        /// </summary>
        public VideoAspectRatioEnum AspectRatio { get; set; }

        /// <summary>
        /// Background color in hex, formatted as #RRGGBB (e.g. #A1FF12)
        /// </summary>
        public string BackgroundColor { get; set; }

        public VideoEvent[] VideoEvents { get; set; }
    }
}
