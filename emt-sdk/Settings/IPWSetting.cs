using System.Collections.Generic;

namespace emt_sdk.Settings
{
    public class IPWSetting
    {
        public enum IPWOrientation
        {
            Vertical,
            Horizontal,
            Single
        }

        public float LensShift { get; set; } = 0.5f;
        public IPWOrientation Orientation { get; set; }

        public List<DisplaySetting> Displays { get; set; } = new List<DisplaySetting>();
    }
}
