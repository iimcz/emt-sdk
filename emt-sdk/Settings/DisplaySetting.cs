namespace emt_sdk.Settings
{
    public class DisplaySetting
    {
        public int DisplayId { get; set; }
        public ColorSetting Color { get; set; } = new ColorSetting();
        public SkewSetting Skew { get; set; } = new SkewSetting();
    }
}
