namespace emt_sdk.Packages
{
    public partial class CanvasDimensions
    {
        public static bool IsNullOrEmpty(CanvasDimensions dimensions)
        {
            if (dimensions == null) return true;
            if (dimensions.Width == 0 && dimensions.Height == 0) return true;
            return false;
        }
    }
}