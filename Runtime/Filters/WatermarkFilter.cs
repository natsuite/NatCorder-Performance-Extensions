/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Filters {

    using UnityEngine;
    using Inputs;

    /// <summary>
    /// Recorder input for recording video frames from textures with a watermark.
    /// </summary>
    public sealed class WatermarkFilter : ITextureFilter { // DEPLOY
        
        #region --Client API--
        /// <summary>
        /// Watermark image.
        /// If `null`, no watermark will be rendered.
        /// </summary>
        public Texture watermark;

        /// <summary>
        /// Watermark display rect in pixel coordinates of the recorder.
        /// </summary>
        public RectInt displayRect;

        /// <summary>
        /// Watermark aspect mode.
        /// </summary>
        public AspectMode aspectMode;

        /// <summary>
        /// Create a watermark filter.
        /// </summary>
        /// <param name="width">Recording width.</param>
        /// <param name="height">Recording height.</param>
        public WatermarkFilter (int width, int height) {
            this.material = new Material(Shader.Find(@"Hidden/NCPX/WatermarkFilter"));
            this.frameSizeInv = new Vector2(1f / width, 1f / height);
            this.displayRect = new RectInt(0, 0, width, height);
            this.aspectMode = 0;
        }

        /// <summary>
        /// Filter a frame.
        /// </summary>
        /// <param name="source">Source texture.</param>
        /// <param name="destination">Destination texture.</param>
        public void FilterFrame (Texture source, RenderTexture destination) {
            // Base
            Graphics.Blit(source, destination);
            if (!watermark)
                return;
            // Offset
            var offset = Matrix4x4.Translate(Vector2.Scale(displayRect.position, frameSizeInv));
            // Aspect
            var dstAspect = (float)destination.width / destination.height;
            var watermarkAspect = (float)watermark.width / watermark.height;
            var aspect = Matrix4x4.Scale(new Vector3(1f, dstAspect, 1f)) * Matrix4x4.Scale(new Vector3(1f, 1f / watermarkAspect, 1f));
            // Scale
            var axisScale = Vector2.Scale(displayRect.size, frameSizeInv);
            var scaleFactor = aspectMode == AspectMode.Fill ? Mathf.Max(axisScale.x, axisScale.y) : Mathf.Min(axisScale.x, axisScale.y);
            var scale = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1f));
            // Transform
            var forward = offset * aspect * scale;
            var inverse = Matrix4x4.identity;
            if (!Matrix4x4.Inverse3DAffine(forward, ref inverse))
                return;
            // Blit
            material.SetMatrix("_Transform", inverse);
            Graphics.Blit(watermark, destination, material);
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