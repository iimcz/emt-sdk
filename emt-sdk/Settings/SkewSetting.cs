using Naki3D.Common.Protocol;

namespace emt_sdk.Settings
{
    /// <summary>
    /// Describes screen keystone transformation quad
    /// Coordinate space starts at -1, -1 for the bottom left corner
    /// And ends at 1, 1 for the rop right corner
    /// </summary>
    public class SkewSetting
    {
        public Vector2 TopLeft { get; set; } = new Vector2
        {
            X = -1,
            Y = 1
        };

        public Vector2 TopRight { get; set; } = new Vector2
        {
            X = 1,
            Y = 1
        };

        public Vector2 BottomLeft { get; set; } = new Vector2
        {
            X = -1,
            Y = -1,
        };

        public Vector2 BottomRight { get; set; } = new Vector2
        {
            X = 1,
            Y = -1
        };
    }
}
