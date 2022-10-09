namespace emt_sdk.ScenePackage
{
    public class Viewport
    {
        public Viewport(int width, int height, int x, int y)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
