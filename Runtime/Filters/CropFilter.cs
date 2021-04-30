/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Filters {

    using UnityEngine;
    using Inputs;

    /// <summary>
    /// Recorder input for recording video frames from textures with cropping.
    /// </summary>
    public sealed class CropFilter : ITextureFilter { // INCOMPLETE

        #region --Client API--
        /// <summary>
        /// Crop rect in pixel coordinates of the recorder.
        /// </summary>
        public RectInt cropRect;

        /// <summary>
        /// Crop aspect mode.
        /// </summary>
        public AspectMode aspectMode;

        /// <summary>
        /// Create a crop filter.
        /// </summary>
        /// <param name="width">Recording width.</param>
        /// <param name="height">Recording height.</param>
        public CropFilter (int width, int height) {
            this.material = new Material(Shader.Find(@"Hidden/NCPX/CropFilter"));
            this.frameSizeInv = new Vector2(1f / width, 1f/ height);
            this.cropRect = new RectInt(0, 0, width, height);
            this.aspectMode = 0;
        }

        /// <summary>
        /// Filter a frame.
        /// </summary>
        /// <param name="source">Source texture.</param>
        /// <param name="destination">Destination texture.</param>
        public void FilterFrame (Texture source, RenderTexture destination) { // INCOMPLETE
            // Offset
            var offset = Matrix4x4.Translate(Vector2.Scale(-cropRect.position, frameSizeInv));
            // Scale

            // Transform
            var forward = offset;
            var inverse = Matrix4x4.identity;
            if (!Matrix4x4.Inverse3DAffine(forward, ref inverse))
                return;
            // Blit
            material.SetMatrix("_Transform", inverse);
            Graphics.Blit(source, destination, material);
        }

        /// <summary>
        /// Dispose the filter.
        /// </summary>
        public void Dispose () => Material.Destroy(material);
        #endregion


        #region --Operations--
        private readonly Material material;
        private readonly Vector2 frameSizeInv;
        #endregion
    }
}