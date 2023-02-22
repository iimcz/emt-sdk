using emt_sdk.ScenePackage;
using System.Linq;
using System.Text.RegularExpressions;

namespace emt_sdk.Packages
{
    public partial class Element
    {
        private int[] ViewportSizes => Regex.Match(ViewportTransform, @"(\d+)x(\d+)([+-]\d+)([+-]\d+)")
            .Groups
            .Cast<Capture>()
            .Skip(1)
            .Select(x => int.Parse(x.Value))
            .ToArray();

        public Viewport Viewport
        {
            get
            {
                var sizes = ViewportSizes;
                return new Viewport(sizes[0], sizes[1], sizes[2], sizes[3]);
            }
        }
    }
}
