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
        /// Types of 3D object animation
        /// </summary>
        public enum OrbitTypeEnum
        { 
            /// <summary>
            /// Orbits the camera around the object
            /// </summary>
            RotateCamera, 

            /// <summary>
            /// Spins the object around its axis
            /// </summary>
            RotateObject 
        }

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
            public string ObjectName { get; set; }

            /// <summary>
            /// Absolute coordinates in model space
            /// </summary>
            public Vector3 Position { get; set; } = new Vector3();
        }

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
            public string Text { get; set; }

            /// <summary>
            /// Action to execute on flag activation, null if no action should be performed. Flags without actions also cannot be selected.
            /// </summary>
            public string Action { get; set; }
        }

        /// <summary>
        /// Camera orbit definition
        /// </summary>
        public class Orbit
        {
            /// <summary>
            /// Origin point around which the camera rotates
            /// </summary>
            public GltfLocation Origin { get; set; }

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
        /// Type of orbit used for animation
        /// </summary>
        public OrbitTypeEnum OrbitType { get; set; }

        /// <summary>
        /// Camera orbit definition, only used when <see cref="OrbitType"/> is equal to <see cref="OrbitTypeEnum.RotateCamera"/>
        /// </summary>
        public Orbit CameraOrbit { get; set; }

        /// <summary>
        /// Time in seconds it takes to spin the object around once, only used when <see cref="OrbitType"/> is equal to <see cref="OrbitTypeEnum.RotateObject"/>
        /// </summary>
        public float ObjectRevolutionTime { get; set; }

        public FlagInteractionTypeEnum FlagInteraction { get; set; } = FlagInteractionTypeEnum.Swipe;

        /// <summary>
        /// List of displayed flags on the model. Order of flags in this list will be used to define order of selection during interaction if <see cref="FlagInteraction"/> is equal to <see cref="FlagInteractionTypeEnum.Swipe"/>.
        /// </summary>
        public List<Flag> Flags { get; set; }
    }
}
