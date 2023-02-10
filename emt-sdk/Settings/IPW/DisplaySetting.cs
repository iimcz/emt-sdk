namespace emt_sdk.Settings.IPW
{
    public class DisplaySetting
    {
        /// <summary>
        /// Id of display as defined in Unity. Should match display numbers in Windows / xorg.
        /// </summary>
        public int DisplayId { get; set; }

        /// <summary>
        /// Color transformation settings
        /// </summary>
        public ColorSetting Color { get; set; } = new ColorSetting();

        /// <summary>
        /// Keystone transformation settings
        /// </summary>
        public SkewSetting Skew { get; set; } = new SkewSetting();

        /// <summary>
        /// Relative percantage of image (0.0 - 1.0) that should be overlayed in the middle. This is used for a smoother transition in the middle of the IPW.
        /// </summary>
        public float CrossOver { get; set; }
    }
}
