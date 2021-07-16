namespace emt_sdk.Settings
{
    public class IPWSetting
    {
        public float LensShift { get; set; }
        public ColorSetting TopScreenColor { get; set; } = new ColorSetting();
        public ColorSetting BottomScreenColor { get; set; } = new ColorSetting();
        public SkewSetting TopScreenSkew { get; set; } = new SkewSetting();
        public SkewSetting BottomScreenSkew { get; set; } = new SkewSetting();
    }
}
