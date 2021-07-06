/* 
*   NatCorder Performance Extensions
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Inputs {

    using UnityEngine;

    /// <summary>
    /// Recorder input for recording video frames from textures with a watermark.
    /// </summary>
    public sealed class WatermarkTextureInput : ITextureInput { // DEPLOY
        
        #region --Client API--
        /// <summary>
        /// Watermark image.
        /// If `null`, no watermark will be rendered.
        /// </summary>
        public Texture watermark;

        /// <summary>
        /// Watermark display rect in pixel coordinates of the recorder.
        /// </summary>
        public RectInt rect;

        /// <summary>
        /// Watermark aspect mode.
        /// </summary>
        public AspectMode aspectMode;

        /// <summary>
        /// Create a watermark texture input.
        /// </summary>
        /// <param name="input">Backing texture input to receive watermarked frames.</param>
        public WatermarkTextureInput (ITextureInput input) {
            this.input = input;
            this.material = new Material(Shader.Find(@"Hidden/NCPX/WatermarkFilter"));
            this.frameSizeInv = new Vector2(1f / input.frameSize.width, 1f / input.frameSize.height);
            this.rect = new RectInt(0, 0, input.frameSize.width, input.frameSize.height);
            this.aspectMode = 0;
        }

        /// <summary>
        /// Commit a video frame from a texture.
        /// </summary>
        /// <param name="texture">Source texture.</param>
        /// <param name="timestamp">Frame timestamp in nanoseconds.</param>
        public void CommitFrame (Texture texture, long timestamp) {
            if (watermark) {
                // Base
                var result = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(texture, result);
                // Offset
                var offset = Matrix4x4.Translate(Vector2.Scale(rect.position, frameSizeInv));
                // Aspect
                var dstAspect = (float)result.width / result.height;
                var watermarkAspect = (float)watermark.width / watermark.height;
                var aspect = Matrix4x4.Scale(new Vector3(1f, dstAspect, 1f)) * Matrix4x4.Scale(new Vector3(1f, 1f / watermarkAspect, 1f));
                // Scale
                var axisScale = Vector2.Scale(rect.size, frameSizeInv);
                var scaleFactor = aspectMode == AspectMode.Fill ? Mathf.Max(axisScale.x, axisScale.y) : Mathf.Min(axisScale.x, axisScale.y);
                var scale = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1f));
                // Transform
                var forward = offset * aspect * scale;
                var inverse = Matrix4x4.identity;
                if (!Matrix4x4.Inverse3DAffine(forward, ref inverse))
                    return;
                // Blit
                material.SetMatrix("_Transform", inverse);
                Graphics.Blit(watermark, result, material);
                // Commit
                input.CommitFrame(result, timestamp);
                RenderTexture.ReleaseTemporary(result);
            }
            else
                input.CommitFrame(texture, timestamp);
        }

        /// <summary>
        /// Stop recorder input and release resources.
        /// </summary>
        public void Dispose () {
            input.Dispose();
            Material.Destroy(material);
        }
        #endregion


        #region --Operations--
        private readonly ITextureInput input;
        private readonly Material material;
        private readonly Vector2 frameSizeInv;
        (int, int) ITextureInput.frameSize => input.frameSize;
        #endregion
    }
}