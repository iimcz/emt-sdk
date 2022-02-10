using Naki3D.Common.Protocol;
using System.Collections.Generic;

namespace emt_sdk.Scene
{
    /// <summary>
    /// Definition of 3D scene specific data
    /// </summary>
    public class GltfObject
    {

        /// <summary>
        /// Defines how the user interacts with flags
        /// </summary>
        public enum FlagInteractionTypeEnum
        {
            /// <summary>
            /// Allows the user to swipe left and right to select previous/next flag
            /// </summary>
            Swipe,

            /// <summary>
            /// Allows user to point at the desired flag.
            /// </summary>
            //TODO: NOT CURRENTLY IMPLEMENTED
            Point
        }

        /// <summary>
        /// Location inside a GLTF Model
        /// </summary>
        public class GltfLocation
        {
            /// <summary>
            /// Name of a GLTF object to be used as a poistion. If null <see cref="Position"/> is used instead.
            /// </summary>
            // TODO: Have a list of names that are reserved in docs
            public string ObjectName { get; set; }

            /// <summary>
            /// Offset coordinates in model space relative to <see cref="ObjectName"/> or scene root if it is <see langword="null"/>.
            /// </summary>
            public Vector3 Offset { get; set; } = new Vector3();
        }
        
        /// <summary>
        /// Base interface for all animations
        /// </summary>
        public interface ICameraAnimation { }

        /// <summary>
        /// Description flag in model
        /// </summary>
        public class Flag
        {
            /// <summary>
            /// Location of flag
            /// </summary>
            public GltfLocation Location { get; set; }

            /// <summary>
            /// Displayed text
            /// </summary>
            // TODO: Add link to tmpro
            public string Text { get; set; }

            /// <summary>
            /// Action to execute on flag activation, <see langword="null"/> if no action should be performed.
            /// </summary>
            public string ActivatedAction { get; set; }

            /// <summary>
            /// Action to execute on flag selection, <see langword="null"/> if no action should be performed.
            /// </summary>
            public string SelectedAction { get; set; }

            public string ForegroundColor { get; set; }
            public string BackgroundColor { get; set; }
            public string StalkColor { get; set; }

            // TODO: Background material adjustement

            /// <summary>
            /// Whether this flag can be selected. Value of <see langword="false"/> disables both <see cref="ActivatedAction"/> and <see cref="SelectedAction"/>.
            /// </summary>
            public bool CanSelect { get; set; }
        }

        /// <summary>
        /// Camera cylinder orbit definition
        /// </summary>
        public class OrbitAnimation : ICameraAnimation
        {
            /// <summary>
            /// Origin point around which the camera rotates
            /// </summary>
            public GltfLocation Origin { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public GltfLocation LookAt { get; set; }

            /// <summary>
            /// Distance of the camera from the object, radius of rotation path
            /// </summary>
            public float Distance { get; set; }

            /// <summary>
            /// Height of the camera relative to the object
            /// </summary>
            public float Height { get; set; }

            /// <summary>
            /// Time in seconds it takes to spin around the object once
            /// </summary>
            public float RevolutionTime { get; set; }
        }

        /// <summary>
        /// Name of the input GLTF file
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Name of the skybox cubemap file, white if no file is specified
        /// </summary>
        public string Skybox { get; set; }

        /// <summary>
        /// Tint applied to the skybox, background color if no skybox is specified
        /// </summary>
        public string SkyboxTint { get; set; }
        
        public ICameraAnimation CameraAnimation { get; set; }

        public FlagInteractionTypeEnum FlagInteraction { get; set; } = FlagInteractionTypeEnum.Swipe;

        /// <summary>
        /// List of displayed flags on the model. Order of flags in this list will be used to define order of selection during interaction if <see cref="FlagInteraction"/> is equal to <see cref="FlagInteractionTypeEnum.Swipe"/>.
        /// </summary>
        public List<Flag> Flags { get; set; }
    }
}
