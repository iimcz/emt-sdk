namespace emt_sdk.Settings
{
    public class ColorSetting
    {
        public class Color
        {
            public float R { get; set; } = 1f;
            public float G { get; set; } = 1f;
            public float B { get; set; } = 1f;
        }
    
        /// <summary>
        /// Absolute saturation of image (0.0 - 1.0). Does not support HDR.
        /// </summary>
        public float Saturation { get; set; } = 1f;

        /// <summary>
        /// Absolute contrast of image (0.0 - 1.0).
        /// </summary>
        public float Contrast { get; set; } = 1f;

        /// <summary>
        /// Multiplicative brightness of image (0.0 - 1.0). IPW does not support HDR.
        /// </summary>
        public Color Brightness { get; set; } = new Color();
    }
}
