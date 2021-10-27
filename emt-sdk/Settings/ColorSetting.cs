namespace emt_sdk.Settings
{
    public class ColorSetting
    {
        /// <summary>
        /// Absolute saturation of image (0.0 - 1.0). Does not support HDR.
        /// </summary>
        public float Saturation { get; set; } = 1f;

        /// <summary>
        /// Absolute contrast of image (0.0 - 1.0).
        /// </summary>
        public float Contrast { get; set; } = 1f;

        /// <summary>
        /// Additive brightness of image (-1.0 - 1.0). Does not support HDR.
        /// </summary>
        public float Brightness { get; set; } = 0f;
    }
}
