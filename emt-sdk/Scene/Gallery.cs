using Naki3D.Common.Protocol;

namespace emt_sdk.Scene
{
    /// <summary>
    /// Definition of gallery specific data
    /// </summary>
    public class Gallery
    {
        public class GalleryLayout { }

        /// <summary>
        /// Layout where images are laid in a single line, wrapping after reaching the end of the list
        /// </summary>
        public class ListLayout : GalleryLayout
        {
            /// <summary>
            /// Amount of images visible at any given time
            /// </summary>
            public int VisibleImages { get; set; }

            /// <summary>
            /// Percentage of screen space used between individual images (not on screen border) from 0.0 to 1.0
            /// </summary>
            public float Spacing { get; set; }

            /// <summary>
            /// List of displayed images
            /// </summary>
            public GalleryImage[] Images { get; set; }
        }

        /// <summary>
        /// Layout where images are aligned in a grid, wrapping?
        /// </summary>
        public class GridLayout : GalleryLayout
        {
            /// <summary>
            /// Width of the grid
            /// </summary>
            public int Width { get; set; }

            /// <summary>
            /// Height of the grid
            /// </summary>
            public int Height { get; set; }

            /// <summary>
            /// Percentage of screen space used between rows of the grid from 0.0 to 1.0
            /// </summary>
            public float VerticalSpacing { get; set; }

            /// <summary>
            /// Percentage of screen space used between columns of the grid from 0.0 to 1.0
            /// </summary>
            public float HorizontalSpacing { get; set; }

            // TODO: Wrap: Each row is individual list, desync is allowed
            public GalleryImage[,] Images { get; set; }
        }

        /// <summary>
        /// Single gallery image
        /// </summary>
        public class GalleryImage
        {
            /// <summary>
            /// Image file path
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Executed action when image is activated (specifically interacted with)
            /// </summary>
            public string ActivatedAction { get; set; }

            /// <summary>
            /// Executed action when image is selected (either through <see cref="AutoScroll"/> or through manual interacion)
            /// </summary>
            public string SelectedAction { get; set; }
        }

        /// <summary>
        /// Layout of the gallery images
        /// </summary>
        public enum GalleryLayoutEnum
        {
            Grid,
            List
        }

        /// <summary>
        /// Active layout for this gallery
        /// </summary>
        public GalleryLayoutEnum LayoutType { get; set; }
        
        /// <summary>
        /// Parameters of the specific layout in <see cref="LayoutType"/>
        /// </summary>
        public GalleryLayout Layout { get; set; }

        // TODO: Add to docs that all sizes are relative to the entire screen space (including multiple IPWs)
        /// <summary>
        /// Percentage of screen space along the edges used as padding from 0.0 to 1.0 (e.g. (0.2, 0.1) would be 20% horizontally and 10% vertically).
        /// </summary>
        public Vector2Data Padding { get; set; }

        // TODO: Mention in docs that different animation could be added, also mention custom animations
        /// <summary>
        /// How long should the delay between automatic scrolling steps in seconds. Value of 0 disables automatic scrolling.
        /// </summary>
        public float ScrollDelay { get; set; }

        /// <summary>
        /// How long the scroll animation itself should be in seconds (default is 0.3s)
        /// </summary>
        public float SlideAnimationLength { get; set; }

        /// <summary>
        /// Background color in hex, formatted as #RRGGBB (e.g. #A1FF12)
        /// </summary>
        public string BackgroundColor { get; set; }
    }
}
